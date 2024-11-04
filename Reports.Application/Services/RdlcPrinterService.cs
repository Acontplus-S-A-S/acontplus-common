using System.Drawing.Imaging;
using System.Drawing.Printing;
using Common.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using Reports.Application.Models;

namespace Reports.Application.Services
{
    public interface IRdlcPrinterService
    {
        bool Print(RdlcPrinter rdlcPrinter, RdlcPrintRequest printRequest);
    }

    public class RdlcPrinterService(IServiceScopeFactory scopeFactory) : IRdlcPrinterService
    {
        public bool Print(RdlcPrinter rdlcPrinter, RdlcPrintRequest printRequest)
        {
            var streams = new List<Stream>();
            try
            {
                using LocalReport lr = new LocalReport();
                string reportPath =
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", rdlcPrinter.FileName);
                lr.LoadReportDefinition(LoadReportDefinition(reportPath));

                // Handle data sources
                if (printRequest.DataSources != null)
                {
                    foreach (var item in printRequest.DataSources)
                    {
                        lr.DataSources.Add(new ReportDataSource(item.Key,
                            DataConverters.JsonToDataTable(JsonConvert.SerializeObject(item.Value))));
                    }
                }

                // Handle report parameters
                if (lr.GetParameters().Count > 0 && printRequest.ReportParams != null)
                {
                    SetReportParameters(lr, rdlcPrinter, printRequest.ReportParams);
                }

                // Render the report as an image
                lr.Render("Image", rdlcPrinter.DeviceInfo, (name, fileNameExtension, encoding, mimeType, willSeek) =>
                {
                    var stream = new MemoryStream();
                    streams.Add(stream);
                    return stream;
                }, out _);

                if (streams.Count == 0)
                {
                    throw new Exception("Error: no stream to print.");
                }

                // Print the document
                return PrintDocumentToPrinter(streams, rdlcPrinter);
            }
            catch (Exception ex)
            {
                // Log the exception
                using var scope = scopeFactory.CreateScope();
                var log = scope.ServiceProvider.GetService<ICustomLogger>();
                log.LogActivity($"Printing failed: \n {ex.StackTrace}");

                return false;
            }
            finally
            {
                // Ensure all streams are closed
                foreach (Stream stream in streams)
                {
                    stream?.Dispose();
                }
            }
        }

        private static MemoryStream LoadReportDefinition(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Report file not found: {filePath}");
            }

            MemoryStream memoryStream = new MemoryStream();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                fileStream.CopyTo(memoryStream);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        private void SetReportParameters(LocalReport lr, RdlcPrinter rdlcPrinter,
            Dictionary<string, string> reportParams)
        {
            foreach (var param in reportParams)
            {
                lr.SetParameters(param.Key == "logo"
                    ? new ReportParameter(param.Key, GetLogoAsBase64(rdlcPrinter))
                    : new ReportParameter(param.Key, param.Value));
            }
        }

        private string GetLogoAsBase64(RdlcPrinter rdlcPrinter)
        {
            string logoPath = FindLogoPath(rdlcPrinter);
            if (string.IsNullOrEmpty(logoPath))
            {
                throw new FileNotFoundException("Logo file not found.");
            }

            return Convert.ToBase64String(File.ReadAllBytes(logoPath));
        }

        private static string FindLogoPath(RdlcPrinter rdlcPrinter)
        {
            if (Directory.Exists(rdlcPrinter.LogoDirectory))
            {
                string[] fileEntries = Directory.GetFiles(rdlcPrinter.LogoDirectory);
                foreach (var entry in fileEntries)
                {
                    string fileName = Path.GetFileNameWithoutExtension(entry);
                    if (fileName == rdlcPrinter.LogoName)
                    {
                        return Path.Combine(rdlcPrinter.LogoDirectory, entry);
                    }
                }
            }

            return string.Empty;
        }

        private bool PrintDocumentToPrinter(List<Stream> streams, RdlcPrinter rdlcPrinter)
        {
            using PrintDocument printDoc = new PrintDocument
            {
                PrinterSettings = new PrinterSettings { PrinterName = rdlcPrinter.PrinterName },
                DefaultPageSettings = new PageSettings
                {
                    PrinterSettings = new PrinterSettings { PrinterName = rdlcPrinter.PrinterName }
                }
            };

            if (!printDoc.PrinterSettings.IsValid)
            {
                throw new InvalidOperationException("Invalid printer settings.");
            }

            int currentPage = 0;
            printDoc.PrintPage += (sender, e) =>
            {
                using Metafile pageImage = new Metafile(streams[currentPage]);
                e.Graphics.DrawImage(pageImage, e.PageBounds);
                currentPage++;
                e.HasMorePages = currentPage < streams.Count;
            };

            printDoc.EndPrint += (sender, e) =>
            {
                foreach (Stream stream in streams)
                {
                    stream.Dispose();
                }
            };

            printDoc.PrinterSettings.Copies = rdlcPrinter.Copies;
            printDoc.Print();

            return true;
        }
    }
}
