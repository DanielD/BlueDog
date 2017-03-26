using Microsoft.Azure.Documents.Client;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using BlueDog.Models;
using System.Net.Mail;
using System.Security.Cryptography;

namespace BlueDog
{
    public static class Users
    {
        public static Uri CreateUserCollectionUri(ServerConfiguration configuration)
        {
            return UriFactory.CreateDocumentCollectionUri(configuration.UsersDatabaseName, configuration.UsersCollectionName);
        }

        public static User GetUserByEmail(this DocumentClient client, string email, ServerConfiguration configuration)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

            IQueryable<User> query = client.CreateDocumentQuery<User>( CreateUserCollectionUri(configuration), queryOptions)
                .Where(f => f.email == email);

            User user = query.AsEnumerable().FirstOrDefault();

            return user;
        }

        public static User GetUserById(this DocumentClient client, string id, ServerConfiguration configuration)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

            IQueryable<User> query = client.CreateDocumentQuery<User>(CreateUserCollectionUri(configuration), queryOptions)
                .Where(f => f.id == id);

            User user = query.AsEnumerable().FirstOrDefault();


            return user;
        }


        public static User GetUserByValidation(this DocumentClient client, string validation, ServerConfiguration configuration)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

            IQueryable<User> query = client.CreateDocumentQuery<User>(CreateUserCollectionUri(configuration), queryOptions)
                .Where(f => f.emailValidationCode.Equals(validation));

            User user = query.AsEnumerable().FirstOrDefault();


            return user;
        }



        public static async Task<BlueDogResult> SaveUser(this DocumentClient client, User user, ServerConfiguration configuration)
        {
            if (user.id == null)
            {
                await client.CreateDocumentAsync(CreateUserCollectionUri(configuration), user);
            } 
            else
            {
                await client.ReplaceDocumentAsync(CreateUserCollectionUri(configuration), user);
            }

            return BlueDogResult.Ok;
        }


        public const int SALT_BYTES = 24;
        public const int HASH_BYTES = 18;
        public const int PBKDF2_ITERATIONS = 64000;


        public static User CreateNewUser(string email)
        {
            User user = new Models.User { email = email };

            user.created = DateTime.Now;

            byte[] salt = new byte[SALT_BYTES];
            using (RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(salt);
            }

            user.salt = System.Convert.ToBase64String(salt);
            user.emailValidated = false;

            return user;
        }

        public static User SetPassword(this User user, string password)
        {
            user.password = HashPassword(password, user.salt);
            return user;
        }

        public static bool PasswordMatches(this User user, string passwordToCheck)
        {
            string hashedPass = HashPassword(passwordToCheck, user.salt);
            return user.password.Equals(hashedPass);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] saltArray = System.Convert.FromBase64String(salt);

            byte[] hashedPassword = PBKDF2(password, saltArray, PBKDF2_ITERATIONS, SALT_BYTES);

            return System.Convert.ToBase64String(hashedPassword);
        }

        private static byte[] PBKDF2(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt))
            {
                pbkdf2.IterationCount = iterations;
                return pbkdf2.GetBytes(outputBytes);
            }
        }



    }
}
