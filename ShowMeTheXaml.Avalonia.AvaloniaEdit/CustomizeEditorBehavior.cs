using System;
using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit; 

/// <remarks>
/// SUITABLE ONLY FOR THIS PROJECT
/// </remarks>
public class CustomizeEditorBehavior : Behavior<TextEditor> {
    #region No width reduction

    private double _originalMinWidth;
    private IDisposable? _boundsChangedObservable;
    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();
        _originalMinWidth = AssociatedObject!.MinWidth;
        _boundsChangedObservable = AssociatedObject.GetObservable(Visual.BoundsProperty)
            .Subscribe(OnBoundsChanged);
    }
    private void OnBoundsChanged(Rect obj) {
        AssociatedObject!.MinWidth = Math.Min(Math.Max(obj.Width, AssociatedObject.MinWidth), AssociatedObject.MaxWidth);
    }

    protected override void OnDetachedFromVisualTree() {
        _boundsChangedObservable?.Dispose();
        AssociatedObject!.MinWidth = _originalMinWidth;
        base.OnDetachedFromVisualTree();
    }

    #endregion
}