using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BlueDog;
using BlueDog.Models;

namespace BlueDogTest
{
    [TestClass]
    public class TokenTests
    {
        [TestMethod]
        public void TestToken()
        {
            User user = Users.CreateNewUser("abcd@efghi.com");

            user.SetPassword("password1");

            user.id = "1234";

            string token = Tokens.TokenForUser(user, DateTime.Now.AddHours(1), "abcdef");


            TokenData tokenData = Tokens.DecodeToken(token, "abcdef");

            Assert.AreEqual(tokenData.userid, "1234");


        }
    }
}
