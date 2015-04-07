using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Web;

namespace SpaBundler
{
    /// <summary>
    /// Class represents any file type that is used in a Website
    /// </summary>
    internal class WebFile
    {
        /// <summary>
        /// Gets or sets file path
        /// </summary>
        public String Path { get; set; }
        /// <summary>
        /// Gets the name of the file
        /// </summary>
        public String Name { get; private set; }
        /// <summary>
        /// Gets file mime-type
        /// </summary>
        public String MimeType { get; private set; }
        /// <summary>
        /// Gets or sets the file body content 
        /// </summary>
        public String Body { get; set; }
        /// <summary>
        /// Gets file body content as a byte array
        /// </summary>
        public byte[] BodyBytes { get; private set; }
        /// <summary>
        /// Gets body content as Base64 data-uri
        /// </summary>
        public String DataUri { get; private set; }
        /// <summary>
        /// Gets or sets the dependency list of webfiles for the webfile
        /// </summary>
        public IList<WebFile> DependencyList { get; private set; }
        public string ReferenceUri { get; private set; }

        /// <summary>
        /// Constructs a webfile
        /// </summary>
        /// <param name="path">String path of physical file</param>
        /// <param name="reference">String reference that calls this file</param>
        public WebFile(String path, string reference = null)
        {
            Contract.Requires<FileNotFoundException>(File.Exists(path), "Path must point to an existing file");
            Path = path;
            Name = new FileInfo(path).Name;
            MimeType = MimeMapping.GetMimeMapping(path);
            Body = File.ReadAllText(Path);
            BodyBytes = File.ReadAllBytes(Path);
            DataUri = "data:" + MimeType + ";base64," + Convert.ToBase64String(BodyBytes);
            DependencyList = GetDependencies(Path.Replace(Name, string.Empty), Body);
            ReferenceUri = reference;
        }


        private IList<WebFile> GetDependencies(string basePath, String bodyString)
        {
            var dependencyList = new List<WebFile>();
            switch (MimeType)
            {
                case "text/html":
                    dependencyList.AddRange(WebFileUtilities.GetHtmlReferences(bodyString)
                        .Select(uri => new WebFile(WebFileUtilities.GetFullPathFromUri(basePath, uri), uri)));
                    break;
                case "text/css":
                    dependencyList.AddRange(WebFileUtilities.GetCssReferences(Body)
                        .Select(uri => new WebFile(WebFileUtilities.GetFullPathFromUri(basePath, uri), uri)));
                    break;
            }
            return dependencyList;
        }
 
        public void Save(String path)
        {
            using (var sw = new StreamWriter(path, false))
                sw.Write(Body);
        }

    }
}
