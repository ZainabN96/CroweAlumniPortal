using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace CroweAlumniPortal.Extenstions
{
    public static class AppSessionExtensions
    {
        private static IHttpContextAccessor _httpContextAccessor;

        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public static HttpContext Current => _httpContextAccessor.HttpContext;

        
        public static T Session<T>(string key, T value)
        {

            if (String.IsNullOrEmpty(Current.Session.GetString(key)))
            {
                string obj = JsonConvert.SerializeObject(value);
                Current.Session.SetString(key, obj);
                return value;
            }
            else
            {
                string obj = Current.Session.GetString(key);
                return JsonConvert.DeserializeObject<T>(obj);
            }
        }

        
    }
}
