using AccountDownloaderLibrary.Models;
using Newtonsoft.Json;

namespace AcountDownloaderLibrary.Tests;


[TestClass]
public class SearchParameterTests
{
    [TestMethod]
    public void TestFeaturedNullNewtonSoft()
    {
        var ap = new AccountDownloaderSearchParameters();

        ap.OnlyFeatured = null;

        var jsonA = JsonConvert.SerializeObject(ap);
        var deSerialize = JsonConvert.DeserializeObject<AccountDownloaderSearchParameters>(jsonA);

        Assert.IsNull(deSerialize?.OnlyFeatured, "OnlyFeatured should only be present if the user included it");

        ap.OnlyFeatured = false;
        jsonA = JsonConvert.SerializeObject(ap);

        deSerialize = JsonConvert.DeserializeObject<AccountDownloaderSearchParameters>(jsonA);

        Assert.IsFalse(deSerialize?.OnlyFeatured, "OnlyFeatured was included and should be false");

    }

    [TestMethod]
    public void TestFeaturedNullNET()
    {
        var ap = new AccountDownloaderSearchParameters();

        ap.OnlyFeatured = null;

        var jsonA = System.Text.Json.JsonSerializer.Serialize(ap);
        var deSerialize = System.Text.Json.JsonSerializer.Deserialize<AccountDownloaderSearchParameters>(jsonA);

        Assert.IsNull(deSerialize?.OnlyFeatured, "OnlyFeatured should only be present if the user included it");

        ap.OnlyFeatured = false;
        jsonA = JsonConvert.SerializeObject(ap);

        deSerialize = JsonConvert.DeserializeObject<AccountDownloaderSearchParameters>(jsonA);

        Assert.IsFalse(deSerialize?.OnlyFeatured, "OnlyFeatured was included and should be false");

    }
}
