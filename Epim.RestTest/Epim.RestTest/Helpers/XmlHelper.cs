using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Epim.RestTest.Helpers
{
    public class XmlHelper
    {
        public T Deserialize<T>(string xml, XmlReaderSettings xmlReaderSettings = null)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentException("xml");
            }

            var serializer = new XmlSerializer(typeof(T));

            var settings = xmlReaderSettings ?? new XmlReaderSettings();

            // No settings need modifying here
            using (var textReader = new StringReader(xml))
            {
                using (var xmlReader = XmlReader.Create(textReader, settings))
                {
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        public string Serialize<T>(T value, XmlWriterSettings xmlWriterSettings = null)
        {
            if (value == null)
            {
                throw new ArgumentException("value");
            }
            var ns = new XmlSerializerNamespaces();
            ns.Add("elhns", "http://www.logisticshub.no/elh");

            var serializer = new XmlSerializer(typeof(T));

            var settings = xmlWriterSettings ?? new XmlWriterSettings
            {
                Encoding = new UnicodeEncoding(false, false),
                Indent = false,
                OmitXmlDeclaration = true
            };

            using (var textWriter = new StringWriter())
            {
                using (var xmlWriter = XmlWriter.Create(textWriter, settings))
                {
                    serializer.Serialize(xmlWriter, value, ns);
                }

                return textWriter.ToString();
            }
        }

    }
}
