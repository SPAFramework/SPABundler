using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpaBundler;

namespace SpaBundlerTests
{
    [TestClass]
    public class WebFileTests
    {
        [TestMethod]
        public void DependencyListTest_WithDependencies()
        {
            var wf = new WebFile(@"C:\Users\DONO\Source\Workspaces\Verdicter\Verdicter\SpaBundlerTests\Content\Main.html");
            Assert.AreEqual(wf.DependencyList.Count, 3);
        }
    }
}
