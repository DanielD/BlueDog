using BlueDog.DataProvider;
using BlueDog.Models;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog
{
    /// <summary>
    /// A base class for certain responses.
    /// </summary>
    public class UserServiceResult
    {
        /// <summary>
        /// Initializases the result.
        /// </summary>
        /// <param name="result"></param>
        public UserServiceResult(BlueDogResult result)
        {
            Result = result;
        }

        /// <summary>
        /// An potentially updated Jwt (future)
        /// </summary>
        public string Jwt { get; set; }
        public BlueDogResult Result { get; set; }
    }

    /// <summary>
    /// The result from a login attempt.
    /// </summary>
    public class LoginResult : UserServiceResult
    {
        public LoginResult( BlueDogResult result ) : base(result) { }

        /// <summary>
        /// The logged-in user, if successful. No hashed password or salt returned.
        /// </summary>
        public UserData User { get; set; }
    }

    /// <summary>
    /// The result from a get-user type call
    /// </summary>
    public class UserResult : UserServiceResult
    {
        public UserResult(BlueDogResult result) : base(result) { }

        /// <summary>
        /// The logged-in user, if successful. No hashed password or salt returned.
        /// </summary>
        public UserData User { get; set; }
    }

    /// <summary>
    /// The result from a start-reset password
    /// </summary>
    public class StartResetPasswordResult : UserServiceResult
    {
        public StartResetPasswordResult(BlueDogResult result) : base(result) { }

        /// <summary>
        /// The key that was generated for a reset password. This would be sent to the user in an email. 
        /// Note: sending of email isn't implemented here.
        /// </summary>
        public string ResetPasswordKey { get; set; }
    }

    /// <summary>
    /// Result from starting a reset email result.
    /// </summary>
    public class StartResetEmailResult : UserServiceResult
    {
        public StartResetEmailResult(BlueDogResult result) : base(result) { }
        public string ResetEmailResult { get; set; }
    }


    /// <summary>
    /// The entry point for User authentication services
    /// </summary>
    public class UserServices
    {
        public IDataProvider DataProvider { get; set; }
        public UserServices( IDataProvider dataProvider = null)
        {
            DataProvider = dataProvider;
        }


        /// <summary>
        /// Registers a new user
        /// </summary>
        /// <param name="emailAddress">The email address of the registrant</param>
        /// <param name="password">Their desired password</param>
        /// <param name="configuration">General configuration</param>
        /// <returns></returns>
        public async Task<BlueDogResult> Register(string emailAddress, string password, ServerConfiguration configuration)
        {
            if (String.IsNullOrWhiteSpace(emailAddress))
                throw new ArgumentNullException("emailAddress");

            if (String.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("password");

            if (configuration == null)
                throw new ArgumentNullException("configuration");


            // Get the user based on the email address
            User user = await DataProvider.GetUserByEmail(emailAddress);

            // != null means record already used. (DocumentDB doesn't have unique fields)
            if (user != null)
                return BlueDogResult.EmailInUse;

            // Calls convenience extension methods to initialize
            // new user and set encrypted password with salt.
            user = Users.CreateNewUser(emailAddress).
            SetPassword(password);

            return await DataProvider.SaveUser(user);
        }

        /// <summary>
        /// Logs the user on, returns a JWT
        /// </summary>
        /// <param name="email">The email address of the person attempting to log on</param>
        /// <param name="password">The login password</param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<LoginResult> Login(string email, string password, ServerConfiguration configuration)
        {
            User user = await DataProvider.GetUserByEmail(email);

            if (user == null)
                return new LoginResult( BlueDogResult.NoSuchUser);

            var secret = configuration.JwtSecretKey;

            // Check to see if the password matches
            bool passwordMatches = Users.PasswordMatches(user, password);

            if ( !passwordMatches)
                return new LoginResult (BlueDogResult.BadPassword);

            // It matches, so create a jwt token, good for one hour
            string jwt = Tokens.TokenForUser(user, DateTime.Now.AddHours(1.0), secret);

            return new LoginResult (BlueDogResult.Ok) { Jwt = jwt, User = new UserData(user) };
        }

        /// <summary>
        /// Gets the current user
        /// </summary>
        /// <param name="jwt"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<UserResult> GetCurrentUser(string jwt, ServerConfiguration configuration)
        {
            if (jwt == null)
                return new UserResult(BlueDogResult.BadToken);

            var secret = configuration.JwtSecretKey;

            // Get the data out of the jwt.
            TokenData token = Tokens.DecodeToken(jwt, secret);

            // verify the token
            if (token == null)
                return new UserResult(BlueDogResult.BadToken);

            // check expiration
            if ( Tokens.PayloadExpired (token))
                return new UserResult(BlueDogResult.ExpiredToken);

            // TODO future: check IP address

            // Get the user
            User user = await DataProvider.GetUserById(token.userid);

            if (user == null)
                return new UserResult(BlueDogResult.NoSuchUser);

            // user data is sanitized.
            return new UserResult(BlueDogResult.Ok) { Jwt = jwt, User = new UserData( user )};
        }

        /// <summary>
        /// Validates an email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="validationKey"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<BlueDogResult> ValidateEmail( string emailAddress, string validationKey, ServerConfiguration configuration)
        {
            User user = await DataProvider.GetUserByEmailValidation(validationKey);

            if (user == null)
                return BlueDogResult.NoSuchUser;

            if (user.emailValidationCode == null || user.emailValidationCode.Equals(validationKey) == false)
                return BlueDogResult.InvalidEmailKey;

            if (user.emailValidationExpires == null || user.emailValidationExpires.Value < DateTime.Now)
                return BlueDogResult.EmailValidationKeyExpired;

            user.emailValidationCode = null;
            user.emailValidated = true;

            return await DataProvider.SaveUser(user);
        }

        /// <summary>
        /// Starts the password reset
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<StartResetPasswordResult> StartResetPassword(string emailAddress,ServerConfiguration configuration)
        {
            User user = await DataProvider.GetUserByEmail(emailAddress);

            if (user == null)
                return new StartResetPasswordResult(BlueDogResult.NoSuchUser);

            // generate a new password reset code and save it
            // it is good for only so long.
            user.resetPasswordCode = Guid.NewGuid().ToString();
            user.resetPasswordExpires = DateTime.Now.AddMinutes(configuration.PasswordResetDurationMinutes);

            var result = await DataProvider.SaveUser(user);

            if (result != BlueDogResult.Ok)
                return new StartResetPasswordResult(result);

            // return the reset key
            return new StartResetPasswordResult(BlueDogResult.Ok) { ResetPasswordKey = user.resetPasswordCode };
        }

        /// <summary>
        /// Completes the reset of a password.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="validationKey"></param>
        /// <param name="password"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<BlueDogResult> CompleteResetPassword(string emailAddress, string validationKey, string password, ServerConfiguration configuration)
        {
            User user = await DataProvider.GetUserByEmail(emailAddress);

            if (user == null)
                return BlueDogResult.NoSuchUser;

            if (user.resetPasswordCode == null || user.resetPasswordCode.Equals(validationKey) == false)
                return BlueDogResult.InvalidPasswordValidationKey;

            if (user.resetPasswordExpires == null || user.resetPasswordExpires.Value < DateTime.Now)
                return BlueDogResult.PasswordValidationKeyExpired;

            user.resetPasswordCode = null;
            user.resetPasswordExpires = null;

            // extension method that automatically hashes
            // user's password
            user.SetPassword(password);

            return await DataProvider.SaveUser(user);
        }

        /// <summary>
        /// Starts email change by setting a new change email token in the user's account.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<StartResetPasswordResult> StartChangeEmail(string emailAddress, ServerConfiguration configuration)
        {
            User user = await DataProvider.GetUserByEmail(emailAddress);

            // create a new change email code
            user.emailValidationCode = Guid.NewGuid().ToString();
            user.emailValidationExpires = DateTime.Now.AddMinutes(configuration.PasswordResetDurationMinutes);

            var result = await DataProvider.SaveUser(user);

            if (result != BlueDogResult.Ok)
                return new StartResetPasswordResult(result);

            return new StartResetPasswordResult(BlueDogResult.Ok) { ResetPasswordKey = user.resetPasswordCode };
        }

        /// <summary>
        /// Update an email address
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <param name="validationKey"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public async Task<BlueDogResult> UpdateEmail(string jwt, string newEmailAddress, string validationKey, ServerConfiguration configuration)
        {
            // user must be logged in.

            // TODO: add helper function to validate jwt
            var secret = configuration.JwtSecretKey;

            TokenData token = Tokens.DecodeToken(jwt, secret);

            if (Tokens.PayloadExpired(token))
            {
                return BlueDogResult.ExpiredToken;
            }

            User user = await DataProvider.GetUserById(token.userid);

            // was there a user
            if (user == null)
                return BlueDogResult.NoSuchUser;

            // is the validation code good, or was a validation even started?
            if (user.emailValidationCode == null || user.emailValidationCode.Equals(validationKey) == false)
                return BlueDogResult.InvalidEmailKey;

            // All's good. Update email
            user.emailValidationCode = null;
            user.emailValidated = true;
            user.email = newEmailAddress;

            return await DataProvider.SaveUser(user);
        }
    }
}
