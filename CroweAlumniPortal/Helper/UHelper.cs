using CroweAlumniPortal.Models;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace CroweAlumniPortal.Helper
{
    public class UHelper
    {
        public static string ExtractIdentifierFromUrl(string url)
        {
            string filename = Path.GetFileNameWithoutExtension(url);
            return filename;
        }

        public static bool IsUrl(string input)
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uri))
            {
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
            }
            else
            {
                return false;
            }
        }

        public static string ExtractExtensionFromBase64DataUri(string base64DataUri)
        {
            var regex = new Regex(@"data:(?<type>.*?);base64,");
            var match = regex.Match(base64DataUri);
            var type = match.Groups["type"].Value;
            var extension = "";

            switch (type)
            {
                case "image/png":
                    extension = "png";
                    break;
                case "image/jpeg":
                    extension = "jpeg";
                    break;
                case "image/jpg":
                    extension = "jpg";
                    break;
                default:
                    throw new ArgumentException("Unsupported file type");
            }

            return extension;
        }
    }
}
