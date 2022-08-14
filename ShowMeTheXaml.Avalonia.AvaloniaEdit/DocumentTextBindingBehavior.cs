using System;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit;

public class DocumentTextBindingBehavior : Behavior<TextEditor> {
    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<DocumentTextBindingBehavior, string>(nameof(Text));

    public string Text {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnAttached() {
        base.OnAttached();

        AssociatedObject!.TextChanged += TextChanged;
        this.GetObservable(TextProperty).Subscribe(TextPropertyChanged);
    }

    protected override void OnDetaching() {
        AssociatedObject!.TextChanged -= TextChanged;
        base.OnDetaching();
    }

    private void TextChanged(object sender, EventArgs eventArgs) {
        if (AssociatedObject!.Document != null) {
            Text = AssociatedObject.Document.Text;
        }
    }

    private void TextPropertyChanged(string? text) {
        if (AssociatedObject!.Document != null && text != null) {
            var caretOffset = AssociatedObject.CaretOffset;
            AssociatedObject.Document.Text = text;
            AssociatedObject.CaretOffset = caretOffset;
        }
    }
}