using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        private const string ExperienceBankUrl = "https://api.experiencebank.io/v1";
        private const string PublicKey = "pub_3ab6848cd66da12356dd2b014100eb5beaf7cd7d63";
        private const string PrivateKey = "sec_ae2453a283ae77dd398fb564fe98792527ff5b47aa ";
        private const string SupplierId = "par_fa585577-fba0-4ae0-bc18-5396a54ff55c";
        private static JsonSerializerSettings _jsonSerializerSettings;
        private static HttpClient _http;

        static void Main(string[] args)
        {
            _http = new HttpClient();
            SetupJsonSerializer();
            var response = InvokeMethod("supplier.find", null).Result;

            Console.WriteLine(response);
            Console.ReadKey();
        }

        private static void SetupJsonSerializer()
        {
            _jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static byte[] HashHMAC(string message, string key)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var keyBytes = Encoding.UTF8.GetBytes(key);


            var hash = new HMACSHA256(keyBytes);
            return hash.ComputeHash(messageBytes);
        }

        private static string Base64Encode(string text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string Base64EncodeWithoutPadding(string text)
        {
            var encodedText = Base64Encode(text);

            encodedText = encodedText.Remove('=');
            encodedText = encodedText.Replace('+', '-');
            encodedText = encodedText.Replace('/', '_');

            return encodedText;
        }

        private static void SetupHttpClientHeaders(string requestBody)
        {
            _http.DefaultRequestHeaders.Clear();

            requestBody = Base64EncodeWithoutPadding(requestBody);

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                $"Authorization", $"Basic { Base64Encode(PublicKey + ":" + HashHMAC(requestBody, PrivateKey)) }");

            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public static async Task<Newtonsoft.Json.Linq.JObject> InvokeMethod(string method, Parameters parameters)
        {
            EbRequest request = new EbRequest();
            request.Jsonrpc = "2.0";
            request.Method = method;

            var query = new Query
            {
                SupplierIds = new string[0],
                Cursor = null
            };

            parameters = new Parameters
            {
                Query = query
            };

            request.Params = parameters;
            request.Id = 1;


            string serializedRequest = JsonConvert.SerializeObject(request, _jsonSerializerSettings);

            SetupHttpClientHeaders(serializedRequest);

            Console.WriteLine(JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(serializedRequest, _jsonSerializerSettings));

            var result = await SendMessage(serializedRequest);

            Newtonsoft.Json.Linq.JObject response = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(result, _jsonSerializerSettings);

            return response;
        }

        private static async Task<string> SendMessage(string requestBody)
        {
            var jsonString = string.Empty;
            HttpContent httpContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _http.PostAsync($"{ ExperienceBankUrl}/", httpContent);

            if (response.IsSuccessStatusCode)
            {
                jsonString = await response.Content.ReadAsStringAsync();
            }

            return jsonString;
        }
    }
}
