using TradingHub.Localization;

namespace TradingHub.Tests;

public class LocalizationTests
{
    [Fact]
    public void GeneratedSdkIndicatorLabelsUseEnglishOverrides()
    {
        Assert.Equal("Period", Strings.Period);
        Assert.Equal("Settings", Strings.Settings);
        Assert.Equal("High/Low", Strings.HighLow);
        Assert.Equal("Bearish color", Strings.BearishColor);
        Assert.Equal("Alert file", Strings.AlertFile);
    }

    [Fact]
    public void CallerOverridesStillTakePrecedence()
    {
        const string key = "phase5-localization-test-key";
        Assert.Equal("Default", Strings.Get(key, "Default"));
        Strings.Set(key, "Custom value");
        Assert.Equal("Custom value", Strings.Get(key, "Default"));
    }
}
