using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;

namespace ShowMeTheXaml;

public class XamlDisplayPopupBehavior : Behavior<Control> {
    public static readonly DirectProperty<XamlDisplayPopupBehavior, Button> ApplyButtonProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayPopupBehavior, Button>("ApplyButton",
            o => o.ApplyButton,
            (o, v) => o.ApplyButton = v);
    public static readonly DirectProperty<XamlDisplayPopupBehavior, Button> ResetButtonProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayPopupBehavior, Button>("ResetButton",
            o => o.ResetButton,
            (o, v) => o.ResetButton = v);
    public static readonly DirectProperty<XamlDisplayPopupBehavior, TextBox> MarkupErrorsTextBoxProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayPopupBehavior, TextBox>("MarkupErrorsTextBlock",
            o => o.MarkupErrorsTextBox,
            (o, v) => o.MarkupErrorsTextBox = v);
    public static readonly DirectProperty<XamlDisplayPopupBehavior, TextBox> MarkupTextBoxProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayPopupBehavior, TextBox>("MarkupTextBox",
            o => o.MarkupTextBox,
            (o, v) => o.MarkupTextBox = v);
    private Button _applyButton = null!;
    private TextBox _markupErrorsTextBlock = null!;
    private TextBox _markupTextBox = null!;
    private Button _resetButton = null!;
    private Dictionary<string, string>? _cachedNamespaceAliases;
    private IDisposable? _previewErrorsObservable;

    public Button ApplyButton {
        get => _applyButton;
        set => SetAndRaise(ApplyButtonProperty, ref _applyButton, value);
    }

    public Button ResetButton {
        get => _resetButton;
        set => SetAndRaise(ResetButtonProperty, ref _resetButton, value);
    }

    public TextBox MarkupErrorsTextBox {
        get => _markupErrorsTextBlock;
        set => SetAndRaise(MarkupErrorsTextBoxProperty, ref _markupErrorsTextBlock, value);
    }

    public TextBox MarkupTextBox {
        get => _markupTextBox;
        set => SetAndRaise(MarkupTextBoxProperty, ref _markupTextBox, value);
    }

    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();
        _previewErrorsObservable = MarkupTextBox
            .GetObservable(TextBox.TextProperty)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(SynchronizationContext.Current)
            .Subscribe(s => LoadMarkupOrPrintErrors(s));
        ResetButton.Click += ResetButtonOnClick;
        ApplyButton.Click += ApplyButtonOnClick;
    }

    protected override void OnDetachedFromVisualTree() {
        base.OnDetachedFromVisualTree();
        _previewErrorsObservable?.Dispose();
        ResetButton.Click -= ResetButtonOnClick;
        ApplyButton.Click -= ApplyButtonOnClick;
    }

    private void ApplyButtonOnClick(object sender, RoutedEventArgs e) {
        var result = LoadMarkupOrPrintErrors(MarkupTextBox.Text);
        if (result != null) {
            var xamlDisplay = LocateXamlDisplay();
            xamlDisplay.Content = result;
            xamlDisplay.XamlText = MarkupTextBox.Text;
        }
    }
    
    private void ResetButtonOnClick(object sender, RoutedEventArgs e) {
        var xamlDisplay = LocateXamlDisplay();
        xamlDisplay.Reset();
        // Force reset text
        MarkupTextBox.Text = xamlDisplay.XamlText;
        LoadMarkupOrPrintErrors(xamlDisplay.XamlText!);
    }

    private object? LoadMarkupOrPrintErrors(string xaml) {
        try {
            _cachedNamespaceAliases ??= LocateXamlDisplay().CurrentFileNamespaceAliases;
            var result = AvaloniaRuntimeXamlLoaderHelper.Parse(xaml, _cachedNamespaceAliases);
            MarkupErrorsTextBox.Text = string.Empty;
            return result;
        }
        catch (XmlException e) {
            MarkupErrorsTextBox.Text = e.Message;
        }
        catch (Exception e) {
            MarkupErrorsTextBox.Text = e.Message;
        }
        return null;
    }

    private XamlDisplay LocateXamlDisplay() =>
        AssociatedObject.FindLogicalAncestorOfType<XamlDisplay>();
}