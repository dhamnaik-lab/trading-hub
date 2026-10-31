using Avalonia.Controls;
using Avalonia.Input;

namespace TradingHub.App.Services;

/// <summary>Una acción que se puede disparar con un atajo de teclado.</summary>
public sealed record HotkeyDef(string Id, string Group, string Name, string? DefaultGesture = null);

/// <summary>
/// Atajos de teclado configurables (Ajustes → Atajos de teclado). Los de operar vienen sin asignar a
/// propósito: cada uno elige los suyos. En cuentas reales siguen pidiendo confirmación si está activada.
/// </summary>
public static class Hotkeys
{
    public static readonly HotkeyDef[] All =
    {
        new("buy-market", "Trading", "Buy at market"),
        new("sell-market", "Trading", "Sell at market"),
        new("buy-bid", "Trading", "Buy limit at bid"),
        new("sell-ask", "Trading", "Sell limit at ask"),
        new("flatten", "Trading", "Close position"),
        new("reverse", "Trading", "Reverse position"),
        new("cancel-all", "Trading", "Cancel all orders"),
        new("close-25", "Trading", "Close 25%"),
        new("close-50", "Trading", "Close 50%"),
        new("breakeven", "Trading", "Move stop to breakeven"),
        new("brackets", "Trading", "Apply stop and target template"),
        new("qty-next", "Trading", "Quantity: next preset"),
        new("qty-prev", "Trading", "Quantity: previous preset"),

        new("period-m1", "Chart", "Period 1 minute", "Alt+D1"),
        new("period-m5", "Chart", "Period 5 minutes", "Alt+D2"),
        new("period-m15", "Chart", "Period 15 minutes", "Alt+D3"),
        new("period-h1", "Chart", "Period 1 hour", "Alt+D4"),
        new("period-h4", "Chart", "Period 4 hours", "Alt+D5"),
        new("period-d1", "Chart", "Period daily", "Alt+D6"),
        new("style-candles", "Chart", "Candlesticks"),
        new("style-footprint", "Chart", "Footprint"),
        new("style-heikin", "Chart", "Heikin Ashi"),
        new("autoscale", "Chart", "Auto scale"),
        new("go-last", "Chart", "Go to latest price"),
        new("tool-hline", "Chart", "Draw horizontal line", "Alt+H"),
        new("tool-cursor", "Chart", "Cursor (release tool)"),

        new("layout-1", "Windows", "Single-chart layout"),
        new("layout-4", "Windows", "Four-chart layout"),
        new("next-tab", "Windows", "Next tab", "Ctrl+Tab"),
        new("prev-tab", "Windows", "Previous tab", "Ctrl+Shift+Tab"),
        new("detach", "Windows", "Detach chart to window"),
        new("toggle-dom", "Windows", "Show/hide DOM"),
        new("toggle-trader", "Windows", "Show/hide chart trader", "Alt+T"),
        new("toggle-panel", "Windows", "Show/hide right panel"),
    };

    public static string? GestureOf(AppSettings s, string id) =>
        s.Hotkeys.TryGetValue(id, out var g) ? (string.IsNullOrEmpty(g) ? null : g) : All.FirstOrDefault(h => h.Id == id)?.DefaultGesture;

    /// <summary>Texto lindo para mostrar un atajo (⌥1, ⇧B, F5… en la Mac; Alt+1, Shift+B, F5… en Windows).</summary>
    public static string Pretty(string? gesture) => Pretty(gesture, mac: !Os.IsWindows);

    internal static string Pretty(string? gesture, bool mac)
    {
        if (string.IsNullOrEmpty(gesture)) return "—";
        try
        {
            var g = KeyGesture.Parse(gesture);
            string key = g.Key switch
            {
                >= Key.D0 and <= Key.D9 => ((int)(g.Key - Key.D0)).ToString(),
                >= Key.NumPad0 and <= Key.NumPad9 => "Num " + (int)(g.Key - Key.NumPad0),
                Key.Space => "Space",
                Key.Return => mac ? "↩" : "Enter",
                Key.Tab => "Tab",
                Key.Up => "↑",
                Key.Down => "↓",
                Key.Left => "←",
                Key.Right => "→",
                _ => g.Key.ToString(),
            };
            return Keys.Format(g.KeyModifiers, key, mac);
        }
        catch
        {
            return gesture;
        }
    }

    /// <summary>Atajo a partir de una tecla presionada (null si es solo un modificador).</summary>
    public static string? FromKey(Key key, KeyModifiers mods)
    {
        if (key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin or Key.None) return null;
        return new KeyGesture(key, mods).ToString();
    }

    /// <summary>Acción que corresponde a la tecla, o null. No interfiere con lo que se escribe en un campo de texto.</summary>
    public static string? Match(AppSettings s, KeyEventArgs e, TopLevel? top)
    {
        if (e.Key is Key.LeftShift or Key.RightShift or Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LWin or Key.RWin) return null;
        bool typing = top?.FocusManager?.GetFocusedElement() is TextBox;
        bool strongModifier = (e.KeyModifiers & (KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Meta)) != 0;
        bool functionKey = e.Key is >= Key.F1 and <= Key.F24;
        if (typing && !strongModifier && !functionKey) return null;
        foreach (var h in All)
        {
            var g = GestureOf(s, h.Id);
            if (g == null) continue;
            try
            {
                if (KeyGesture.Parse(g).Matches(e)) return h.Id;
            }
            catch
            {
                // atajo mal guardado: se ignora
            }
        }

        return null;
    }
}
