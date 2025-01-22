using System.Xml.Linq;

namespace TXTextControl.DocumentServer.PDF.AcroForms.Xfdf
{
    /// <summary>
    /// Provides methods for processing and generating XFDF (XML Forms Data Format) documents.
    /// </summary>
    public class XfdfProcessing
    {
        /// <summary>
        /// Generates an XFDF string for a set of form fields and associates it with a PDF filename.
        /// </summary>
        /// <param name="formFields">An array of form fields to include in the XFDF.</param>
        /// <param name="filename">The filename of the associated PDF document.</param>
        /// <returns>A string containing the generated XFDF XML.</returns>
        public static string GenerateXfdf(FormField[] formFields, string filename)
        {
            if (formFields == null || formFields.Length == 0)
            {
                throw new ArgumentException("Form fields cannot be null or empty.", nameof(formFields));
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException("Filename cannot be null or empty.", nameof(filename));
            }

            var xfdfFields = new List<XfdfField>();

            foreach (var formField in formFields)
            {
                // Process different types of form fields
                var xfdfField = CreateXfdfField(formField);
                if (xfdfField != null)
                {
                    xfdfFields.Add(xfdfField);
                }
            }

            return GenerateXfdfXml(xfdfFields, filename);
        }

        /// <summary>
        /// Creates an XFDF field representation for a given form field.
        /// </summary>
        /// <param name="formField">The form field to process.</param>
        /// <returns>An XFDF field object or null if the form field type is unsupported.</returns>
        private static XfdfField? CreateXfdfField(FormField formField)
        {
            if (formField == null)
            {
                throw new ArgumentNullException(nameof(formField), "Form field cannot be null.");
            }

            return formField switch
            {
                FormTextField textField => new XfdfField
                {
                    Name = textField.FieldName,
                    Value = textField.Value
                },
                FormCheckBox checkBox => new XfdfField
                {
                    Name = checkBox.FieldName,
                    Value = checkBox.IsChecked ? "On" : "Off"
                },
                FormComboBox comboBox => new XfdfField
                {
                    Name = comboBox.FieldName,
                    Value = comboBox.Value
                },
                FormChoiceField choiceField => new XfdfField
                {
                    Name = choiceField.FieldName,
                    Value = choiceField.Value
                },
                _ => null // Unsupported form field type
            };
        }

        /// <summary>
        /// Parses an XFDF XML string to generate a list of XFDF fields.
        /// </summary>
        /// <param name="xmlContent">The XFDF XML content to parse.</param>
        /// <returns>A list of XFDF fields extracted from the XML.</returns>
        public static List<XfdfField> ParseXfdfFromXml(string xmlContent)
        {
            if (string.IsNullOrEmpty(xmlContent))
            {
                throw new ArgumentException("XML content cannot be null or empty.", nameof(xmlContent));
            }

            var fields = new List<XfdfField>();
            XNamespace ns = "http://ns.adobe.com/xfdf/";

            try
            {
                var document = XDocument.Parse(xmlContent);
                var fieldElements = document.Root?.Element(ns + "fields")?.Elements(ns + "field");

                if (fieldElements != null)
                {
                    foreach (var fieldElement in fieldElements)
                    {
                        var name = fieldElement.Attribute("name")?.Value;
                        var value = fieldElement.Element(ns + "value")?.Value;

                        if (!string.IsNullOrEmpty(name))
                        {
                            fields.Add(new XfdfField
                            {
                                Name = name,
                                Value = value
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to parse XFDF XML.", ex);
            }

            return fields;
        }

        /// <summary>
        /// Generates the XML for the XFDF document.
        /// </summary>
        /// <param name="fields">The fields to include in the XFDF.</param>
        /// <param name="pdfHref">The associated PDF file href.</param>
        /// <returns>A string containing the XFDF XML.</returns>
        private static string GenerateXfdfXml(List<XfdfField> fields, string pdfHref)
        {
            // Define the XFDF namespace
            XNamespace ns = "http://ns.adobe.com/xfdf/";

            // Create the root XFDF element
            var xfdf = new XElement(ns + "xfdf",
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                new XElement(ns + "f", new XAttribute("href", pdfHref)),
                new XElement(ns + "fields",
                    fields.Select(field =>
                    {
                        var fieldElement = new XElement(ns + "field", new XAttribute("name", field.Name));
                        if (!string.IsNullOrEmpty(field.Value))
                        {
                            fieldElement.Add(new XElement(ns + "value", field.Value));
                        }
                        return fieldElement;
                    })
                ),
                new XElement(ns + "ids") // Placeholder for IDs
            );

            // Return the formatted XML string
            return new XDeclaration("1.0", "UTF-8", null) + xfdf.ToString();
        }

        /// <summary>
        /// Represents an XFDF field with a name and value.
        /// </summary>
        public class XfdfField
        {
            public string? Name { get; set; }
            public string? Value { get; set; }
        }
    }
}
