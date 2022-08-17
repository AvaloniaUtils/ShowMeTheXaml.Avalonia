using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;

namespace ShowMeTheXaml.Avalonia.Demo.Views {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DevTools.Attach(this, new KeyGesture(Key.F12));
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void StyleToggleSwitch_OnClick(object sender, RoutedEventArgs e) {
            var lastStyle = Application.Current!.Styles.Last();
            Application.Current.Styles.Remove(lastStyle);

            var styleSource = (bool)((ToggleSwitch)sender).IsChecked!
                ? new Uri("avares://ShowMeTheXaml.Avalonia.AvaloniaEdit/XamlDisplayStyles.axaml")
                : new Uri("avares://ShowMeTheXaml.Avalonia/XamlDisplay.xaml");
            var styleInclude = new StyleInclude(new Uri("avares://ShowMeTheXaml.Avalonia.Demo/App.xaml")) {Source = styleSource};
            Application.Current.Styles.Add(styleInclude);
        }
    }
}