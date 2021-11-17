/*
 * QueryAPI.cs - C# script that runs Devo Query API requests.
 * 
 * 
 * Version:  1.0.0
 * Author:   Roberto Meléndez  [Cambridge, USA]
 * GitHub:   https://github.com/rcmelendez/devo-api-csharp
 * Contact:  @rcmelendez on LinkedIn, Medium, and GitHub
 * Released: November 17, 2021
 */ 

using System.Security.Cryptography;
using System.Text;


namespace Devo
{
    class Program
    {

        public void callDevoAPI()
        {
            /*
             * START USER CONFIGURATION
             * 
             * Replace the following variables with your own values:
             * - key
             * - secret
             * - cloud
             * - query
             * - body
             */

            // In the Devo webapp, go to Administration -> Credentials -> Access Keys tab
            String key = "YOUR-DEVO-API-KEY-GOES-HERE";
            String secret = "YOUR-DEVO-API-SECRET-GOES-HERE";

            // Available options: us, eu, es, ca, saas
            String cloud = "us";
            
            String query = "from box.unix select *";
            String body = "" +
                "{\"from\": \"2h\", " +
                "\"to\": \"now\", " +
                "\"query\":\"" + query + "\"," +
                "\"limit\": 10, " +
                "\"mode\": { \"type\":\"csv\"}}";

            /*
             * END USER CONFIGURATION
             */ 


            var httpclient = new HttpClient();
            httpclient.Timeout = TimeSpan.FromMinutes(9999);
            String queryEndpoint = "https://apiv2-" + cloud + ".devo.com/search/query";
            var webRequest = new HttpRequestMessage(HttpMethod.Post, queryEndpoint)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            // HMAC-SHA256 signature
            String unixTimestamp = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds.ToString();
            unixTimestamp = unixTimestamp.Substring(0, unixTimestamp.IndexOf("."));
            String data = key + body + unixTimestamp;
            byte[] byteArrayData = Encoding.UTF8.GetBytes(data);
            byte[] byteArraySecret = Encoding.UTF8.GetBytes(secret);
            var hash = new HMACSHA256(byteArraySecret);
            byte[] byteSigned = hash.ComputeHash(byteArrayData);
            var hexString = BitConverter.ToString(byteSigned);
            String sign = hexString.Replace("-", "").ToLower();

            // Required Devo headers
            httpclient.DefaultRequestHeaders.Add("x-logtrust-apikey", key);
            httpclient.DefaultRequestHeaders.Add("x-logtrust-timestamp", unixTimestamp);
            httpclient.DefaultRequestHeaders.Add("x-logtrust-sign", sign);


            try
            {
                var response = httpclient.Send(webRequest);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }

            catch (Exception ex)
            {
                Console.WriteLine("Exception occured:  " + ex.Message);
            }
        }

        public static void Main(string[] args)
        {
            Program p = new Program();
            p.callDevoAPI();
        }
    }
}
