using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit; 

/// <remarks>
/// SUITABLE ONLY FOR THIS PROJECT
/// </remarks>
public class CustomizeEditorBehavior : Behavior<TextEditor> {
    #region No width reduction

    private double originalMinWidth = 0;
    private IDisposable? _boundsChangedObservable;
    protected override void OnAttachedToVisualTree() {
        base.OnAttachedToVisualTree();
        originalMinWidth = AssociatedObject.MinWidth;
        _boundsChangedObservable = AssociatedObject.GetObservable(Visual.BoundsProperty)
            .Subscribe(OnBoundsChanged);
    }
    private void OnBoundsChanged(Rect obj) {
        AssociatedObject.MinWidth = Math.Min(Math.Max(obj.Width, AssociatedObject.MinWidth), AssociatedObject.MaxWidth);
    }

    protected override void OnDetachedFromVisualTree() {
        _boundsChangedObservable?.Dispose();
        AssociatedObject.MinWidth = originalMinWidth;
        base.OnDetachedFromVisualTree();
    }

    #endregion
}