using System.CodeDom.Compiler;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpaBundler;

namespace SpaBundlerTests
{
    [TestClass]
    public class WebFileUtilitiesTests
    {
        [TestMethod]
        public void GetHtmlRefferencesTest_HtmlWithoutReferences()
        {
            const string testHtml = "This text has no html refferences" +
                "<!--<link href=\"../Styles/bundle.css\" rel=\"stylesheet\" />-->";
            var result = WebFileUtilities.GetHtmlReferences(testHtml);
            Assert.IsTrue(!result.Any(), "Should not return empty list");
        }

        [TestMethod]
        public void GetHtmlRefferencesTest_HtmlWithReferences()
        {
            const string testHtml = "<html><body>" +
                "<script src=\"../Scripts/bundle.js\"></script>" +
                "<link href=\"../Styles/Main.css\" rel=\"stylesheet\" />" +
                "<img src=\"Images/Logo.png\">" +
                "</body></html>";

            var result = WebFileUtilities.GetHtmlReferences(testHtml);
            Assert.IsTrue(result.Count == 3, "Should return 3 refferences");
            Assert.AreEqual("../Scripts/bundle.js", result[0]);
            Assert.AreEqual("../Styles/Main.css", result[1]);
            Assert.AreEqual("Images/Logo.png", result[2]);
        }

        [TestMethod]
        public void GetCssReferences_CssWithoutReferences()
        {
            const string testCss = "body p {background-color: red;}" +
                "/*url('../Fonts/AppIcons.svg#AppIcons') format('svg');*/";
            var result = WebFileUtilities.GetCssReferences(testCss);
            Assert.IsTrue(!result.Any(), "Should return empty list");
        }

        [TestMethod]
        public void GetCssReferences_CssWithReferences()
        {
            const string testCss = "background: url('../Images/logo_light.png') no-repeat;"
                                   + " @font-face {" +
                                   "font-family: 'AppIcons';" +
                                   "src:url('../Fonts/AppIcons.eot');" +
                                   "src: url('../Fonts/AppIcons.eot') format('embedded-opentype')," +
                                   "url('../Fonts/AppIcons.woff') format('woff')," +
                                   "url('../Fonts/AppIcons.ttf') format('truetype');" +
                                   "font-weight: lighter;}";
            var result = WebFileUtilities.GetCssReferences(testCss);
            Assert.IsTrue(result.Count == 5, "Should return only 5 items");
            Assert.AreEqual("../Images/logo_light.png", result[0]);
            Assert.AreEqual("../Fonts/AppIcons.eot", result[1]);
            Assert.AreEqual("../Fonts/AppIcons.eot", result[2]);
        }

        [TestMethod]
        public void GetFullPathFromUriTest_UriWithDotSegments()
        {
            const string basePath = @"C:\WebSites\SampleSite\Views\";
            const string uri = "../Images/Logo.png";
            var result = WebFileUtilities.GetFullPathFromUri(basePath, uri);
            Assert.AreEqual("C:\\WebSites\\SampleSite\\Images\\Logo.png", result);
        }

        [TestMethod]
        public void GetFullPathFromUriTest_UriWithoutDotSegments()
        {
            const string basePath = @"C:\WebSites\SampleSite\Views\";
            const string uri = "Images/Logo.png";
            var result = WebFileUtilities.GetFullPathFromUri(basePath, uri);
            Assert.AreEqual("C:\\WebSites\\SampleSite\\Views\\Images\\Logo.png", result);
        }



    }
}