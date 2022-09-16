using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Xml;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Media.TextFormatting;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaEdit.Rendering;
using TextRun = AvaloniaEdit.Text.TextRun;
using TextRunProperties = AvaloniaEdit.Text.TextRunProperties;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit;

public class XamlDisplayAvaloniaEditPopupBehavior : XamlDisplayAvaloniaEditTextBindingBehavior {
    public static readonly DirectProperty<XamlDisplayAvaloniaEditPopupBehavior, Button> ApplyButtonProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayAvaloniaEditPopupBehavior, Button>("ApplyButton",
            o => o.ApplyButton,
            (o, v) => o.ApplyButton = v);
    public static readonly DirectProperty<XamlDisplayAvaloniaEditPopupBehavior, Button> ResetButtonProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayAvaloniaEditPopupBehavior, Button>("ResetButton",
            o => o.ResetButton,
            (o, v) => o.ResetButton = v);
    public static readonly DirectProperty<XamlDisplayAvaloniaEditPopupBehavior, TextBox> CommonErrorsTextBoxProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayAvaloniaEditPopupBehavior, TextBox>("CommonErrorsTextBox",
            o => o.CommonErrorsTextBox,
            (o, v) => o.CommonErrorsTextBox = v);
    private Button _applyButton = null!;
    private TextBox _commonErrorsTextBox = null!;
    private Button _resetButton = null!;
    private Dictionary<string, string>? _cachedNamespaceAliases;
    private ErrorsElementGenerator _errorsElementGenerator = new();
    private IDisposable? _previewErrorsObservable;

    public Button ApplyButton {
        get => _applyButton;
        set => SetAndRaise(ApplyButtonProperty, ref _applyButton, value);
    }

    public Button ResetButton {
        get => _resetButton;
        set => SetAndRaise(ResetButtonProperty, ref _resetButton, value);
    }

