using Microsoft.VisualStudio.TestTools.UnitTesting;
using PX.Serializers.Json;

namespace UnitTests.Json
{
    [TestClass]
    public class JsonSerializerTests
    {
        [TestMethod]
        public void ShouldCreateNewJsonSerializer()
        {
            var serialize = new JsonSerializer();
            Assert.IsNotNull(serialize);
        }
    }
}
