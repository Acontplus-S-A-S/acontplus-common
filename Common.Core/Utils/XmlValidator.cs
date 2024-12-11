using System.Xml.Schema;
using System.Xml;

namespace Common.Core.Utils;
public class ValidationError
{
    public string Message { get; set; }
    public XmlSeverityType Severity { get; set; }
    public int LineNumber { get; set; }
    public int LinePosition { get; set; }
}

public static class XmlValidator
{
    /// <summary>
    /// Validates the provided XmlDocument against an XSD schema file.
    /// </summary>
    /// <param name="xmlDocument">The XML document to validate.</param>
    /// <param name="xsdFilePath">The path to the XSD file.</param>
    /// <returns>A list of ValidationError objects containing error details.</returns>
    public static List<ValidationError> Validate(XmlDocument xmlDocument, Stream xsdStream)
    {
        var validationErrors = new List<ValidationError>();

        if (xmlDocument == null)
            throw new ArgumentNullException(nameof(xmlDocument));
        if (xsdStream == null)
            throw new ArgumentNullException(nameof(xsdStream));

        try
        {

            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, XmlReader.Create(xsdStream));

            // Configure XmlReaderSettings
            XmlReaderSettings settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemaSet
            };

            settings.ValidationEventHandler += (sender, e) =>
            {
                validationErrors.Add(new ValidationError
                {
                    Message = e.Message,
                    Severity = e.Severity,
                    LineNumber = e.Exception?.LineNumber ?? 0,
                    LinePosition = e.Exception?.LinePosition ?? 0
                });
            };

            // Validate XmlDocument
            using (StringReader stringReader = new StringReader(xmlDocument.OuterXml))
            using (XmlReader reader = XmlReader.Create(stringReader, settings))
            {
                while (reader.Read()) { } // Read and validate the entire XML
            }
        }
        catch (XmlException ex)
        {
            validationErrors.Add(new ValidationError
            {
                Message = $"XML Exception: {ex.Message}",
                Severity = XmlSeverityType.Error
            });
        }
        catch (Exception ex)
        {
            validationErrors.Add(new ValidationError
            {
                Message = $"Unexpected Exception: {ex.Message}",
                Severity = XmlSeverityType.Error
            });
        }

        return validationErrors;
    }

    /// <summary>
    /// Exports validation errors to a JSON file.
    /// </summary>
    /// <param name="errors">List of validation errors.</param>
    /// <param name="outputFilePath">The path to save the JSON file.</param>
    public static void ExportErrorsToJson(List<ValidationError> errors, string outputFilePath)
    {
        if (errors == null || errors.Count == 0)
            return;

        string json = System.Text.Json.JsonSerializer.Serialize(errors, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(outputFilePath, json);
    }
}
