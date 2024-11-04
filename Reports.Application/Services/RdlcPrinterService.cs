using System.Drawing.Imaging;
using System.Drawing.Printing;
using Common.Core.Utils;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using Reports.Application.Models;

namespace Reports.Application.Services;

public interface IRdlcPrinterService
{
    public bool Print(RdlcPrinter rdlcPrinter, RdlcPrintRequest printRequest);
}

public class RdlcPrinterService : IRdlcPrinterService
{
    public bool Print(RdlcPrinter rdlcPrinter, RdlcPrintRequest printRequest)
    {
        var streams = new List<Stream>();
        using LocalReport lr = new LocalReport();
        lr.LoadReportDefinition(LoadReportDefinition(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            "Reports", rdlcPrinter.FileName)));
        if (printRequest.DataSources != null)
        {
            foreach (var item in printRequest.DataSources)
            {
                lr.DataSources.Add(new ReportDataSource(item.Key,
                    DataConverters.JsonToDataTable(JsonConvert.SerializeObject(item.Value))));
            }
        }

        var reportParams = lr.GetParameters();

        if (reportParams.Count > 0 && printRequest.ReportParams != null)
        {
            foreach (var item in printRequest.ReportParams)
            {
                printRequest.ReportParams.TryGetValue("mimeType", out _);
                if (item.Key == "logo")
                {
                    string logoPath = "";
                    if (Directory.Exists(rdlcPrinter.LogoDirectory))
                    {
                        string[] fileEntries = Directory.GetFiles(rdlcPrinter.LogoDirectory);
                        foreach (var entry in fileEntries)
                        {
                            string fileName = Path.GetFileNameWithoutExtension(entry);
                            if (fileName == rdlcPrinter.LogoName)
                            {
                                logoPath = Path.Combine(rdlcPrinter.LogoDirectory, entry);
                                break;
                            }
                        }
                    }

                    lr.SetParameters(new ReportParameter(item.Key,
                        Convert.ToBase64String(File.ReadAllBytes(logoPath))));
                }
                else
                {
                    lr.SetParameters(new ReportParameter(item.Key, item.Value));
                }
            }
        }

        lr.Render("Image", rdlcPrinter.DeviceInfo, (name, fileNameExtension, encoding, mimeType, willSeek) =>
        {
            var stream = new MemoryStream();
            streams.Add(stream);
            return stream;
        }, out _);

        foreach (Stream stream in streams)
            stream.Position = 0;

        if (streams == null || streams.Count == 0)
        {
            throw new Exception("Error: no stream to print.");
        }

        PrintDocument printDoc = new PrintDocument();
        printDoc.PrinterSettings.PrinterName = rdlcPrinter.PrinterName;
        PrinterSettings pageSettings = new PrinterSettings();
        pageSettings.PrinterName = rdlcPrinter.PrinterName;
        printDoc.DefaultPageSettings = pageSettings.DefaultPageSettings;

        int currentIndex = 0;
        switch (printDoc.PrinterSettings.IsValid)
        {
            case true:
                printDoc.PrintPage += (sender, e) =>
                {
                    Metafile pageImage = new Metafile(streams[currentIndex]);
                    e.Graphics.DrawImage(
                        pageImage, e.PageBounds);
                    currentIndex++;
                    e.HasMorePages = currentIndex < streams.Count;
                };
                printDoc.EndPrint += (sender, e) =>
                {
                    if (streams != null)
                    {
                        foreach (Stream item in streams)
                        {
                            item.Close();
                        }

                        streams.Clear();
                    }
                };
                printDoc.PrinterSettings.Copies = rdlcPrinter.Copies;
                printDoc.Print();

                printDoc.EndPrint += (o, e) =>
                {
                    if (printDoc.PrintController.IsPreview)
                    {
                        
                    }
                };
                return true;
            default:
                return false;
        }
    }

    private static MemoryStream LoadReportDefinition(string filePath)
    {
        MemoryStream memoryStream = new MemoryStream();
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fileStream.CopyTo(memoryStream);
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }
}
