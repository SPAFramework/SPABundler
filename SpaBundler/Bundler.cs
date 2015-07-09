using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

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
            // ReSharper disable once AssignNullToNotNullAttribute
            Contract.Requires<DirectoryNotFoundException>(outputPath != null && Directory.Exists(Path.GetDirectoryName(outputPath)), "Output path must point to a valid directory");
            Contract.Ensures(File.Exists(outputPath), "The file bundled was not created");
            var ouputFile = Bundle(inputPath);
            ouputFile.Save(outputPath);
        }
        /// <summary>
        /// Bundles and minifies, Images, Fonts, CSS, JS, and Html into a string.
        /// </summary>
        /// <param name="inputPath">The path for the starting page of the Website. (ex. c:\MyWebsite\Intex.html)</param>
        /// <returns>String containing the website</returns>
        public string BundleToString(string inputPath)
        {
            var outputFile = Bundle(inputPath);
            return outputFile.Body;
        }

        /// <summary>
        /// This function bundles CSS, JavaScript, and Images into a single html file
        /// </summary>
        /// <param name="inputPath">The path of the main html file</param>
        /// <returns>A WebFile containg the bundle</returns>
        private WebFile Bundle(String inputPath)
        {
            var inputFile = new WebFile(inputPath);
            var outputFile = inputFile;

            //Adding Font and Images to 2nd Layer
            foreach (var f in inputFile.DependencyList.Where(x => x.DependencyList.Any()))
                foreach (var d in f.DependencyList)
                    f.Body = f.Body.Replace(d.ReferenceUri, d.DataUri);

            //Bundling 1st Layer Text-Based (JS and CSS)
            var html = inputFile.Body;
            var css = BundleDependenciesByType(inputFile.DependencyList, "css");
            var js = BundleDependenciesByType(inputFile.DependencyList, "javascript"); 

            //Optimization
            if(OptimizeHtml != null) html = OptimizeHtml(html);
            if (OptimizeCss != null) css = OptimizeCss(css);
            if (OptimizeJs != null) js = OptimizeJs(js);

            //Remove Imported dependency nodes 
            foreach (var dependency in inputFile.DependencyList.Where(x => (x.MimeType.Contains("css") || x.MimeType.Contains("javascript"))))
                WebFileUtilities.RemoveReferenceNodes(ref html, dependency.ReferenceUri);

            //Add Images to HTML file
            html = inputFile.DependencyList.Where(x => (!x.MimeType.Contains("css") || !x.MimeType.Contains("javascript")))
                .Aggregate(html, (current, d) => current.Replace(d.ReferenceUri, d.DataUri));

            WebFileUtilities.InsertNode(ref html,"head","style", css); //Add Styles
            WebFileUtilities.InsertNode(ref html, "body", "script", js); //Add Scripts

            outputFile.Body = html;
            return outputFile;
        }

        /// <summary>
        /// Joins together web files of same type
        /// </summary>
        /// <param name="webFileList">A list of webfile</param>
        /// <param name="fileType">String phrase contained in the mimetype of the file</param>
        /// <returns>A string containing the bundled files.</returns>
        private static string BundleDependenciesByType(IEnumerable<WebFile> webFileList, string fileType)
        {
            var acceptableTypes = new[] {"css", "javascript"};
            Contract.Requires(acceptableTypes.Contains(fileType), "fileType is invalid");
            return webFileList.Where(x => x.MimeType.Contains(fileType))
                .Aggregate(String.Empty, (current, dependency) => current + dependency.Body);
        }

    }
}
