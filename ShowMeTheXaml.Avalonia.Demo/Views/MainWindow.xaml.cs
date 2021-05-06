using Avalonia;
using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace ShowMeTheXaml.Avalonia.Demo.Views {
    public class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DevTools.Attach(this, new KeyGesture(Key.F12));
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}