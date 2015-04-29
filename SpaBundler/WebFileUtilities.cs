using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
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
        /// <returns> IEnumerable collection of string reference uris</returns>
        public static IEnumerable<string> GetHtmlReferences(string html)
        {
            Contract.Requires(html != null, "Argument must be a HTML string.");
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var references = doc.DocumentNode.SelectNodes("//*[@src] | //*[@href]");
            return (references != null) ?
                references.Select(n => n.Attributes.Any(a => a.Name == "src") ? n.Attributes["src"].Value : n.Attributes["href"].Value) :
                Enumerable.Empty<string>();
        }

        /// <summary>
        /// Extracts refference uris from a CSS string
        /// </summary>
        /// <param name="css">CSS string</param>
        /// <returns>IEnumerable collection of string reference uris</returns>
        public static IEnumerable<string> GetCssReferences(string css)
        {
            Contract.Requires(css != null, "Argument must be a CSS string.");
            Contract.Ensures(Contract.Result<IEnumerable<string>>() != null);
            css=Regex.Replace(css, @"/\*.+?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Matches(css, @"url\('(.*?)'\)").Cast<Match>()
                .Select(m => m.Value.Replace("url('", String.Empty).Replace("')", String.Empty));
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
            Contract.Ensures(Contract.Result<string>() != null);
            var baseCrums = basePath.Trim().Split(new[]{'\\'}, StringSplitOptions.RemoveEmptyEntries);
            baseCrums[0] += "\\"; //Add the slash to confirm with Path.Combine() requirements
            var uriCrums = uri.Trim().Split(new[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var moveUp = uriCrums.Count(x => x.Equals(".."));
            var pathCrums = baseCrums.Take(baseCrums.Length - moveUp)
                .Concat(uriCrums.Where(crum => !crum.Equals("..")));

            return Path.Combine(pathCrums.ToArray());
        }

        /// <summary>
        /// Removes Nodes that reference a specific uri in a html string
        /// </summary>
        /// <param name="html">Html String as reference</param>
        /// <param name="uri">Uri Reference</param>
        public static void RemoveReferenceNodes(ref string html, string uri)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            foreach(var node in doc.DocumentNode.SelectNodes("//*[contains(@src, '" + uri + "')] | //*[contains(@href,'" + uri + "')]"))
                node.Remove();
            html = doc.DocumentNode.OuterHtml;
        }

        /// <summary>
        /// Inserts a Html node inside a parent node in a html document
        /// </summary>
        /// <param name="html">Html string as reference</param>
        /// <param name="parentTagName">The tag name of the parent Ex "head"</param>
        /// <param name="tagName">The tag name for the node you want to create</param>
        /// <param name="content">The content inside the node</param>
        public static void InsertNode(ref string html, string parentTagName, string tagName, string content)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var parent = doc.DocumentNode.Descendants(parentTagName).First();
            var node = String.Format("<{0}>{1}</{0}>", tagName, content);
            parent.AppendChild(HtmlNode.CreateNode(node));
            html = doc.DocumentNode.OuterHtml;
        }
    }
}
