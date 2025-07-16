using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit; 

public class XamlDisplayAvaloniaEditTextBindingBehavior : Behavior<Control> {
    public static readonly DirectProperty<XamlDisplayAvaloniaEditTextBindingBehavior, TextEditor> MarkupTextEditorProperty
        = AvaloniaProperty.RegisterDirect<XamlDisplayAvaloniaEditTextBindingBehavior, TextEditor>("MarkupTextEditor",
            o => o.MarkupTextEditor,
            (o, v) => o.MarkupTextEditor = v);
    private TextEditor _markupTextEditor = null!;
    private bool _isTextAssigned;

    public TextEditor MarkupTextEditor {
        get => _markupTextEditor;
        set => SetAndRaise(MarkupTextEditorProperty, ref _markupTextEditor, value);
    }
    
    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();
        if (_isTextAssigned) return;
        MarkupTextEditor.Text = LocateXamlDisplay().XamlText;
        _isTextAssigned = true;
    }
    
    private XamlDisplay LocateXamlDisplay() =>
        AssociatedObject!.FindLogicalAncestorOfType<XamlDisplay>()!;
}