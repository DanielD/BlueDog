using BlueDog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog.DataProvider
{
    public interface IDataProvider
    {
        void Initialize(ServerConfiguration configuration);

        Task<BlueDogResult> SaveUser(User user);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserById(string id);
        Task<User> GetUserByEmailValidation(string validation);

    }
}
