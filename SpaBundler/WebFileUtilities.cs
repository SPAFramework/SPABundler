using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace SpaBundler
{
    internal static class WebFileUtilities
    {
        /// <summary>
        /// Extracts references uris from a HTML string 
        /// </summary>
        /// <param name="html">HTML String</param>
        /// <returns> A list of string reference uris</returns>
        public static IList<string> GetHtmlReferences(string html)
        {
            Contract.Requires(html != null, "Argument must be a HTML string.");
            Contract.Ensures(Contract.Result<IList<string>>() != null);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var refferences = doc.DocumentNode.SelectNodes("//*[@src] | //*[@href]");
            return (refferences != null) ?
                refferences.Select(n =>
                    n.Attributes.Any(a => a.Name == "src") ? n.Attributes["src"].Value : n.Attributes["href"].Value).ToList() :
                new List<string>();
        }

        /// <summary>
        /// Extracts refference uris from a CSS string
        /// </summary>
        /// <param name="css">CSS string</param>
        /// <returns>A list of reference uris</returns>
        public static IList<string> GetCssReferences(string css)
        {
            Contract.Requires(css != null, "Argument must be a CSS string.");
            Contract.Ensures(Contract.Result<IList<string>>() != null);
            css=Regex.Replace(css, @"/\*.+?\*/", string.Empty, RegexOptions.Singleline);
            var refferences = Regex.Matches(css, @"url\('(.*?)'\)").Cast<Match>().ToList();
            return refferences.Any() ?
                refferences.Select(m => m.Value.Replace("url('", String.Empty).Replace("')", String.Empty)).ToList() :
                new List<string>();
        }
         
        /// <summary>
        /// Gets the absolute path from a basePath and uri
        /// </summary>
        /// <param name="basePath">Website base path (must end with \)</param>
        /// <param name="uri">Relative resource uri</param>
        /// <returns>Absolute path as a string</returns>
        public static string GetFullPathFromUri(string basePath, string uri)
        {
            Contract.Requires(basePath != null && uri != null, "basePath and uri should not be null");
            Contract.Requires(basePath.EndsWith("\\"), "basePath must end with \"\\\"" );
            Contract.Requires(!uri.StartsWith("/"), "Uri must not start with \"/\"");
            Contract.Ensures(Contract.Result<string>() != null);
            var pathCrums = basePath.Split('\\'); 
            var moveUp = Regex.Matches(uri, "../").Count;
            pathCrums = pathCrums.Take(pathCrums.Length - moveUp).ToArray();
            basePath = pathCrums.Aggregate("", (c, x) => c + x + @"\");
            var relativePath = uri.Replace("../", "").Replace("/", @"\");
            return basePath + relativePath;
        }
    }
}
