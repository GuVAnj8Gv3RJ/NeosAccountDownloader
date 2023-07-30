using AccountDownloaderLibrary.Mime;

namespace AcountDownloaderLibrary.Tests
{
    [TestClass]
    public class MimeTest
    {
        [DataTestMethod]
        [DataRow("image/png", "png")]
        [DataRow("application/ogg", "ogg")]
        [DataRow("image/x-targa", "tga")]
        [DataRow("font/otc", "otf")]
        public void TestMimeToExtension(string mime, string extension)
        {
            var ext = MimeDetector.Instance.ExtensionFromMime(mime);

            Assert.AreEqual(extension, ext, "Incorrect extension for mime: " + mime);
        }
    }
}
