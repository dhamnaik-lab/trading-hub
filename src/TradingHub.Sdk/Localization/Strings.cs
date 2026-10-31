using System.Collections.Concurrent;

namespace TradingHub.Localization
{
    public static partial class Strings
    {
        private static readonly ConcurrentDictionary<string, string> Overrides = new();


        // English defaults for inherited ATAS-compatible display keys.
        // Do not rename generated keys or edit Strings.g.cs. User overrides still win
        // because Strings.Set replaces these initial values when called.
        static Strings()
        {
            (string Key, string Value)[] defaults =
            {
                (nameof(Absorption), "Absorption"),
                (nameof(AlertFile), "Alert file"),
                (nameof(Alerts), "Alerts"),
                (nameof(ApproximationAlert), "Approximation alert"),
                (nameof(ApproximationFilter), "Approximation filter"),
                (nameof(BackGround), "Background"),
                (nameof(BaseLine), "Baseline"),
                (nameof(BearishColor), "Bearish color"),
                (nameof(BearlishColor), "Bearish color"),
                (nameof(Border), "Border"),
                (nameof(BorderColor), "Border color"),
                (nameof(BullishColor), "Bullish color"),
                (nameof(Calculation), "Calculation"),
                (nameof(Candles), "Candles"),
                (nameof(ColoredDirection), "Color by direction"),
                (nameof(Colors), "Colors"),
                (nameof(Data), "Data"),
                (nameof(Days), "Days"),
                (nameof(FontColor), "Font color"),
                (nameof(FontSize), "Font size"),
                (nameof(High), "High"),
                (nameof(HighLow), "High/Low"),
                (nameof(Line), "Line"),
                (nameof(LineStyle), "Line style"),
                (nameof(Location), "Location"),
                (nameof(LongPeriod), "Long period"),
                (nameof(Low), "Low"),
                (nameof(Period), "Period"),
                (nameof(Range), "Range"),
                (nameof(Session), "Session"),
                (nameof(Settings), "Settings"),
                (nameof(ShortPeriod), "Short period"),
                (nameof(Signal), "Signal"),
                (nameof(SignalPeriod), "Signal period"),
                (nameof(Size), "Size"),
                (nameof(Text), "Text"),
                (nameof(TextColor), "Text color"),
                (nameof(Value), "Value"),
                (nameof(Visualization), "Visualization"),
                (nameof(Font), "Font"),
                (nameof(Mode), "Mode"),
                (nameof(Negative), "Negative"),
                (nameof(Positive), "Positive"),
                (nameof(Show), "Show"),
                (nameof(ShowValue), "Show value"),
                (nameof(StartTime), "Start time"),
                (nameof(Trades), "Trades"),
                (nameof(Volume), "Volume"),
                (nameof(ZeroValue), "Zero value"),
            };

            foreach (var (key, value) in defaults)
                Overrides[key] = value;
        }

        /// <summary>Overrides display text at runtime, including user-provided translations.</summary>
        public static void Set(string key, string value) => Overrides[key] = value;

        public static string Get(string key, string fallback) => Overrides.TryGetValue(key, out var v) ? v : fallback;
    }
}
