using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlueDog
{
    public static class FunctionHelper
    {
        public static async Task<string> QueryOrBody( this HttpRequestMessage req, string key )
        {
            // parse query parameter
            string value = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, key, true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            value = value ?? data["key"];

            return value;
        }

    }
}
