# SPABundler
SPABundler bundles Images, Fonts, Css, Js and Html into a single html file. It is intended to optimize performance for Single Page Applications.
You can integrate your minifying methods for CSS, JS, and HTML. The bundler can either return a string which can be G-Zipped and sent to client 
or it can save the output into a file.

-In the sample included using Ajax minifier it was able to save ~72KB.  

Usage 

Simple Method:

            //1. Create a bundler object
            var bundler = new Bundler();

			//2a. Get the bundled string
            var result = bundler.BundleToString(@"C:\MyWebsite\Index.html");

            //2b.Save bundled file 
			bundler.BundleToFile(@"C:\MyWebsite\Index.html", @"C:\MyWebsite\BundledIndex.html");


Advanced With Optimizers:

			//1. Create a bundler object
            var bundler = new Bundler();

			//2. Define the Optimizers in any order you like
            bundler.OptimizeHtml = (html =>
            {
                html = Regex.Replace(html, @"// (.*?)\r?\n", "", RegexOptions.Singleline);
                html = Regex.Replace(html, @"\s*\n\s*", "\n");
                html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
                html = Regex.Replace(html, @"<!--(?!\[)(.*?)-->", "");
                return html.Trim();
            });
			//Ajaz Minifier or any function you like
            var minifier = new Minifier();
            bundler.OptimizeCss = (css => minifier.MinifyStyleSheet(css));

            bundler.OptimizeJs = (js => minifier.MinifyJavaScript(js));

			//3a. Get the bundled string
            var result = bundler.BundleToString(@"C:\MyWebsite\Index.html");

            //3b.Save bundled file 
			bundler.BundleToFile(@"C:\MyWebsite\Index.html", @"C:\MyWebsite\BundledIndex.html");

