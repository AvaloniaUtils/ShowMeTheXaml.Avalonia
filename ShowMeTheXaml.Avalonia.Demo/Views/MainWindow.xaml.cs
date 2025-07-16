using Avalonia;
using Avalonia.Controls;

namespace ShowMeTheXaml.Avalonia.Demo.Views {
    public partial class MainWindow : Window {

        private void StyleSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            var styleSource = ((ComboBox)sender).SelectedIndex == 0
                ? App.XamlDisplayAvaloniaEditStyles
                : App.XamlDisplayDefaultStyles;
            Application.Current!.Styles[1] = styleSource;
        }
    }
}