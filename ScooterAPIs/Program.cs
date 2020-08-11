using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace ScooterAPIs
{
    class Program
    {
        // Constants for Bird demo
        const string DEVICE_ID = "123E4337-E99B-12D7-A411-426655157070";
        const string EMAIL = "mail@mail.com";
        const string LATITUDE = "32.7114906"; // Location
        const string LONGITUDE = "-117.1595124";
        const string RADIUS = "25";
        // Lime
        const string PHONE_NUM = ""; // ex: 15555555555

        static void Main(string[] args)
        {
            // Pick a demo to run
            RunBirdAsync().GetAwaiter().GetResult();
            //RunLimeAsync().GetAwaiter().GetResult();
            Console.ReadLine();
        }

        // Bird ====================================================================================
        static async Task RunBirdAsync()
        {
            // Try to get bird auth token
            // Run our async Task which calls the Bird Auth API to get our auth token
            BirdAuth auth = new BirdAuth();
            auth = await GetBirdAuthAsync(DEVICE_ID, EMAIL);

            // Make sure we have a valid token
            if (string.IsNullOrEmpty(auth.token))
                throw new Exception("Failed to get proper Bird Auth Token");

            // Use our token to call main Bird API to get a list of scooters
            BirdAPI api = new BirdAPI();
            api = await GetBirdScootersAsync(auth.token, LATITUDE, LONGITUDE, RADIUS);

            // Print out info for each scooter found
            Console.WriteLine($"Found {api.birds.Count} scooters!\n");
            foreach (var scooter in api.birds)
                Console.WriteLine($"Scooter {scooter.id}:\n    " +
                    $"Lat: {scooter.location.latitude}, " +
                    $"Long: {scooter.location.longitude}, " +
                    $"Battery: {scooter.battery_level}%\n");
        }

        static async Task<BirdAuth> GetBirdAuthAsync(string deviceId, string email)
        {
            HttpClient client = new HttpClient();
            string url = "https://api.bird.co/user/login";
            BirdAuth auth = null;
            // Add required headers
            client.DefaultRequestHeaders.Add("Device-id", deviceId);
            client.DefaultRequestHeaders.Add("Platform", "ios");
            /* HTTP post request to api
               Here our data is sent to the API */
            HttpResponseMessage response = await client.PostAsync(url,
                new StringContent($"{{\"email\": \"{email}\"}}", Encoding.UTF8, "application/json"));

            // If we got a successful response...
            if (response.IsSuccessStatusCode)
            {
                // ...store response in a string
                var content = await response.Content.ReadAsStringAsync();
                // Take that string and put it into our BirdAuth data structure
                auth = JsonConvert.DeserializeObject<BirdAuth>(content);
            }
            else // Response unsuccessful
                throw new Exception("BirdAuthAPI request rejected (Bad params?)");

            return auth;
        }

        static async Task<BirdAPI> GetBirdScootersAsync(string token, string latitude, string longitude, string radius)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.bird.co/bird/nearby?latitude={latitude}&longitude={longitude}&radius={radius}";
            BirdAPI api = null;
            client.DefaultRequestHeaders.Add("Authorization", $"Bird {token}");
            client.DefaultRequestHeaders.Add("Device-id", DEVICE_ID);
            client.DefaultRequestHeaders.Add("App-Version", "4.27.1.0");
            client.DefaultRequestHeaders.Add("Location",
                                             $"{{\"latitude\":{latitude}," +
                                             $"\"longitude\":{longitude}," +
                                             $"\"altitude\":500," +
                                             $"\"accuracy\":100," +
                                             $"\"speed\":-1," +
                                             $"\"heading\":-1}}");
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                api = JsonConvert.DeserializeObject<BirdAPI>(content);
            }
            else
                throw new Exception("BirdAPI request rejected (Bad params?)");

            return api;
        }

        // Lime ====================================================================================
        static async Task RunLimeAsync()
        {
            /* This call will send a token to your phone. In order for this to work you must
             * have a Lime account registered to your PHONE_NUM.
             * You should run this line by itself to get a token texted to you.
             * Then uncomment the rest of this method and comment the line below to actually 
             * run the demo.
             * Put the code you get texted in the GetLimeAuthAsync() call. */
            await GetLimeRegisterTokenAsync(PHONE_NUM);

            /*
            // Auth
            LimeAuth auth = new LimeAuth();
            auth = await GetLimeAuthAsync(PHONE_NUM, "527544");

            // Make sure we have a valid token
            if (string.IsNullOrEmpty(auth.token))
                throw new Exception("Failed to get proper Lime Auth Token");
            
            // Use our token to call main Lime API to get a list of scooters
            LimeAPI api = new LimeAPI();
            api = await GetLimeScootersAsync(auth.token, auth.cookie, NE_LAT, NE_LONG, SW_LAT, SW_LONG,
                LATITUDE, LONGITUDE);

            // Print out info for each scooter found
            Console.WriteLine($"Found {api.data.attributes.bikes.Count} scooters!\n");
            foreach (var scooter in api.data.attributes.bikes)
                Console.WriteLine($"Scooter {scooter.id}:\n    " +
                    $"Lat: {scooter.attributes.latitude}, " +
                    $"Long: {scooter.attributes.longitude}, " +
                    $"Battery: {scooter.attributes.battery_level}\n");
            */
        }

        static async Task<Object> GetLimeRegisterTokenAsync(string phoneNum)
        {
            HttpClient client = new HttpClient();
            string url = $"https://web-production.lime.bike/api/rider/v1/login?phone=%2B{phoneNum}";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(content);
            }
            else
                throw new Exception("LimeRegisterAPI request rejected (Bad params?)");

            return new Object();
        }

        static async Task<LimeAuth> GetLimeAuthAsync(string phoneNum, string limeCode)
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            HttpClient client = new HttpClient(handler);
            string url = "https://web-production.lime.bike/api/rider/v1/login";
            LimeAuth auth = null;
            /* HTTP post request to api
               Here our data is sent to the API */
            HttpResponseMessage response = await client.PostAsync(url,
                new StringContent($"{{\"login_code\": \"{limeCode}\", \"phone\": \"+{phoneNum}\"}}", Encoding.UTF8, "application/json"));

            // If we got a successful response...
            if (response.IsSuccessStatusCode)
            {
                // ...store response in a string
                var content = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(content);
                // Take that string and put it into our LimeAuth data structure
                auth = JsonConvert.DeserializeObject<LimeAuth>(content);

                IEnumerable<Cookie> responseCookies = cookies.GetCookies(new Uri(url, UriKind.Absolute)).Cast<Cookie>();
                foreach (Cookie cookie in responseCookies)
                    //Console.WriteLine(cookie.Name + ": " + cookie.Value);
                    if (cookie.Name == "_limebike-web_session") auth.cookie = cookie.Value;
            }
            else if ((int)response.StatusCode == 429)
                throw new Exception("Lime: Too Many Requests (Rate Limited)");
            else // Response unsuccessful
                throw new Exception("LimeAuthAPI request rejected (Bad params?)");

            return auth;
        }

        static async Task<LimeAPI> GetLimeScootersAsync(string token, string cookie, string neLat,
            string neLong, string swLat, string swLong, string latitude, string longitude)
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseCookies = false;
            HttpClient client = new HttpClient(handler);
            string url = $"https://web-production.lime.bike/api/rider/v1/views/map?ne_lat={neLat}&ne_lng={neLong}&sw_lat={swLat}&sw_lng={swLong}&user_latitude={latitude}&user_longitude={longitude}&zoom=16";
            LimeAPI api = null;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("Cookie", $"_limebike-web_session={cookie}");

            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(content);
                api = JsonConvert.DeserializeObject<LimeAPI>(content);
            }
            else if ((int)response.StatusCode == 429)
                throw new Exception("Lime: Too Many Requests (Rate Limited)");
            else
                throw new Exception("LimeAuthAPI request rejected (Bad params?)");

            return api;
        }
    }
}
