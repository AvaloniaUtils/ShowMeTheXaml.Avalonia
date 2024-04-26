using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Metadata;

// ReSharper disable once CheckNamespace
namespace ShowMeTheXaml
{
    public class XamlDisplay : TemplatedControl
    {
        public static readonly StyledProperty<string?> XamlTextProperty =
            AvaloniaProperty.Register<XamlDisplay, string?>(nameof(XamlText));

        public static readonly StyledProperty<object?> ContentProperty =
            ContentControl.ContentProperty.AddOwner<Panel>();

        public static readonly StyledProperty<AlignmentY> XamlButtonAlignmentProperty =
            AvaloniaProperty.Register<XamlDisplay, AlignmentY>(nameof(XamlButtonAlignment), AlignmentY.Bottom);

        public static readonly StyledProperty<bool> IsEditableProperty =
            AvaloniaProperty.Register<XamlDisplay, bool>("IsEditable", true);

        private IDisposable? _buttonClickHandler;
        private Popup? _popup;
        private string _uniqueId = null!;

        public XamlDisplay()
        {
#if NETSTANDARD2_1_OR_GREATER
            IsEditable = RuntimeFeature.IsDynamicCodeSupported;
#else
            IsEditable = false;
#endif
        }

        public bool IsEditable
        {
            get => GetValue(IsEditableProperty);
            set => SetValue(IsEditableProperty, value);
        }

        public string UniqueId
        {
            get => _uniqueId;
            set
            {
                _uniqueId = value;
                Reset();
            }
        }

        public string? XamlText
        {
            get => GetValue(XamlTextProperty);
            set => SetValue(XamlTextProperty, value);
        }

        [Content]
        public object? Content
        {
            get => GetValue(ContentProperty);
            set
            {
                if (GetValue(ContentProperty) is ILogical oldLogical) LogicalChildren.Remove(oldLogical);
                SetValue(ContentProperty, value);
                if (value is ILogical newLogical) LogicalChildren.Add(newLogical);
            }
        }

        public AlignmentY XamlButtonAlignment
        {
            get => GetValue(XamlButtonAlignmentProperty);
            set => SetValue(XamlButtonAlignmentProperty, value);
        }

        public Dictionary<string, string> CurrentFileNamespaceAliases =>
            XamlFilesNamespaceAliases[DisplayContent[UniqueId].FileName];

        private void SourceXamlButtonOnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (_popup != null)
            {
                _popup.IsOpen = !_popup.IsOpen;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _buttonClickHandler?.Dispose();
            _popup = e.NameScope.Find<Popup>("XamlPopup");
            _buttonClickHandler = e.NameScope.Find<Control>("SourceXamlButton")
                .AddDisposableHandler(PointerPressedEvent, SourceXamlButtonOnPointerPressed);
        }

        public void Reset()
        {
            if (!DisplayContent.TryGetValue(UniqueId, out var xamlDisplayInstanceData)) return;
            if (!string.IsNullOrEmpty(XamlText))
            {
                Content = AvaloniaRuntimeXamlLoaderHelper.Parse(xamlDisplayInstanceData.Data,
                    CurrentFileNamespaceAliases);
            }

            XamlText = xamlDisplayInstanceData.Data;
        }

        #region ShowMeTheXaml static data

        private static Dictionary<string, XamlDisplayInstanceData>? _displayContent;
        private static Dictionary<string, Dictionary<string, string>>? _xamlFilesNamespaceAliases;

        public static Dictionary<string, XamlDisplayInstanceData> DisplayContent
        {
            get => _displayContent
                   ?? throw new NullReferenceException(
                       "Install ShowMeTheXaml.Avalonia.Generator and call XamlDisplayInternalData.RegisterXamlDisplayData" +
                       "Also check \"Getting started\" on our Github");
            set => _displayContent = value;
        }

        public static Dictionary<string, Dictionary<string, string>> XamlFilesNamespaceAliases
        {
            get => _xamlFilesNamespaceAliases
                   ?? throw new NullReferenceException(
                       "Install ShowMeTheXaml.Avalonia.Generator and call XamlDisplayInternalData.RegisterXamlDisplayData" +
                       "Also check \"Getting started\" on our Github");
            set => _xamlFilesNamespaceAliases = value;
        }

        #endregion
    }
}