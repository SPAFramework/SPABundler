using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace SpaBundler
{
    /// <summary>
    /// Function for optimizing various web mime-types
    /// </summary>
    /// <param name="fileBody">A string containing the body of the file</param>
    /// <returns>Optimized string containing the body of the file</returns>
    public delegate String Optimizer(string fileBody);

    /// <summary>
    /// Bundles and minifies, Images, Fonts, CSS, JS, and Html into one optimized html file.
    /// </summary>
    public class Bundler
    {
        /// <summary>
        /// Function to optimize HTML EX: bundler.OptimizeHtml = (html => minifier.MinifyStyleSheet(html))
        /// </summary>
        public Optimizer OptimizeHtml { get; set; }
        /// <summary>
        /// Function to optimize Css EX: bundler.OptimizeCss = (css => minifier.MinifyStyleSheet(css))
        /// </summary>
        public Optimizer OptimizeCss { get; set; }
        /// <summary>
        /// Function to optimize JS EX: bundler.OptimizeJs = (js => minifier.MinifyStyleSheet(js))
        /// </summary>
        public Optimizer OptimizeJs { get; set; }

        /// <summary>
        /// Bundles and minifies, Images, Fonts, CSS, JS, and Html into one optimized html file.
        /// </summary>
        /// <param name="inputPath">The path for the starting page of the Website. (ex. c:\MyWebsite\Intex.html)</param>
        /// <param name="outputPath">The path where the bundled file should be saved</param>
        public void BundleToFile(string inputPath, string outputPath)
        {
            var inputFile = new WebFile(inputPath);
            var ouputFile = Bundle(inputFile);
            ouputFile.Save(outputPath);
        }

        /// <summary>
        /// Bundles and minifies, Images, Fonts, CSS, JS, and Html into a string.
        /// </summary>
        /// <param name="inputPath">The path for the starting page of the Website. (ex. c:\MyWebsite\Intex.html)</param>
        /// <returns>String containing the website</returns>
        public string BundleToString(string inputPath)
        {
            var inputFile = new WebFile(inputPath);
            var outputFile = Bundle(inputFile);
            return outputFile.Body;
        }

        private WebFile Bundle(WebFile inputFile)
        {
            Contract.Requires(inputFile.Body.Contains("<head>") || inputFile.Body.Contains("<body>"), "Html must contain <head> and <body> tags.");
            var outputFile = inputFile;

            //Bundling 2nd Layer (Font and Images)
            foreach (var f in inputFile.DependencyList.Where(x => x.DependencyList.Any()))
                foreach (var d in f.DependencyList)
                    f.Body = f.Body.Replace(d.ReferenceUri, d.DataUri);

            //Bundling 1st Layer Text-Based (JS and CSS)
            var html = inputFile.Body;
            var css = inputFile.DependencyList.Where(x => x.MimeType.Contains("css"))
                .Aggregate(String.Empty, (current, dependency) => current + dependency.Body);
            var js = inputFile.DependencyList.Where(x => x.MimeType.Contains("javascript"))
                .Aggregate(String.Empty, (current, dependency) => current + dependency.Body);

            //Optimization
            if(OptimizeHtml != null)
                html = OptimizeHtml(html);
            if (OptimizeCss != null)
                css = OptimizeCss(css);
            if (OptimizeJs != null)
                js = OptimizeJs(js);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //Remove Imported dependency nodes 
            foreach (var n in inputFile.DependencyList.Where(x => x.MimeType.Contains("css") || x.MimeType.Contains("javascript"))
                .SelectMany(d => doc.DocumentNode.SelectNodes("//*[contains(@src, '" + d.ReferenceUri + "')] | //*[contains(@href,'" + d.ReferenceUri + "')]")))
                n.Remove();

            //Add style
            var header = doc.DocumentNode.Descendants("head").First();
            header.AppendChild(HtmlNode.CreateNode(String.Format("<style>{0}</style>", css)));

            //Add script
            var body = doc.DocumentNode.Descendants("body").First();
            body.AppendChild(HtmlNode.CreateNode(String.Format("<script>{0}</script>", js)));

            //Bundling 1st Layer Binary (Images)
            foreach (var d in inputFile.DependencyList.Where(x => !x.MimeType.Contains("css") || !x.MimeType.Contains("javascript")))
                doc.DocumentNode.InnerHtml = doc.DocumentNode.InnerHtml.Replace(d.ReferenceUri, d.DataUri);

            outputFile.Body = doc.DocumentNode.OuterHtml;
            return outputFile;
        }
    }
}
