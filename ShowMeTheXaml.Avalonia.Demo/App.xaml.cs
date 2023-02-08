using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Themes.Simple;
using ShowMeTheXaml.Avalonia.Demo.ViewModels;
using ShowMeTheXaml.Avalonia.Demo.Views;

namespace ShowMeTheXaml.Avalonia.Demo {
    public class App : Application {
        public override void Initialize() {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted() {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                desktop.MainWindow = new MainWindow {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static readonly Uri BaseUri
            = new Uri("avares://ShowMeTheXaml.Avalonia.Demo/App.xaml");

        public static readonly StyleInclude XamlDisplayAvaloniaEditStyles
            = new StyleInclude(BaseUri) { Source = new Uri("avares://ShowMeTheXaml.Avalonia.AvaloniaEdit/XamlDisplayStyles.axaml") };

        public static readonly StyleInclude XamlDisplayDefaultStyles
            = new StyleInclude(BaseUri) { Source = new Uri("avares://ShowMeTheXaml.Avalonia/XamlDisplay.xaml") };

        public static readonly FluentTheme Fluent
            = new FluentTheme();

        public static SimpleTheme Simple
            = new SimpleTheme();
    }
}