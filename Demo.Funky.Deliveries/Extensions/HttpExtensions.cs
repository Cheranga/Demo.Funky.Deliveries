using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;

namespace Demo.Funky.Deliveries.Extensions
{
    public static class HttpExtensions
    {
        public static async Task<TModel> ToModel<TModel>(this HttpRequest request) where TModel : class, new()
        {
            var content = await request.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                return new TModel();
            }

            var model = JsonConvert.DeserializeObject<TModel>(content, new JsonSerializerSettings
            {
                Error = (_, args) => args.ErrorContext.Handled = true
            });

            return model ?? new TModel();
        }
    }
}