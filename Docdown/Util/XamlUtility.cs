using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Docdown.Util
{
    public static class XamlUtility
    {
        public static bool Validate(XDocument xDocument, XmlSchema xmlSchema)
        {
            XmlSchemaSet set = new XmlSchemaSet();
            set.Add(xmlSchema);

            bool result = true;
            xDocument.Validate(set, (_, __) => result = false);
            return result;
        }

        public static bool ValidateWizard(XDocument xDocument)
        {
            const string location = "Docdown.Resources.Wizards.Wizard.xsd";
            using (var resourceStream = IOUtility.LoadResource(location))
            {
                XmlSchema schema = XmlSchema.Read(resourceStream, (_, __) => { });
                return Validate(xDocument, schema);
            }
        }

        public static object Parse(string xamlString)
        {
            return XamlReader.Parse(xamlString);
        }

        public static async Task<object> ParseAsync(string xamlString)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                return XamlReader.Parse(xamlString);
            });
        }

        public static async Task<object> ParseAsync(XmlNode xamlNode)
        {
            return await ParseAsync(xamlNode.OuterXml);
        }

        public static async Task<object> ParseAsync(XNode xamlNode)
        {
            return await ParseAsync(xamlNode.ToString());
        }

        public static async Task<FlowDocument> ParseDocumentAsync(string xamlString)
        {
            return await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var item = XamlReader.Parse(xamlString);
                    FlowDocument doc = null;
                    switch (item)
                    {
                        case FlowDocument document:
                            doc = document;
                            break;
                        case Block block:
                            doc = new FlowDocument();
                            doc.Blocks.Add(block);
                            break;
                        case Inline inline:
                            doc = new FlowDocument();
                            var paragraph = new Paragraph();
                            paragraph.Inlines.Add(inline);
                            doc.Blocks.Add(paragraph);
                            break;
                    }
                    return doc;
                }
                catch
                {
                    return null;
                }
            });
        }

        public static async Task<FlowDocument> ParseDocumentAsync(XmlNode xamlNode)
        {
            return await ParseDocumentAsync(xamlNode.OuterXml);
        }

        public static async Task<FlowDocument> ParseDocumentAsync(XNode xamlNode)
        {
            return await ParseDocumentAsync(xamlNode.ToString());
        }
    }
}