# ShowMeTheXaml.Avalonia
A Avalonia component making it easy to show the corresponding XAML for custom styles and controls. 
It was built out of a need to shows the XAML for the theme library Material.Avalonia.

## Getting started
1. Install `ShowMeTheXaml.Avalonia.Generator` [nuget package](https://www.nuget.org/packages/ShowMeTheXaml.Avalonia.Generator/). This will also install the `ShowMeTheXaml.Avalonia` [nuget package](https://www.nuget.org/packages/ShowMeTheXaml.Avalonia/) as well.
2. Add the following code to csproj file
```c#
<ItemGroup>
    <AdditionalFiles Include="**\*.xaml"/>
    <AdditionalFiles Include="**\*.axaml"/>
</ItemGroup>
```
3. Add XamlDisplay style to your app in `App.xaml`. See the example of `App.xaml`:
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
4. Initialize `DisplayContent` dictionary in `XamlDisplay` class 

Add `UseXamlDisplay()` in `Program.cs` to `BuildAvaloniaApp` method.
It should look like this:
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

5. Add `XamlDisplay` in your xaml. Set unique `UniqueId` property value. Example:
```xaml
<showMeTheXaml:XamlDisplay UniqueId="123">
    <!-- Your code here -->
</showMeTheXaml:XamlDisplay>
```

## Compiling sources
1. Clone this repo:
```
git clone https://github.com/AvaloniaUtils/ShowMeTheXaml.Avalonia.git
```
2. Navigate to repo folder
3. Fetch all submodules:
```
git submodule update --init --recursive
```
4. Compile project:
```
dotnet build
```