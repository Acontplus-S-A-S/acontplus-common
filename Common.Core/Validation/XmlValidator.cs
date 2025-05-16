using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Common.Core.Validation;

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
    ///     Validates the provided XmlDocument against an XSD schema file.
    /// </summary>
    /// <param name="xmlDocument">The XML document to validate.</param>
    /// <param name="xsdFilePath">The path to the XSD file.</param>
    /// <returns>A list of ValidationError objects containing error details.</returns>
    public static List<ValidationError> Validate(XmlDocument xmlDocument, Stream xsdStream)
    {
        var validationErrors = new List<ValidationError>();

        if (xmlDocument == null)
        {
            throw new ArgumentNullException(nameof(xmlDocument));
        }

        if (xsdStream == null)
        {
            throw new ArgumentNullException(nameof(xsdStream));
        }

        try
        {
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, XmlReader.Create(xsdStream));

            // Configure XmlReaderSettings
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, Schemas = schemaSet };

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
            using (var stringReader = new StringReader(xmlDocument.OuterXml))
            using (var reader = XmlReader.Create(stringReader, settings))
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
    ///     Exports validation errors to a JSON file.
    /// </summary>
    /// <param name="errors">List of validation errors.</param>
    /// <param name="outputFilePath">The path to save the JSON file.</param>
    public static void ExportErrorsToJson(List<ValidationError> errors, string outputFilePath)
    {
        if (errors == null || errors.Count == 0)
        {
            return;
        }

        var json = JsonSerializer.Serialize(errors, new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(outputFilePath, json);
    }
    /// <summary>
    /// Limpia un XML para hacerlo compatible con SQL Server
    /// </summary>
    public static string CleanXmlForSqlServer(string xml)
    {
        // Si el XML está vacío o es nulo, retornarlo tal cual
        if (string.IsNullOrWhiteSpace(xml))
            return xml;
        try
        {
            // 1. Eliminar la declaración XML (<?xml version="1.0" encoding="UTF-8"?>)
            xml = Regex.Replace(xml, @"<\?xml.*?\?>", "", RegexOptions.Singleline).TrimStart();

            // 2. Eliminar caracteres BOM (Byte Order Mark) si existen
            xml = RemoveBomChars(xml);

            // 3. Corregir ampersands no escapados (&) que no sean parte de entidades XML
            xml = EscapeUnescapedAmpersands(xml);

            // 4. Normalizar saltos de línea
            xml = NormalizeLineBreaks(xml);

            // 5. Eliminar caracteres no válidos para XML
            xml = RemoveInvalidXmlChars(xml);

            return xml;
        }
        catch (Exception ex)
        {
            // En caso de error, al menos eliminar la declaración XML
            return RemoveXmlDeclaration(xml);
        }
    }

    /// <summary>
    /// Escapa los ampersands (&) que no sean parte de entidades XML válidas
    /// </summary>
    private static string EscapeUnescapedAmpersands(string xml)
    {
        // Patrón para encontrar ampersands no escapados
        // Un ampersand es considerado no escapado si no es seguido por:
        // 1. Una entidad XML predefinida (amp;, lt;, gt;, quot;, apos;)
        // 2. Una referencia numérica (&#123; o &#xABC;)
        // 3. El inicio de una referencia de entidad que termina con ;
        return Regex.Replace(
            xml,
            @"&(?!(amp;|lt;|gt;|quot;|apos;|#[0-9]+;|#x[0-9a-fA-F]+;|\w+;))",
            "&amp;",
            RegexOptions.IgnoreCase
        );
    }

    /// <summary>
    /// Elimina caracteres BOM (Byte Order Mark) que pueden causar problemas de codificación
    /// </summary>
    private static string RemoveBomChars(string xml)
    {
        // BOM para UTF-8: EF BB BF
        if (xml.StartsWith("\xEF\xBB\xBF"))
            xml = xml.Substring(3);
        // Otros BOM comunes
        if (xml.StartsWith("\xFE\xFF") || xml.StartsWith("\xFF\xFE"))
            xml = xml.Substring(2);
        return xml;
    }

    /// <summary>
    /// Normaliza los saltos de línea para evitar problemas con diferentes sistemas operativos
    /// </summary>
    private static string NormalizeLineBreaks(string xml)
    {
        // Convertir todos los tipos de saltos de línea a \n
        return Regex.Replace(xml, @"\r\n?|\n", "\n");
    }

    /// <summary>
    /// Elimina caracteres que no son válidos en XML según la especificación
    /// </summary>
    private static string RemoveInvalidXmlChars(string xml)
    {
        // Según la especificación XML, estos caracteres no son válidos
        return Regex.Replace(xml, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", "");
    }

    /// <summary>
    /// Método original para eliminar declaración XML
    /// </summary>
    private static string RemoveXmlDeclaration(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml)) return xml;
        // Elimina cualquier declaración como <?xml version="1.0" encoding="UTF-8"?>
        return Regex.Replace(xml, @"<\?xml.*?\?>", "", RegexOptions.Singleline).TrimStart();
    }
}
