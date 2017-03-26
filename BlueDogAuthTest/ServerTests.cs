using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlueDog;
using BlueDog.Models;
using System.Threading.Tasks;
using BlueDog.DataProvider;
using PCLMock;

namespace BlueDogTest
{
    [TestClass]
    public class ServerTests
    {
        DataProviderMock DataProviderMock { get; set; }

        [TestInitialize]
        public void Setup()
        {
            DataProviderMock = new DataProviderMock();
        }


        [TestMethod]
        public void RegisterUserEmailUsed()
        {
            ServerConfiguration configuration = new ServerConfiguration
            {
                JwtSecretKey = "secret"
            };

            BlueDogResult res = BlueDogResult.Ok;

            DataProviderMock.When(x => x.GetUserByEmail(It.IsAny<string>())).Return(Task.FromResult<User>(new User()));
            UserServices userServices = new UserServices(DataProviderMock);

            Task.Run(async () =>
                {
                    res = await userServices.Register("abcd@gmail.com", "password123", configuration);
                }).Wait();

            DataProviderMock.Verify(x => x.GetUserByEmail("abcd@gmail.com")).WasCalledExactlyOnce();
            Assert.AreEqual(res, BlueDogResult.EmailInUse);

            Console.WriteLine("Done");

        }

        [TestMethod]
        public void RegisterUserEmailUnused()
        {
            ServerConfiguration configuration = new ServerConfiguration
            {
                JwtSecretKey = "secret"
            };

            BlueDogResult res = BlueDogResult.Ok;

            DataProviderMock.When(x => x.GetUserByEmail(It.IsAny<string>())).Return(Task.FromResult<User>(null));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));
            UserServices userServices = new UserServices(DataProviderMock);

            Task.Run(async () =>
            {
                res = await userServices.Register("abcd@gmail.com", "password123", configuration);
            }).Wait();

            DataProviderMock.Verify(x => x.SaveUser(It.IsAny<User>())).WasCalledExactlyOnce();
            DataProviderMock.Verify(x => x.SaveUser(It.IsAny<User>())).WasCalledExactlyOnce();
            Assert.AreEqual(res, BlueDogResult.Ok);

