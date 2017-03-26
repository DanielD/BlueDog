
using System;
using System.Configuration;
using BlueDog.Models;
using Newtonsoft.Json;
using JWT;
using JWT.Serializers;
using JWT.Algorithms;
using System.Threading.Tasks;
using BlueDog.DataProvider;

namespace BlueDog
{
    public static class Tokens
    {

        public static bool ValidPayload(TokenData data)
        {
            return data.issued != null && data.expires != null && data.userid != null;
        }




        public static bool PayloadExpired(TokenData data)
        {
            if (data.expires.Value.Ticks < DateTime.Now.Ticks)
                return true;

            return false;
        }

        public static string TokenForUser(User user, DateTime expiration, string secret)
        {
            //            var secret = ConfigurationManager.AppSettings["JWT_Secret_Key"];

            var payload = new TokenData
            {
                issued = DateTime.Now,
                expires = expiration,
                userid = user.id
            };
            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer);

            string token = encoder.Encode(payload, secret);
            return token;
        }

        public static TokenData DecodeToken(string token, string secret)
        {
            IJsonSerializer serializer = new JsonNetSerializer();
            IDateTimeProvider provider = new UtcDateTimeProvider();
            IJwtValidator validator = new JwtValidator(serializer, provider);
            IJwtDecoder decoder = new JwtDecoder(serializer, validator);

            string json;

            try
            {
                json = decoder.Decode(token, secret, verify: true);
            }
            catch (Exception e)
            {
                return null;
            }

            TokenData data = JsonConvert.DeserializeObject<TokenData>(json);

            return data;
        }

        public static async Task<BlueDogResult> ValidateToken( string jwt, IDataProvider dataProvider, ServerConfiguration configuration, out User user )
        {
            var secret = configuration.JwtSecretKey;

            TokenData token = Tokens.DecodeToken(jwt, secret);

            if (Tokens.PayloadExpired(token))
            {
                return BlueDogResult.ExpiredToken;
            }

            user = await dataProvider.GetUserById(token.userid);

            // was there a user
            if (user == null)
                return BlueDogResult.NoSuchUser;

            return BlueDogResult.Ok;
        }

    }
}
