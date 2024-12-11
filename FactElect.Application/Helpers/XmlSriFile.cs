using Microsoft.AspNetCore.Http;
using System.Xml.Linq;

namespace FactElect.Application.Helpers;

public class XmlSriFile
{
    public string CodDoc { get; set; }
    public string ClaveAcceso { get; set; }
    public string FechaEmision { get; set; }
    public string VersionComp { get; set; }
    public XmlDocument XmlSri { get; set; }
    public XmlDocument XmlComprobante { get; set; }
    private const string TagFechaEmision = "fechaEmision";
    private const string TagVersionComp = "versionComp";

    public async Task Get(IFormFile file)
    {
        XDocument xmlLoaded;

        using (var str = new MemoryStream())
        {
            await file.CopyToAsync(str);
            str.Position = 0;
            xmlLoaded = XDocument.Load(str);
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
            XmlComprobante = new XmlDocument();
            XmlComprobante.LoadXml(comprobanteNode.InnerText);

            //clean declarations
            var declarationsXmlComprobante = XmlComprobante.ChildNodes.OfType<XmlNode>()
                .Where(x => x.NodeType == XmlNodeType.XmlDeclaration)
                .ToList();
            declarationsXmlComprobante.ForEach(x => XmlComprobante.RemoveChild(x));


            var infoTributariaNode = XmlComprobante.GetElementsByTagName("infoTributaria")[0];

            ClaveAcceso = infoTributariaNode?.SelectSingleNode("claveAcceso")?.InnerText;
            CodDoc = infoTributariaNode?.SelectSingleNode("codDoc")?.InnerText;

            switch (CodDoc)
            {
                case "01":
                    VersionComp = XmlComprobante.GetElementsByTagName("factura")[0]?.Attributes?[TagVersionComp]?.Value;
                    FechaEmision = XmlComprobante.GetElementsByTagName("infoFactura")[0]!
                        .SelectSingleNode(TagFechaEmision)
                        ?.InnerText;
                    break;
                case "03":
                    VersionComp =
                        XmlComprobante.GetElementsByTagName("liquidacionCompra")[0]?.Attributes![TagVersionComp]?.Value;
                    FechaEmision = XmlComprobante.GetElementsByTagName("infoLiquidacionCompra")[0]!
                        .SelectSingleNode(TagFechaEmision)
                        ?.InnerText;
                    break;
                case "04":
                    VersionComp = XmlComprobante.GetElementsByTagName("notaCredito")[0]?.Attributes?[TagVersionComp]
                        ?.Value;
                    FechaEmision = XmlComprobante.GetElementsByTagName("infoNotaCredito")[0]
                        ?.SelectSingleNode(TagFechaEmision)
                        ?.InnerText;
                    break;
                case "05":
                    VersionComp = XmlComprobante.GetElementsByTagName("notaDebito")[0]?.Attributes?[TagVersionComp]
                        ?.Value;
                    FechaEmision = XmlComprobante.GetElementsByTagName("infoNotaDebito")[0]
                        ?.SelectSingleNode(TagFechaEmision)
                        ?.InnerText;
                    break;
                case "06":
                    VersionComp = XmlComprobante.GetElementsByTagName("guiaRemision")[0]?.Attributes?[TagVersionComp]
                        ?.Value;
                    FechaEmision = XmlComprobante.GetElementsByTagName("infoGuiaRemision")[0]
                        ?.SelectSingleNode(TagFechaEmision)
                        ?.InnerText;
                    break;
                case "07":
                    VersionComp = XmlComprobante.GetElementsByTagName("comprobanteRetencion")[0]
                        ?.Attributes?[TagVersionComp]?.Value;
                    FechaEmision = XmlComprobante.GetElementsByTagName("infoCompRetencion")[0]
                        ?.SelectSingleNode(TagFechaEmision)
                        ?.InnerText;
                    break;
            }

            XmlSri = new XmlDocument();
            XmlSri.LoadXml(authorizationNode.OuterXml);


            var declarationsXmlSri = XmlSri.ChildNodes.OfType<XmlNode>()
                .Where(x => x.NodeType == XmlNodeType.XmlDeclaration)
                .ToList();

            declarationsXmlSri.ForEach(x => XmlSri.RemoveChild(x));

            var root = XmlSri.GetElementsByTagName("autorizacion")[0];

            if (root != null)
            {
                root.SelectSingleNode("comprobante")!.InnerText = XmlComprobante.OuterXml;
            }
        }
    }
}
