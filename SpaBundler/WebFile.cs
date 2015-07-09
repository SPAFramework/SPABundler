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
        /// Holds the base path for the website
        /// </summary>
        private readonly string _basePath;

        /// <summary>
        /// Constructs a webfile
        /// </summary>
        /// <param name="path">String path of physical file</param>
        /// <param name="reference">String reference that calls this file</param>
        public WebFile(String path, string reference = null)
        {
            Contract.Requires<FileNotFoundException>(File.Exists(path), "Path must point to an existing web file in the local file system");
            Path = path;
            ReferenceUri = reference;
            Name = new FileInfo(path).Name;
            MimeType = MimeMapping.GetMimeMapping(path);
            Body = File.ReadAllText(Path);
            BodyBytes = File.ReadAllBytes(Path);
            DataUri = "data:" + MimeType + ";base64," + Convert.ToBase64String(BodyBytes);
            _basePath = Path.Replace(Name, string.Empty);
            DependencyList = GetDependencies(Body);
            
        }


        /// <summary>
        /// Populates dependency list with Webfiles from the references in the body of the file passed in
        /// </summary>
        /// <param name="bodyString">The body content of the file</param>
        /// <returns>A list of WebFiles</returns>
        private IList<WebFile> GetDependencies(String bodyString)
        {
            var dependencyList = new List<WebFile>();
            IEnumerable<string> references = null;
            switch (MimeType)
            {
                case "text/html":
                    references= WebFileUtilities.GetHtmlReferences(bodyString);
                    break;
                case "text/css":
                    references = WebFileUtilities.GetCssReferences(bodyString);
                    break;
            }
            if(references != null)
                dependencyList.AddRange(GetValidWebFiles(references));
            return dependencyList;
        }

        /// <summary>
        /// Checks a list of Uri if the physical files exist. If they exist a matching webfile is created.
        /// </summary>
        /// <param name="uriList">Enumerable List of URIs</param>
        /// <returns>Enumerable List of Validated Webfiles</returns>
        private IEnumerable<WebFile> GetValidWebFiles(IEnumerable<string> uriList)
        {
           return uriList.Select(uri => new {Path = WebFileUtilities.GetFullPathFromUri(_basePath, uri), Uri = uri})
                        .Where(reference => File.Exists(reference.Path))
                        .Select(reference => new WebFile(reference.Path, reference.Uri));
        }

        /// <summary>
        /// Save As the WebFile 
        /// </summary>
        /// <param name="path">Path where to save the file</param>
        public void Save(String path)
        {
            using (var sw = new StreamWriter(path, false))
                sw.Write(Body);
        }

    }
}
