# ShowMeTheXaml.Avalonia
A Avalonia component making it easy to show the corresponding XAML for custom styles and controls. 
It was built out of a need to shows the XAML for the theme library Material.Avalonia.

## Getting started
1. Install `ShowMeTheXaml.Avalonia.Generator` (nuget package)[]. This will also install the `ShowMeTheXaml.Avalonia` (nuget package)[] as well.
2. Add the following code to csproj file
```c#
<ItemGroup>
    <AdditionalFiles Include="**\*.xaml"/>
    <AdditionalFiles Include="**\*.axaml"/>
</ItemGroup>
```
3. Initialize `DisplayContent` dictionary in `XamlDisplay` class 

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

4. Add `XamlDisplay` in your xaml. And set unique `UniqueId` property value
```xaml
<showMeTheXaml:XamlDisplay UniqueId="123">
    <!-- Your code here -->
</showMeTheXaml:XamlDisplay>
```