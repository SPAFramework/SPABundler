using System;
using System.CodeDom;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpaBundler;
using Microsoft.Ajax.Utilities;

namespace SpaBundlerTests
{
    [TestClass]
    public class BundlerTests
    {
        [TestMethod]
        public void BundlerTest()
        {
            var bundler = new Bundler();
            var result = bundler.BundleToString(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\Main.html");
        }

        [TestMethod]
        public void BundlerTest_WithOptimizers()
        {
            var bundler = new Bundler();
            bundler.OptimizeHtml = (html =>
            {
                html = Regex.Replace(html, @"// (.*?)\r?\n", "", RegexOptions.Singleline);
                html = Regex.Replace(html, @"\s*\n\s*", "\n");
                html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
                html = Regex.Replace(html, @"<!--(?!\[)(.*?)-->", "");
                return html.Trim();
            });

            var minifier = new Minifier();
            bundler.OptimizeCss = (css => minifier.MinifyStyleSheet(css));
            bundler.OptimizeJs = (js => minifier.MinifyJavaScript(js));

        var result = bundler.BundleToString(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\Main.html");
        }

        [TestMethod]
        public void BundlerTest_WithOutOptimizers()
        {
            var bundler = new Bundler();
            var result = bundler.BundleToString(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\Main.html");
            Assert.IsTrue(result != null);
        }

        [TestMethod]
        public void BundlerTest_InterferenceWithTypescript()
        {
            var bundler = new Bundler();
            var result = bundler.BundleToString(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\Main.html");
            Assert.IsTrue(result.Contains("<reference path=\"Scripts/test.js\" />"));
        }

        [TestMethod]
        public void BundlerTest_SaveToFile()
        {
            var bundler = new Bundler();
            bundler.OptimizeHtml = (html =>
            {
                html = Regex.Replace(html, @"// (.*?)\r?\n", "", RegexOptions.Singleline);
                html = Regex.Replace(html, @"\s*\n\s*", "\n");
                html = Regex.Replace(html, @"\s*\>\s*\<\s*", "><");
                html = Regex.Replace(html, @"<!--(?!\[)(.*?)-->", "");
                return html.Trim();
            });

            var minifier = new Minifier();
            bundler.OptimizeCss = (css => minifier.MinifyStyleSheet(css));
            bundler.OptimizeJs = (js => minifier.MinifyJavaScript(js));

            bundler.BundleToFile(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\Main.html",
                @"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\BundledMain.html");
            Assert.IsTrue(File.Exists(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\BundledMain.html"));
            
        }
    }
}
