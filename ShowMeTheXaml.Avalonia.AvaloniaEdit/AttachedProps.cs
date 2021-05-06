using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using AvaloniaEdit;

namespace ShowMeTheXaml.Avalonia.AvaloniaEdit {
    public class AttachedProps {
        private static List<TextEditor> _editors = new List<TextEditor>();
        static AttachedProps() {
            ExternalTextProperty.Changed.Subscribe(x => {
                var textEditor = x.Sender as TextEditor;
                textEditor.Text = x.NewValue.Value + Environment.NewLine;
                _editors.Add(textEditor);
            });

            IsAutosizeEnabledProperty.Changed.Subscribe(args => {
                var textEditor = args.Sender as TextEditor;
                if (args.NewValue.Value) {
                    textEditor.GetSubject(TextEditor.BoundsProperty).Subscribe(rect => {
                        textEditor.Height = Math.Min(textEditor.TextArea.TextView.DocumentHeight + 8, textEditor.MaxHeight);
                    });
                }
            });
        }

        public static readonly AttachedProperty<string?> ExternalTextProperty =
            AvaloniaProperty.RegisterAttached<AttachedProps, TextEditor, string?>("ExternalText");

        public static string? GetExternalText(TextEditor element) {
            return element.GetValue(ExternalTextProperty);
        }

        public static void SetExternalText(TextEditor element, string? value) {
            element.SetValue(ExternalTextProperty, value);
        }

        /// <summary>
        /// WARN: This only works for this project
        /// DO NOT TRY TO USE IT IN OTHER PROJECTS
        /// </summary>
        public static readonly AttachedProperty<bool> IsAutosizeEnabledProperty = AvaloniaProperty.RegisterAttached<AttachedProps, TextEditor, bool>("IsAutosizeEnabled");

        public static bool GetIsAutosizeEnabled(TextEditor element) {
            return element.GetValue(IsAutosizeEnabledProperty);
        }

        public static void SetIsAutosizeEnabled(TextEditor element, bool value) {
            element.SetValue(IsAutosizeEnabledProperty, value);
        }
    }
}