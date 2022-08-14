using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;

// ReSharper disable once CheckNamespace
namespace ShowMeTheXaml {
    public class XamlDisplay : TemplatedControl {
        public static Dictionary<string, string> DisplayContent
        {
            get
            {
                return _displayContent ?? throw new NullReferenceException("Install ShowMeTheXaml.Avalonia.Generator and call XamlDisplayInternalData.RegisterXamlDisplayData" +
                                                                           "Also check \"Getting started\" on our Github");
            }
            set => _displayContent = value;
        }

        private string? _xamlText;
        private string _uniqueId = null!;
        private AlignmentY _xamlButtonAlignment;

        public string UniqueId {
            get => _uniqueId;
            set {
                _uniqueId = value;
                UpdateXamlText();
            }
        }

        public static readonly DirectProperty<XamlDisplay, string?> XamlTextProperty =
            AvaloniaProperty.RegisterDirect<XamlDisplay, string?>(nameof(XamlText), display => display.XamlText);

        public string? XamlText {
            get => _xamlText;
            private set => SetAndRaise(XamlTextProperty, ref _xamlText, value);
        }

        public static readonly StyledProperty<object> ContentProperty =
            ContentControl.ContentProperty.AddOwner<Panel>();

        [Content]
        public object Content {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly StyledProperty<AlignmentY> XamlButtonAlignmentProperty =
            AvaloniaProperty.Register<XamlDisplay, AlignmentY>(nameof(XamlButtonAlignment), AlignmentY.Bottom);

        private static Dictionary<string, string>? _displayContent;

        private IDisposable? _buttonClickHandler;
        private Popup? _popup;

        public AlignmentY XamlButtonAlignment {
            get => GetValue(XamlButtonAlignmentProperty);
            set => SetAndRaise(XamlButtonAlignmentProperty, ref _xamlButtonAlignment, value);
        }

        private void SourceXamlButtonOnPointerPressed(object sender, PointerPressedEventArgs e) {
            if (_popup != null) {
                _popup.IsOpen = !_popup.IsOpen;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
            base.OnApplyTemplate(e);
            _buttonClickHandler?.Dispose();
            _popup = e.NameScope.Find<Popup>("XamlPopup");
            _buttonClickHandler = e.NameScope.Find<Control>("SourceXamlButton").AddDisposableHandler(PointerPressedEvent, SourceXamlButtonOnPointerPressed);
        }

        private void UpdateXamlText() {
            DisplayContent.TryGetValue(UniqueId, out var xamlText);
            XamlText = xamlText;
        }
    }
}