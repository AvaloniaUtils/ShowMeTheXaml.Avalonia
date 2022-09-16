namespace ShowMeTheXaml;

public readonly struct XamlDisplayInstanceData {
    public XamlDisplayInstanceData(string data, string fileName) {
        Data = data;
        FileName = fileName;
    }
    public string Data { get; }
    public string FileName { get; }
}