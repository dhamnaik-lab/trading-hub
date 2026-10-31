using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TradingHub.App.Services;
using TradingHub.Charting;
using TradingHub.Charting.Dom;
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
        Assert.Equal("Start time", Strings.StartTime);
        Assert.Equal("Trades", Strings.Trades);
        Assert.Equal("Volume", Strings.Volume);
        Assert.Equal("Show value", Strings.ShowValue);
        Assert.Equal("Font", Strings.Font);
    }

    [Fact]
    public void RiskAgentPromptUsesEnglishAndPreservesStructuredOutputAndLimits()
    {
        var agent = typeof(StrategyTemplates).Assembly.GetType("TradingHub.App.Services.LiveRiskAgent");
        Assert.NotNull(agent);
        var promptField = agent!.GetField("SystemPrompt",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(promptField);
        var prompt = Assert.IsType<string>(promptField!.GetRawConstantValue());

        Assert.Contains("Respond in clear English", prompt);
        Assert.Contains("max_daily_loss", prompt);
        Assert.Contains("max_drawdown", prompt);
        Assert.Contains("allocations", prompt);
        Assert.Contains("summary", prompt);
        Assert.Contains("exact strategy key", prompt);
        Assert.Contains("±50%", prompt);
        Assert.Contains("0.6", prompt);
        Assert.Contains("[[ROL:live-risk]]", prompt);
        Assert.DoesNotContain("Respondé en español", prompt);
    }

    [Fact]
    public void CallerOverridesStillTakePrecedence()
    {
        const string key = "phase5-localization-test-key";
        Assert.Equal("Default", Strings.Get(key, "Default"));
        Strings.Set(key, "Custom value");
        Assert.Equal("Custom value", Strings.Get(key, "Default"));
    }

    [Fact]
    public void StrategyExamplesKeepStableFileAndClassIdentifiers()
    {
        var examples = StrategyTemplates.All.Where(t => t.Example).ToArray();
        Assert.Equal(
            new[] { "CruceDeMedias", "RupturaDeRango", "AbsorcionConDelta", "ReversionVwap" },
            examples.Select(t => t.FileName).ToArray());

        var crossover = examples[0];
        Assert.Equal("Moving average crossover", crossover.Name);
        var code = crossover.Code(crossover.FileName);
        Assert.Contains("public class CruceDeMedias : Strategy", code);
        Assert.Contains("[Parameter(\"Fast EMA\")", code);
    }

    [Fact]
    public void DomAndFootprintDisplayLabelsAreEnglishWithoutChangingKeys()
    {
        var buyColumn = typeof(DomColumn).GetField(nameof(DomColumn.BuyOrders))!;
        Assert.Equal("Buy orders", buyColumn.GetCustomAttribute<DisplayAttribute>()?.GetName());

        var kind = typeof(FootprintColumnKind).GetField(nameof(FootprintColumnKind.Ladder))!;
        Assert.Equal("Ladder (numbers)", kind.GetCustomAttribute<DisplayAttribute>()?.GetName());

        var width = typeof(FootprintColumn).GetProperty(nameof(FootprintColumn.Width))!;
        Assert.Equal("Relative to other columns in the candle", width.GetCustomAttribute<DisplayAttribute>()?.GetDescription());

        // Stored preset names remain unchanged so previously saved layouts can still match.
        Assert.Equal("Bid x Ask (clásico)", FootprintLayout.Presets[0].Name);
        Assert.Equal("Bid × Ask (classic)", FootprintLayout.DisplayPresetName(FootprintLayout.Presets[0].Name));
        Assert.Equal("User preset", FootprintLayout.DisplayPresetName("User preset"));
    }
}
