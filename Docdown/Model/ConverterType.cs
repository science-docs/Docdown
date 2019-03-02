namespace Docdown.Model
{
    public enum ConverterType
    {
        Undefined,
        Text,
        Markdown,
        Html,
        Latex,
        Pdf,
        Docx
    }

    public static class ConverterTypeExtension
    {
        public static string GetExtension(this ConverterType converterType)
        {
            switch (converterType)
            {
                case ConverterType.Latex:
                    return ".tex";
                case ConverterType.Markdown:
                    return ".md";
                case ConverterType.Undefined:
                case ConverterType.Text:
                    return ".txt";
                default:
                    return "." + converterType.ToString().ToLower();
            }
        }
    }
}
