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
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void StyleSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var styleSource = ((ComboBox)sender).SelectedIndex == 0
                ? App.XamlDisplayAvaloniaEditStyles
                : App.XamlDisplayDefaultStyles;
            Application.Current!.Styles[1] = styleSource;
        }
    }
}