            Console.WriteLine("Done");

        }


        [TestMethod]
        public void LoginTestAndGetUser()
        {
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret"
            };

            LoginResult res = null;
            UserResult res2 = null;

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            DataProviderMock.When(x => x.GetUserByEmail(It.IsAny<string>())).Return(Task.FromResult<User>(tmpUser));

            UserServices userServices = new UserServices(DataProviderMock);

            Task.Run(async () =>
            {
                res = await userServices.Login("abcd@gmail.com", "password123", configuration);
            }).Wait();


            Assert.AreEqual(res.Result, BlueDogResult.Ok);
            Assert.IsNotNull(res.Jwt);
            Assert.IsNotNull(res.User);

            DataProviderMock.Verify(x => x.GetUserByEmail("abcd@gmail.com")).WasCalledExactlyOnce();
        }

        [TestMethod]
        public void LoginTestAndGetUserNotFound()
        {
            ServerConfiguration configuration = new ServerConfiguration();

            LoginResult res = null;
            UserResult res2 = null;

            DataProviderMock.When(x => x.GetUserByEmail(It.IsAny<string>())).Return(Task.FromResult<User>(null));

            UserServices userServices = new UserServices(DataProviderMock);

            Task.Run(async () =>
            {
                res = await userServices.Login("abcd@gmail.com", "password123", configuration);
            }).Wait();


            Assert.AreEqual(res.Result, BlueDogResult.NoSuchUser);
        }

        [TestMethod]
        public void TestGetCurrentUserLoggedIn()
        {
            string id = "1234";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(1), "secret");

            DataProviderMock.When(x => x.GetUserById(id)).Return(Task.FromResult<User>(tmpUser));

            UserServices userServices = new UserServices(DataProviderMock);
            UserResult res = null;

            Task.Run(async () =>
            {
                res = await userServices.GetCurrentUser(jwt, configuration);
            }).Wait();

            Assert.AreEqual(res.Result, BlueDogResult.Ok);


        }

        [TestMethod]
        public void TestGetCurrentUserNoToken()
        {
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            UserServices userServices = new UserServices(DataProviderMock);
            UserResult res = null;

            Task.Run(async () =>
            {
                res = await userServices.GetCurrentUser(null, configuration);
            }).Wait();

            Assert.AreEqual(res.Result, BlueDogResult.BadToken);
        }

        [TestMethod]
        public void TestGetCurrentUserTokenExpired()
        {
            string id = "1234";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserById(id)).Return(Task.FromResult<User>(tmpUser));

            UserServices userServices = new UserServices(DataProviderMock);
            UserResult res = null;

            Task.Run(async () =>
            {
                res = await userServices.GetCurrentUser(jwt, configuration);
            }).Wait();

            Assert.AreEqual(res.Result, BlueDogResult.ExpiredToken);


        }

        [TestMethod]
        public void ValidateEmail()
        {
            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.emailValidationCode = validationCode;
            tmpUser.emailValidationExpires = DateTime.Now.AddHours(1);

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmailValidation(validationCode)).Return(Task.FromResult<User>(tmpUser));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.ValidateEmail("abcd@gmail.com", validationCode, configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.Ok);
        }

        [TestMethod]
        public void ValidateEmailBadKey()
        {
            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.emailValidationCode = "othervalue";
            tmpUser.emailValidationExpires = DateTime.Now.AddHours(1);

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmailValidation(validationCode)).Return(Task.FromResult<User>(tmpUser));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.ValidateEmail("abcd@gmail.com", validationCode, configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.InvalidEmailKey);
        }

        [TestMethod]
        public void ValidateEmailValidationExpired()
        {
            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.emailValidationCode = validationCode;
            tmpUser.emailValidationExpires = DateTime.Now.AddHours(-1);

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmailValidation(validationCode)).Return(Task.FromResult<User>(tmpUser));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.ValidateEmail("abcd@gmail.com", validationCode, configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.EmailValidationKeyExpired);
        }

        [TestMethod]
        public void ValidateEmailNotSetForValidation()
        {
            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.emailValidationCode = null;
            tmpUser.emailValidationExpires = null;

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmailValidation(validationCode)).Return(Task.FromResult<User>(tmpUser));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.ValidateEmail("abcd@gmail.com", validationCode, configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.InvalidEmailKey);
        }

        [TestMethod]
        public void ValidateEmailBadEmail()
        {
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration() { JwtSecretKey = "secret" };

            DataProviderMock.When(x => x.GetUserByEmailValidation(validationCode)).Return(Task.FromResult<User>(null));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.ValidateEmail("abcd@gmail.com", validationCode, configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.NoSuchUser);
        }

        [TestMethod]
        public void StartPasswordReset()
        {
            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret",
                PasswordResetDurationMinutes = 30
            };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmail("abcd@gmail.com")).Return(Task.FromResult<User>(tmpUser));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));

            UserServices userServices = new UserServices(DataProviderMock);
            StartResetPasswordResult res = null;

            Task.Run(async () =>
            {
                res = await userServices.StartResetPassword("abcd@gmail.com", configuration);
            }).Wait();

            Assert.AreEqual(res.Result, BlueDogResult.Ok);
            Assert.IsNotNull(tmpUser.resetPasswordCode);
            Assert.IsNotNull(tmpUser.resetPasswordExpires);
            Assert.IsNotNull(res.ResetPasswordKey);
        }

        [TestMethod]
        public void StartPasswordResetBadPassword()
        {
            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret",
                PasswordResetDurationMinutes = 30
            };


            DataProviderMock.When(x => x.GetUserByEmail("abcd@gmail.com")).Return(Task.FromResult<User>(null));

            UserServices userServices = new UserServices(DataProviderMock);
            StartResetPasswordResult res = null;

            Task.Run(async () =>
            {
                res = await userServices.StartResetPassword("abcd@gmail.com", configuration);
            }).Wait();

            Assert.AreEqual(res.Result, BlueDogResult.NoSuchUser);
        }

        [TestMethod]
        public void CompletePasswordReset()
        {

            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret",
                PasswordResetDurationMinutes = 30
            };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.resetPasswordCode = validationCode;
            tmpUser.resetPasswordExpires = DateTime.Now.AddHours(1);

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmail("abcd@gmail.com")).Return(Task.FromResult<User>(tmpUser));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.CompleteResetPassword("abcd@gmail.com", validationCode, "newpass", configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.Ok);
            Assert.IsNull(tmpUser.resetPasswordCode);
            Assert.IsNull(tmpUser.resetPasswordExpires);

        }

        [TestMethod]
        public void CompletePasswordResetBadEmail()
        {

            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret",
                PasswordResetDurationMinutes = 30
            };

            DataProviderMock.When(x => x.GetUserByEmail("abcd@gmail.com")).Return(Task.FromResult<User>(null));
            DataProviderMock.When(x => x.SaveUser(It.IsAny<User>())).Return(Task.FromResult(BlueDogResult.Ok));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.CompleteResetPassword("abcd@gmail.com", validationCode, "newpass", configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.NoSuchUser);
        }

        [TestMethod]
        public void CompletePasswordResetBadValidation()
        {

            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret",
                PasswordResetDurationMinutes = 30
            };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.resetPasswordCode = "123456789";
            tmpUser.resetPasswordExpires = DateTime.Now.AddHours(1);

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmail("abcd@gmail.com")).Return(Task.FromResult<User>(tmpUser));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.CompleteResetPassword("abcd@gmail.com", validationCode, "newpass", configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.InvalidPasswordValidationKey);
        }

        [TestMethod]
        public void CompletePasswordResetValidationExpired()
        {

            string id = "1234";
            string validationCode = "abcdefghi";
            ServerConfiguration configuration = new ServerConfiguration()
            {
                JwtSecretKey = "secret",
                PasswordResetDurationMinutes = 30
            };

            User tmpUser = Users.CreateNewUser("abcd@gmail.com")
                .SetPassword("password123");

            tmpUser.id = id;
            tmpUser.resetPasswordCode = validationCode;
            tmpUser.resetPasswordExpires = DateTime.Now.AddHours(-1);

            string jwt = Tokens.TokenForUser(tmpUser, DateTime.Now.AddHours(-1), "secret");

            DataProviderMock.When(x => x.GetUserByEmail("abcd@gmail.com")).Return(Task.FromResult<User>(tmpUser));

            UserServices userServices = new UserServices(DataProviderMock);
            BlueDogResult res = BlueDogResult.Ok;

            Task.Run(async () =>
            {
                res = await userServices.CompleteResetPassword("abcd@gmail.com", validationCode, "newpass", configuration);
            }).Wait();

            Assert.AreEqual(res, BlueDogResult.PasswordValidationKeyExpired);

        }
    }
}


