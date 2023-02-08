using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using ShowMeTheXaml.Avalonia.Demo.Models;

namespace ShowMeTheXaml.Avalonia.Demo.Views {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DevTools.Attach(this, new KeyGesture(Key.F12));
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        private void StyleSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var styleSource = ((ComboBox)sender).SelectedIndex == 0
                ? App.XamlDisplayAvaloniaEditStyles
                : App.XamlDisplayDefaultStyles;
            Application.Current!.Styles[1] = styleSource;
        }

        private void ThemeSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            try {
                var theme = (CatalogTheme)((ComboBox)sender).SelectedItem!;
                if (theme == CatalogTheme.Fluent)
                {
                    Application.Current!.Styles[0] = App.Fluent;
                }
                else if (theme == CatalogTheme.Simple)
                {
                    Application.Current!.Styles[0] = App.Simple;
                }
            }
            catch (Exception exception) {
                Console.WriteLine(exception);
                throw;
            }
        }
    }
}