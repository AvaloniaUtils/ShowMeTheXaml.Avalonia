using System;
using Avalonia;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit;

public class XamlDisplayAvaloniaEditThemeBehavior : Behavior<TextEditor> {
    private IDisposable? _disposable;
    private RegistryOptions? _registryOptions;
    private TextMate.Installation? _textMateInstallation;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();

        var xamlDisplay = AssociatedObject.FindLogicalAncestorOfType<XamlDisplay>()!;
        var themeName = xamlDisplay.GetValue(XamlDisplayAvaloniaEdit.CodeHighlightThemeNameProperty);

        _registryOptions = new RegistryOptions(themeName);
        _textMateInstallation = AssociatedObject!.InstallTextMate(_registryOptions);
        _textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId("xml"));

        _disposable = xamlDisplay.GetObservable(XamlDisplayAvaloniaEdit.CodeHighlightThemeNameProperty)
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