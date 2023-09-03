using AccountDownloaderLibrary.Models;
using CloudX.Shared;
using Newtonsoft.Json;
using FluentAssertions;

namespace AcountDownloaderLibrary.Tests;

[TestClass]
public class SearchParameterTests
{

    public T? InOutNewtonsoft<T>(T input)
    {
        var json = JsonConvert.SerializeObject(input);
        return JsonConvert.DeserializeObject<T>(json);
    }
    public T? InOutNet<T>(T input)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(input);
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    [TestMethod]
    public void TestFeaturedNullNewtonSoft()
    {
        var ap = new AccountDownloaderSearchParameters();

        ap.OnlyFeatured = null;

        var result = InOutNewtonsoft(ap);

        Assert.IsNull(result?.OnlyFeatured, "OnlyFeatured should only be present if the user included it");

        ap.OnlyFeatured = false;

        result = InOutNewtonsoft(ap);

        Assert.IsFalse(result?.OnlyFeatured, "OnlyFeatured was included and should be false");

    }

    [TestMethod]
    public void TestFeaturedNullNET()
    {
        var ap = new AccountDownloaderSearchParameters();

        ap.OnlyFeatured = null;

        var result = InOutNet(ap);

        Assert.IsNull(result?.OnlyFeatured, "OnlyFeatured should only be present if the user included it");

        ap.OnlyFeatured = false;


        result = InOutNet(ap);

    }

    [TestMethod]
    public void TestEquivalency()
    {
        var np = new SearchParameters();
        var ap = new AccountDownloaderSearchParameters();
        ap.OnlyFeatured = false;
        InOutNewtonsoft(np)
            .Should()
            .BeEquivalentTo(InOutNewtonsoft(ap));
    }

    [TestMethod]
    public void TestFeaturedSerializedToLiteralNull()
    {
        var ap = new AccountDownloaderSearchParameters();
        var json = JsonConvert.SerializeObject(ap);

        ap.OnlyFeatured = null;

        json.Should().NotContain("\"onlyFeatured\":null");

        json = System.Text.Json.JsonSerializer.Serialize(ap);

        json.Should().NotContain("\"onlyFeatured\":null");

    }
}
