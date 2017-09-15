using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MalStyleImageUpdater
{
    class Utils
    {
        public static string RemoveWhiteSpaceFromStyles(string body)
        {
            //body = Regex.Replace(body, @"[a-zA-Z]+#", "#");
            body = Regex.Replace(body, @"[\n\r]+", string.Empty);
            body = Regex.Replace(body, @"\s+", "");
            //body = Regex.Replace(body, @"\s+", " ");
            //body = Regex.Replace(body, @"\s?([:,;{}])\s?", "$1");
            body = body.Replace(";}", "}");
            body = Regex.Replace(body, @"0(px|pt|%|em)", "0");

            // Remove comments from CSS
            body = Regex.Replace(body, @"/\*[\d\D]*?\*/", string.Empty);
            return body;
        }

    }
}
