using System.Xml.Linq;
using Microsoft.AspNetCore.Http;

namespace FactElect.Application.Services;

public interface IXmlSriFileService
{
    Task<XmlSriFileModel> GetAsync(IFormFile file);
}

public class XmlSriFileService : IXmlSriFileService
{
    private const string TagFechaEmision = "fechaEmision";
    private const string TagVersionComp = "version";

    public async Task<XmlSriFileModel> GetAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty", nameof(file));

        XDocument xmlLoaded;

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            xmlLoaded = XDocument.Load(memoryStream);
        }

        var xmlComplete = new XmlDocument();
        using (var xmlReader = xmlLoaded.CreateReader())
        {
            xmlComplete.Load(xmlReader);
        }

        var authorizationNode = xmlComplete.GetElementsByTagName("autorizacion")[0];
        var comprobanteNode = authorizationNode?.SelectSingleNode("comprobante");
        if (comprobanteNode != null)
        {
            var xmlComprobante = new XmlDocument();
            xmlComprobante.LoadXml(comprobanteNode.InnerText);

            RemoveXmlDeclarations(xmlComprobante);

            var infoTributariaNode = xmlComprobante.GetElementsByTagName("infoTributaria")[0];

            var claveAcceso = infoTributariaNode?.SelectSingleNode("claveAcceso")?.InnerText;
            var codDoc = infoTributariaNode?.SelectSingleNode("codDoc")?.InnerText;

            SetVersionAndFechaEmision(xmlComprobante, codDoc, out var versionComp, out var fechaEmision);

            var xmlSri = new XmlDocument();
            xmlSri.LoadXml(authorizationNode.OuterXml);

            RemoveXmlDeclarations(xmlSri);

            var root = xmlSri.GetElementsByTagName("autorizacion")[0];
            if (root != null)
            {
                root.SelectSingleNode("comprobante")!.InnerText = xmlComprobante.OuterXml;
            }

            return new XmlSriFileModel
            {
                CodDoc = codDoc,
                ClaveAcceso = claveAcceso,
                FechaEmision = fechaEmision,
                VersionComp = versionComp,
                XmlSri = xmlSri,
                XmlComprobante = xmlComprobante
            };
        }

        return null;
    }

    private void RemoveXmlDeclarations(XmlDocument xmlDocument)
    {
        var declarations = xmlDocument.ChildNodes.OfType<XmlNode>()
            .Where(x => x.NodeType == XmlNodeType.XmlDeclaration)
            .ToList();

        declarations.ForEach(x => xmlDocument.RemoveChild(x));
    }

    private void SetVersionAndFechaEmision(XmlDocument xmlComprobante, string codDoc, out string versionComp,
        out string fechaEmision)
    {
        switch (codDoc)
        {
            case "01":
                versionComp = GetAttributeValue(xmlComprobante, "factura", TagVersionComp);
                fechaEmision = GetInnerText(xmlComprobante, "infoFactura", TagFechaEmision);
                break;
            case "03":
                versionComp = GetAttributeValue(xmlComprobante, "liquidacionCompra", TagVersionComp);
                fechaEmision = GetInnerText(xmlComprobante, "infoLiquidacionCompra", TagFechaEmision);
                break;
            case "04":
                versionComp = GetAttributeValue(xmlComprobante, "notaCredito", TagVersionComp);
                fechaEmision = GetInnerText(xmlComprobante, "infoNotaCredito", TagFechaEmision);
                break;
            case "05":
                versionComp = GetAttributeValue(xmlComprobante, "notaDebito", TagVersionComp);
                fechaEmision = GetInnerText(xmlComprobante, "infoNotaDebito", TagFechaEmision);
                break;
            case "06":
                versionComp = GetAttributeValue(xmlComprobante, "guiaRemision", TagVersionComp);
                fechaEmision = GetInnerText(xmlComprobante, "infoGuiaRemision", TagFechaEmision);
                break;
            case "07":
                versionComp = GetAttributeValue(xmlComprobante, "comprobanteRetencion", TagVersionComp);
                fechaEmision = GetInnerText(xmlComprobante, "infoCompRetencion", TagFechaEmision);
                break;
            default: throw new InvalidOperationException($"Unsupported CodDoc: {codDoc}");
        }
    }

    private string GetAttributeValue(XmlDocument xmlDocument, string tagName, string attributeName)
    {
        return xmlDocument.GetElementsByTagName(tagName)[0]?.Attributes?[attributeName]?.Value ??
               throw new InvalidOperationException($"Attribute '{attributeName}' not found in tag '{tagName}'");
    }

    private string GetInnerText(XmlDocument xmlDocument, string parentTagName, string childTagName)
    {
        return xmlDocument.GetElementsByTagName(parentTagName)[0]?.SelectSingleNode(childTagName)?.InnerText ??
               throw new InvalidOperationException($"Tag '{childTagName}' not found in '{parentTagName}'");
    }
}
