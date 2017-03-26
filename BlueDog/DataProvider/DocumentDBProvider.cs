using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlueDog.Models;
using Microsoft.Azure.Documents.Client;

namespace BlueDog.DataProvider
{
    public class DocumentDBProvider : IDataProvider
    {
        public DocumentClient client { get; set; }
        public Uri UserCollectionUri { get; set; }

        public DocumentDBProvider(ServerConfiguration configuration = null)
        {
            if (configuration != null)
                Initialize(configuration);
        }


        /// <summary>
        /// Initialize the provider. Set up the user collection uri and initialize the DocumentDB Client
        /// </summary>
        /// <param name="configuration"></param>
        public void Initialize(ServerConfiguration configuration)
        {
            client = new DocumentClient(new Uri(configuration.DocumentDBEndpointUri), configuration.DocumentDBMasterKey);
            UserCollectionUri = UriFactory.CreateDocumentCollectionUri(configuration.UsersDatabaseName, configuration.UsersCollectionName);
        }

        /// <summary>
        /// Get the user based on email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>The user object. null if it wasn't found.</returns>
        public async Task<User> GetUserByEmail(string email)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

            IQueryable<User> query = client.CreateDocumentQuery<User>(UserCollectionUri, queryOptions)
                .Where(f => f.email == email);

            User user = query.AsEnumerable().FirstOrDefault();

            return user;
        }

        /// <summary>
        /// Gets the user based on its id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The user object. null if it wasn't found.</returns>
        public async Task<User> GetUserById(string id)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1, EnableCrossPartitionQuery = true };

            IQueryable<User> query = client.CreateDocumentQuery<User>(UserCollectionUri, queryOptions)
                .Where(f => f.id == id);

            User user = query.AsEnumerable().FirstOrDefault();


            return user;
        }

        /// <summary>
        /// Gets the user based on the email validation code
        /// </summary>
        /// <param name="validation"></param>
        /// <returns></returns>
        public async Task<User> GetUserByEmailValidation(string validation)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = 1 };

            IQueryable<User> query = client.CreateDocumentQuery<User>(UserCollectionUri, queryOptions)
                .Where(f => f.emailValidationCode.Equals(validation));

            User user = query.AsEnumerable().FirstOrDefault();


            return user;
        }

        public async Task<BlueDogResult> SaveUser(User user)
        {
            if (user.id == null)
                await client.CreateDocumentAsync(UserCollectionUri, user);
            else
                await client.ReplaceDocumentAsync(UserCollectionUri, user);

            return BlueDogResult.Ok;
        }
    }
}
