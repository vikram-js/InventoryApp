using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
namespace InventoryApp
{
    public class UserCred
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public UserCred(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public override string ToString()
        {
            return $"Username({Username}) - Password({Password})";
        }
    }
    public class InventoryItem
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Price { get; set; }

        public InventoryItem(int id, string name, int price)
        {
            Id = id;
            ItemName = name;
            Price = price;
        }


        public override string ToString()
        {
            return $"Id({Id}) - ItemName({ItemName}) - Price({Price})";
        }
    }

    class Program
    {
        // Act as a HTTP client
        static HttpClient client = new HttpClient();

        static void Main(string[] args)
        {
            // Specify Web API base address
            client.BaseAddress =
                new Uri("https://localhost:44332/");

            // Specify headers
            var val = "application/json";
            var media =
                new MediaTypeWithQualityHeaderValue(val);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(media);

            // Make the calls - POST, GET, UPDATE and DELETE
            try
            {
                var message = string.Empty;
                Console.WriteLine("Username:");
                string username = Console.ReadLine();
                Console.WriteLine("password:");
                string password = Console.ReadLine();
                var userCred = new UserCred(username, password);
                string tokenOrig = Authenticate(userCred);
                if(tokenOrig=="Unauthorised!")
                {
                    Console.WriteLine(tokenOrig);
                    return;
                    
                }
                Console.WriteLine($"Auth token: {tokenOrig}");

                var token = tokenOrig.Substring(1, tokenOrig.Length - 2);
                Console.WriteLine($"Auth token: {token}");

                Console.WriteLine("id: ");
                int id = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Name: ");
                string itemName = Console.ReadLine();
                Console.WriteLine("price:");
                int price = Convert.ToInt32(Console.ReadLine());

                var inventoryItem = new InventoryItem(id, itemName, price);
                // create
                message = AddInventoryItem(inventoryItem,token);
                if (message == "Unauthorised!")
                {
                    Console.WriteLine(message);
                    return;
                }
                Console.WriteLine($"Create: {message}");

                Console.WriteLine();

                // read
                Console.WriteLine("List items:");
                var items=GetInventoryItems(token);



//                Console.WriteLine("hey");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }

            Console.ReadLine();
        }



        private static Dictionary<string,InventoryItem> GetInventoryItems(string token)
        {
            Dictionary<string,InventoryItem> items=null;
            using (var client = new HttpClient())
            {
                
                client.BaseAddress = new Uri("https://localhost:44332/v1/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                //HTTP GET
                var responseTask = client.GetAsync("GetInventoryItems");
                responseTask.Wait();

                var result = responseTask.Result;
                if (result.IsSuccessStatusCode)
                {

                    var readTask = result.Content.ReadAsAsync<Dictionary<string,InventoryItem>>();
                    readTask.Wait();

                    items = readTask.Result;

                    foreach (var item in items)
                    {
                        Console.WriteLine(item.Value);
                    }
                    
                }
                
            }
            return items;
        }
  
        private static string AddInventoryItem(InventoryItem inventoryItem, string token)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress =new Uri("https://localhost:44332/v1/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var request =
                    client.PostAsJsonAsync("AddInventoryItems",inventoryItem);
                var response =
                    request.Result.Content.ReadAsStringAsync();

                var statusCode = request.Result.IsSuccessStatusCode;
                Console.WriteLine(statusCode);
                if (!statusCode)
                    return "Unauthorised!";
                return response.Result;
            }
        }

        private static string Authenticate(UserCred userCred)
        {
            var action = "v1/Authenticate";
            var request =
                client.PostAsJsonAsync(action, userCred);

            var response =
                request.Result.Content.ReadAsStringAsync();

            var statusCode = request.Result.IsSuccessStatusCode;
            Console.WriteLine(statusCode);
            if (!statusCode)
                return "Unauthorised!";

            return response.Result;
        }
    }
}
