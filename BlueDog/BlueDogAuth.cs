using BlueDog.DataProvider;
using BlueDog.Models;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog
{
    public class BlueDogAuth
    {
        protected UserServices UserServices { get; set; }
        public ServerConfiguration ServerConfiguration { get; set; }
        public BlueDogAuth(ServerConfiguration configuration, IDataProvider dataProvider = null)
        {
            ServerConfiguration = configuration;
            UserServices = new UserServices(dataProvider);
        }

        /// <summary>
        /// Login Entry Point
        /// </summary>
        /// <param name="req"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> Login(HttpRequestMessage req, TraceWriter log)
        {
            string email = await req.QueryOrBody("email");
            string password = await req.QueryOrBody("password");
            string passlog = password != null ? "###" : "<null>";

            log.Info($"Logging in with {email} and {passlog}");

            LoginResult res = await UserServices.Login(email, password, ServerConfiguration);

            if (res.Result == BlueDogResult.Ok)
            {
                HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Ok, Token = res.Jwt, data = res.User });

                response.Headers.Add("Authorization", "Bearer " + res.Jwt);

                return response;
            }
            else
                return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        public async Task<HttpResponseMessage> Register(HttpRequestMessage req, TraceWriter log)
        {
            string email = await req.QueryOrBody("email");
            string password = await req.QueryOrBody("password");
            string passlog = password != null ? "###" : "<null>";
            log.Info($"Logging in with {email} and {passlog}");


            BlueDogResult result = await UserServices.Register(email, password, ServerConfiguration);

            ResponseCode code = result == BlueDogResult.Ok ? ResponseCode.Ok : ResponseCode.EmailInUse;


            return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = code });
        }

        public async Task<HttpResponseMessage> StartResetPassword(HttpRequestMessage req, TraceWriter log)
        {
            string email = await req.QueryOrBody("email");

            StartResetPasswordResult res = await UserServices.StartResetPassword(email, ServerConfiguration);

            switch (res.Result)
            {
                case BlueDogResult.Ok:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Ok, Token = res.Jwt, data = res.ResetPasswordKey });
                default:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Error });
            }
        }

        public async Task<HttpResponseMessage> GetCurrentUser(HttpRequestMessage req, TraceWriter log)
        {
            string jwt = await req.QueryOrBody("jwt");

            log.Info($"Getting user for {jwt}");

            UserResult res = await UserServices.GetCurrentUser(jwt, ServerConfiguration);

            if (res.Result == BlueDogResult.Ok)
            {
                HttpResponseMessage response = req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Ok, Token = res.Jwt, data = res.User });

                return response;
            }
            else
                return req.CreateResponse(HttpStatusCode.Unauthorized);

        }

        public async Task<HttpResponseMessage> CompletePasswordReset(HttpRequestMessage req, TraceWriter log)
        {
            string email = await req.QueryOrBody("email");
            string validation = await req.QueryOrBody("validation");
            string password = await req.QueryOrBody("password");

            BlueDogResult res = await UserServices.CompleteResetPassword(email, validation, password, ServerConfiguration);

            switch( res)
            {
                case BlueDogResult.InvalidPasswordValidationKey:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Invalid });
                case BlueDogResult.PasswordValidationKeyExpired:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Expired });
                case BlueDogResult.Ok:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Ok });
                default:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Error });
            }

        }

        public async Task<HttpResponseMessage> StartChangeEmail(HttpRequestMessage req, TraceWriter log)
        {
            string email = await req.QueryOrBody("email");

            StartResetPasswordResult res = await UserServices.StartChangeEmail(email, ServerConfiguration);

            switch (res.Result)
            {
                case BlueDogResult.Ok:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Ok });
                default:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Error, data = res.ResetPasswordKey });
            }
        }

        public async Task<HttpResponseMessage> CompleteChangeEmail(HttpRequestMessage req, TraceWriter log)
        {
            string email = await req.QueryOrBody("email");
            string validation = await req.QueryOrBody("validation");
            string jwt = await req.QueryOrBody("jwt");

            BlueDogResult res = await UserServices.UpdateEmail( jwt, email, validation, ServerConfiguration);

            switch (res)
            {
                case BlueDogResult.InvalidPasswordValidationKey:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Invalid });
                case BlueDogResult.PasswordValidationKeyExpired:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Expired });
                case BlueDogResult.Ok:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Ok });
                default:
                    return req.CreateResponse(HttpStatusCode.OK, new ServerResponse { status = ResponseCode.Error });
            }
        }
    }
}