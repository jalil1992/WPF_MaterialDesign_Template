using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WPF_M_Selenium
{
    public static class Utility
    {
        public static string beautify(string input, int len = 50)
        {
            if (input.Length > len)
                input = input.Substring(0, len) + "...";
            return input;
        }

        public static string get_gsheet_id(string url)
        {
            Regex reg = new Regex("d/[^/]*");
            string spreadsheetId = reg.Match(url).Value;
            if (spreadsheetId == "")
            {
                App.log_info("Google sheet URL is invalid.");
                return "";
            }
            spreadsheetId = spreadsheetId.Substring(2);
            return spreadsheetId;
        }

        /// <summary>
        /// Perform a deep Copy of the object, using Json as a serialisation method. NOTE: Private members are not cloned using this method.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }
    }
}
