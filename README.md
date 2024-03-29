# ShowMeTheXaml.Avalonia

An Avalonia component making it easy to show the corresponding XAML for custom styles and controls. It was built out of a
need to shows the XAML for the theme library [Material.Avalonia](https://github.com/AvaloniaCommunity/Material.Avalonia).

## Getting started

1. Install `ShowMeTheXaml.Avalonia.Generator` [nuget package](https://www.nuget.org/packages/ShowMeTheXaml.Avalonia.Generator/):
    ```shell
    dotnet add package ShowMeTheXaml.Avalonia.Generator
    ```
    This will also install the `ShowMeTheXaml.Avalonia` [nuget package](https://www.nuget.org/packages/ShowMeTheXaml.Avalonia/) as well.

2. Add XamlDisplay style to your app in `App.xaml`. See the example of `App.xaml`:
    ```xaml
    <Application ...>
        ...
        <Application.Styles>
            ...
            <!-- This line \/ required -->
            <StyleInclude Source="avares://ShowMeTheXaml.Avalonia/XamlDisplay.xaml"/>
            <!-- This line /\ required -->
        </Application.Styles>
        ...
    </Application>
    ```

3. Initialize `DisplayContent` dictionary in `XamlDisplay` class:  
    Add `UseXamlDisplay()` in `Program.cs` to `BuildAvaloniaApp` method. It should look like this:
    
    ```c#
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
                     .UsePlatformDetect()
                     .LogToTrace()
                     // This line \/
                     .UseXamlDisplay()
                     // This line /\
                     .UseReactiveUI();
    ```
    Or call `XamlDisplayInternalData.RegisterXamlDisplayData()` on your program startup.

4. Add `XamlDisplay` in your xaml. Set unique `UniqueId` property value. Example:
    ```xaml
    <showMeTheXaml:XamlDisplay UniqueId="123">
        <!-- Your code here -->
    </showMeTheXaml:XamlDisplay>
    ```

---

# ShowMeTheXaml.Avalonia.AvaloniaEdit

Style for displaying xaml content inside [AvaloniaEdit (AvalonEdit)](https://github.com/AvaloniaUI/AvaloniaEdit)

## Getting started
Refer to usual [getting started](https://github.com/AvaloniaUtils/ShowMeTheXaml.Avalonia#getting-started) **but:**
1. Instead `ShowMeTheXaml.Avalonia` use (install) `ShowMeTheXaml.Avalonia.AvaloniaEdit` [nuget package](https://www.nuget.org/packages/ShowMeTheXaml.Avalonia.AvaloniaEdit/)
    ```shell
    dotnet add package ShowMeTheXaml.Avalonia.AvaloniaEdit
    ```
2. Use another style. Instead
    ```xaml
    <StyleInclude Source="avares://ShowMeTheXaml.Avalonia/XamlDisplay.xaml"/>
    ```
    use
    ```xaml
    <StyleInclude Source="avares://ShowMeTheXaml.Avalonia.AvaloniaEdit/XamlDisplayStyles.axaml"/>
    ```

Everything else remains the same.

---

# Compiling sources

1. Clone this repo:
    ```shell
    git clone https://github.com/AvaloniaUtils/ShowMeTheXaml.Avalonia.git
    ```

2. Navigate to repo folder
3. Fetch all submodules:
    ```shell
    git submodule update --init --recursive
    ```

4. Compile project:
    ```shell
    dotnet build
    ```