    public TextBox CommonErrorsTextBox {
        get => _commonErrorsTextBox;
        set => SetAndRaise(CommonErrorsTextBoxProperty, ref _commonErrorsTextBox, value);
    }

    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();
        _previewErrorsObservable = Observable.FromEventPattern(
                action => MarkupTextEditor.TextChanged += action,
                action => MarkupTextEditor.TextChanged -= action)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Select(pattern => MarkupTextEditor.Text)
            .Subscribe(s => LoadMarkupOrPrintErrors(s));
        ResetButton.Click += ResetButtonOnClick;
        ApplyButton.Click += ApplyButtonOnClick;
        if (MarkupTextEditor.TextArea.TextView.ElementGenerators.Contains(_errorsElementGenerator)) return;
        // First time attached to tree
        MarkupTextEditor.TextArea.TextView.ElementGenerators.Add(_errorsElementGenerator);
        CommonErrorsTextBox.IsVisible = false;
    }

    protected override void OnDetachedFromVisualTree() {
        base.OnDetachedFromVisualTree();
        _previewErrorsObservable?.Dispose();
        ResetButton.Click -= ResetButtonOnClick;
        ApplyButton.Click -= ApplyButtonOnClick;
    }

    private void ApplyButtonOnClick(object sender, RoutedEventArgs e) {
        var markupText = MarkupTextEditor.Text;
        var result = LoadMarkupOrPrintErrors(markupText);
        if (result != null) {
            var xamlDisplay = LocateXamlDisplay();
            xamlDisplay.Content = result;
            xamlDisplay.XamlText = markupText;
        }
    }

    private void ResetButtonOnClick(object sender, RoutedEventArgs e) {
        var xamlDisplay = LocateXamlDisplay();
        xamlDisplay.Reset();
        // Force reset text
        MarkupTextEditor.Text = xamlDisplay.XamlText;
    }

    private object? LoadMarkupOrPrintErrors(string xaml) {
        try {
            _cachedNamespaceAliases ??= LocateXamlDisplay().CurrentFileNamespaceAliases;
            var result = AvaloniaRuntimeXamlLoaderHelper.Parse(xaml, _cachedNamespaceAliases);
            CommonErrorsTextBox.IsVisible = false;
            if (_errorsElementGenerator.ExceptionText != null) {
                _errorsElementGenerator.ExceptionPosition = -1;
                _errorsElementGenerator.ExceptionText = null;
                MarkupTextEditor.TextArea.TextView.Redraw();
            }
            return result;
        }
        catch (XmlException e) {
            CommonErrorsTextBox.IsVisible = false;
            var errorLine = MarkupTextEditor.Document.GetLineByNumber(e.LineNumber);
            _errorsElementGenerator.ExceptionPosition = errorLine.Offset + e.LinePosition - 1;
            _errorsElementGenerator.ExceptionText = e.Message;
            MarkupTextEditor.TextArea.TextView.Redraw();
        }
        catch (Exception e) {
            CommonErrorsTextBox.IsVisible = true;
            CommonErrorsTextBox.Text = e.Message;
        }
        return null;
    }

    private XamlDisplay LocateXamlDisplay() =>
        AssociatedObject.FindLogicalAncestorOfType<XamlDisplay>();

    private class ErrorsElementGenerator : VisualLineElementGenerator {
        public int ExceptionPosition { get; set; } = -1;
        public string? ExceptionText { get; set; }

        public override int GetFirstInterestedOffset(int startOffset) {
            return startOffset >= ExceptionPosition ? -1 : ExceptionPosition;
        }

        public override VisualLineElement ConstructElement(int offset) {
            return new ErrorInfoInlineElement(1, 0, ExceptionText!);
        }
    }

    private class ErrorInfoInlineElement : VisualLineElement {
        private readonly string _exceptionText;
        public ErrorInfoInlineElement(int visualLength, int documentLength, string exceptionText) : base(visualLength, documentLength) {
            _exceptionText = exceptionText;
        }
        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context) {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return ErrorInfoObjectRun.CreateInstance(1, TextRunProperties, _exceptionText, context.TextView);
        }
    }

    private class ErrorInfoObjectRun : InlineObjectRun {
        private static readonly ImmutableSolidColorBrush BackgroundSolidColorBrush = new(Colors.Red, 0.3);
        private static readonly PolylineGeometry PolylineGeometry = new() { Points = new Points { new(0, 5), new(5, 0), new(10, 5) } };
        private double? _cachedLineHeight;
        private TextView _textView = null!;
        private ErrorInfoObjectRun(int length, TextRunProperties properties, IControl errorInfoTextBlock)
            : base(length, properties, errorInfoTextBlock) { }
        public static ErrorInfoObjectRun CreateInstance(int length, TextRunProperties properties, string exceptionText, TextView contextTextView) {
            var myRect = new ErrorInfoTextBlock(exceptionText, GetDefaultLineHeight(properties.FontMetrics));
            var testInlineObjectRun = new ErrorInfoObjectRun(length, properties, myRect) { _textView = contextTextView };
            return testInlineObjectRun;
        }

        public override void Draw(DrawingContext drawingContext, Point origin) {
            PolylineGeometry.Transform = new TranslateTransform(origin.X - 5, origin.Y + Math.Round(Properties.FontMetrics.LineHeight) - 3);
            drawingContext.DrawGeometry(BackgroundSolidColorBrush, null, PolylineGeometry);

            var defaultLineHeight = _cachedLineHeight ??= Math.Round(GetDefaultLineHeight(Properties.FontMetrics));
            drawingContext.DrawRectangle(BackgroundSolidColorBrush, null, new Rect(0, origin.Y + defaultLineHeight, _textView.Bounds.Width, Element.DesiredSize.Height - defaultLineHeight));
        }

        private static double GetDefaultLineHeight(FontMetrics fontMetrics) {
            // adding an extra 15% of the line height look good across different font sizes
            var extraLineHeight = fontMetrics.LineHeight * 0.15;
            return fontMetrics.LineHeight + extraLineHeight;
        }
    }

    private class ErrorInfoTextBlock : TextBlock {
        private readonly double _defaultLineHeight;
        public ErrorInfoTextBlock(string text, double defaultLineHeight) {
            _defaultLineHeight = defaultLineHeight;
            Text = text;
            ClipToBounds = false;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            TextWrapping = TextWrapping.Wrap;
            Margin = new Thickness(0, defaultLineHeight, 0, -defaultLineHeight);
        }
        protected override Size MeasureOverride(Size availableSize) {
            var textView = (TextView)this.GetVisualAncestors().FirstOrDefault(visual => visual is TextView)!;
            var (_, height) = base.MeasureOverride(new Size(textView.Bounds.Width, double.PositiveInfinity));
            return new Size(1, _defaultLineHeight + height);
        }

        protected override void ArrangeCore(Rect finalRect) {
            base.ArrangeCore(finalRect);
            Bounds = Bounds.WithX(0);
        }
    }
}