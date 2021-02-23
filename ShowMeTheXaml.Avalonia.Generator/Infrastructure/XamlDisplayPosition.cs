using Microsoft.CodeAnalysis.Text;

namespace ShowMeTheXaml.Avalonia.Infrastructure {
    public struct XamlDisplayPosition {
        public LinePositionSpan OpeningTag { get; set; }
        public LinePositionSpan ClosingTag { get; set; }
        public LinePositionSpan XamlDisplaySpan => new LinePositionSpan(OpeningTag.Start, ClosingTag.End);
        public LinePositionSpan ContentSpan { get; set; }
    }
}