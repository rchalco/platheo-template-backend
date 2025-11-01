using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VectorStinger.Foundation.Utilities.Services
{
    public class ClientHelper
    {
        public async Task<T> Consume<T>(string URI, object parameter) where T : class, new()
        {
            T resul = new T();
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(URI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");


                var content = new StringContent(JsonConvert.SerializeObject(parameter), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(URI, content);
                var data = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    resul = JsonConvert.DeserializeObject<T>(data);
                }
                else
                {
                    throw new Exception($"No se puede consumir el servicio. Error: {data}");
                }
            }
            return resul;
        }
    }
}
