using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlueDog;
using BlueDog.Models;

namespace BlueDogTest
{
    [TestClass]
    public class UserTests
    {
        [TestMethod]
        public void TestUser()
        {
            User user = Users.CreateNewUser("abcd@efghi.com");

            user.SetPassword("password1");

            Assert.IsTrue(user.PasswordMatches("password1"));
        }
    }
}
