using Avalonia;
using TextMateSharp.Grammars;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit;

public static class XamlDisplayAvaloniaEdit {
    public static readonly AttachedProperty<ThemeName> CodeHighlightThemeNameProperty =
        AvaloniaProperty.RegisterAttached<XamlDisplay, ThemeName>("CodeHighlightThemeName", typeof(XamlDisplayAvaloniaEdit));

    public static ThemeName GetCodeHighlightThemeName(XamlDisplay element) {
        return element.GetValue(CodeHighlightThemeNameProperty);
    }

    public static void SetCodeHighlightThemeName(XamlDisplay element, ThemeName value) {
        element.SetValue(CodeHighlightThemeNameProperty, value);
    }
}