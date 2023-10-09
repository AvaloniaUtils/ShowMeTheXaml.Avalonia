using System;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit;

public class XamlDisplayAvaloniaEditThemeBehavior : Behavior<TextEditor> {
    private IDisposable? _disposable;
    public static readonly AttachedProperty<ThemeName> CodeHighlightThemeNameProperty =
        XamlDisplayAvaloniaEdit.CodeHighlightThemeNameProperty.AddOwner<XamlDisplayAvaloniaEditThemeBehavior>();
    private RegistryOptions? _registryOptions;
    private TextMate.Installation? _textMateInstallation;

    public ThemeName CodeHighlightThemeName {
        get => GetValue(CodeHighlightThemeNameProperty);
        set => SetValue(CodeHighlightThemeNameProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();
        
        _registryOptions = new RegistryOptions(CodeHighlightThemeName);
        _textMateInstallation = AssociatedObject!.InstallTextMate(_registryOptions);
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("xml"));
        
        _disposable = this.GetObservable(CodeHighlightThemeNameProperty)
            .Subscribe(name => {
                _textMateInstallation.SetTheme(_registryOptions.LoadTheme(name));
            });
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree() {
        base.OnDetachedFromVisualTree();
        _disposable?.Dispose();
        _textMateInstallation?.Dispose();
    }
}

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