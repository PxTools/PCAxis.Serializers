using Microsoft.VisualStudio.TestTools.UnitTesting;
using PCAxis.Sdmx.ExtensionMethods;

namespace PCAxis.Sdmx.UnitTest
{
    [TestClass]
    public class SdmxStringExtensionsTest
    {
        [TestMethod]
        public void ShouldReturnCleanID()
        {
            var idToClean = "  tab1  subtab2   ";
            var cleanId = SdmxStringExtensions.CleanID(idToClean);

            Assert.AreEqual("tab1subtab2",cleanId);
        }
    }
}
