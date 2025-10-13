using Dto;
using Repository;
using Path = System.IO.Path;
using Helpers;
using iText.IO.Image;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using Image = iText.Layout.Element.Image;
using Datos;
using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;


namespace Negocio
{
    public class ProcesaComprobantesNegocio
    {
 
        private readonly IConfigurationRoot _config;
        private IConexion _icon;
        private ILogger _logger;

 
        public ProcesaComprobantesNegocio(IConexion icon)
        {
            _icon = icon;
        }

        public ProcesaComprobantesNegocio(IConexion icon,  IConfigurationRoot config, ILogger logger)
        {
            _icon = icon;
            _config = config;
            _logger = logger;
        }
        public List<ComprobantesDTO> ObtenerComprobantes()
        {
            _logger.Log($"Iniciando obtención de comprobantes...",LogLevel.Warning);
            ManejoComprobantesDatos _comprobanteData = new ManejoComprobantesDatos(_icon);
            return _comprobanteData.ListarComprobantesDTO();
        }
        public string procesaComprobantes()
        {
            _logger.Log($"Iniciando proceso de generación de comprobantes...", LogLevel.Info);
            var listaC = ObtenerComprobantes();

            // Agrupar por folio
            var comprobantesAgrupados = listaC
                .GroupBy(c => c.Folio)
                .ToList();

            foreach (var grupo in comprobantesAgrupados)
            {
                var pdfGenerado = GenerarPdfConHeader(
                    @"C:\Exportados",
                    $"Comprobante_{grupo.Key}.pdf",
                    grupo.ToList()
                );

                Console.WriteLine($"PDF generado para folio {grupo.Key}: {pdfGenerado}");
            }

            _logger.Log($"Proceso finalizado. Se generaron {comprobantesAgrupados.Count} archivos PDF.", LogLevel.Success);
            return $"Proceso finalizado. Se generaron {comprobantesAgrupados.Count} archivos PDF.";
        }

        private async Task<string> GenerarPdfConHeader(string rutaSalida, string nombreArchivo, List<ComprobantesDTO> comprobantes)
         {
            try
            {
                _logger.Log($"Generando PDF: {nombreArchivo}...", LogLevel.Info);
                Directory.CreateDirectory(rutaSalida);
                string exportFile = Path.Combine(rutaSalida, nombreArchivo);

                using (var writer = new PdfWriter(exportFile))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {

                    var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                    var comprobantesFiltrados = comprobantes.Where(c => c.Numeros.Contains(c.Numero.ToString())).ToList();

                    comprobantes = comprobantesFiltrados;

                    var headerDto = comprobantes.First();

                    string fecha = DateTime.Now.ToString("dd-MM-yyyy");
                    // --- AGREGAR LOGO  ---
                    string rutaLogo = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "logo.png");
                   
                    PdfPage page = pdf.AddNewPage();
                    var pageSize = page.GetPageSize();

                    PdfCanvas canvas = new PdfCanvas(page);
                    Canvas modelCanvas = new Canvas(canvas, pageSize);

                    // Logo
                    if (File.Exists(rutaLogo))
                    {
                        var img = new Image(ImageDataFactory.Create(rutaLogo))
                            .ScaleToFit(80, 80)
                            .SetFixedPosition(pageSize.GetRight() - 113, pageSize.GetTop() - 50);

                        modelCanvas.Add(img);
                    }

                    // Texto
                    modelCanvas.Add(new Paragraph("Comprobante de Pago")
                        .SetFont(bold).SetFontSize(12)
                        .SetFixedPosition(pageSize.GetLeft() + 35, pageSize.GetTop() - 40, 500));

                    modelCanvas.Add(new Paragraph("Fecha de emisión: " + fecha)
                        .SetFont(bold).SetFontSize(10)
                        .SetFixedPosition(pageSize.GetLeft() + 35, pageSize.GetTop() - 60, 500));

                    modelCanvas.Close();

                    document.Add(new Paragraph("\n"));

                    float[] cellWidth = { 20f, 80f, 80f, 20f, 80f, 50f, 60f }; 
                    Table table = new Table(UnitValue.CreatePercentArray(cellWidth))
                        .UseAllAvailableWidth();

                    var headers = new[] { "EGRESO", "PROVEEDOR", "GLOSA", "TD", "NUMERO", "MONTO" };
                    foreach (var h in headers)
                    {
                        if (h == "GLOSA")
                            table.AddHeaderCell(new Cell(1, 2).Add(new Paragraph(h).SetFont(bold).SetFontSize(10)));
                        else
                            table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(bold).SetFontSize(10)));
                    }

                    // Filas detalle
                    foreach (var c in comprobantes)
                    {

                        table.AddCell(new Cell().Add(new Paragraph(c.Egreso).SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT));
                        table.AddCell(new Cell().Add(new Paragraph(c.Proveedor).SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT));
                        table.AddCell(new Cell(1, 2).Add(new Paragraph(c.Glosa).SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT));
                        table.AddCell(new Cell().Add(new Paragraph(c.Td).SetFontSize(9)));
                        table.AddCell(new Cell().Add(new Paragraph(c.Numero.ToString()).SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT));
                        table.AddCell(new Cell().Add(new Paragraph($"${c.Monto:N0}").SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT));
                    }
                    table.AddCell(new Cell().Add(new Paragraph("PROVEEDOR").SetFont(bold).SetFontSize(10)).SetTextAlignment(TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph("NOMBRE").SetFont(bold).SetFontSize(10)).SetTextAlignment(TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph("BANCO").SetFont(bold).SetFontSize(10).SetTextAlignment(TextAlignment.LEFT)));
                    table.AddCell(new Cell().Add(new Paragraph("CUENTA").SetFont(bold).SetFontSize(10).SetTextAlignment(TextAlignment.LEFT)));
                    table.AddCell(new Cell().Add(new Paragraph("N° DCTS.").SetFont(bold).SetFontSize(10).SetTextAlignment(TextAlignment.LEFT)));
                    table.AddCell(new Cell().Add(new Paragraph("CORREO(S)").SetFont(bold).SetFontSize(10).SetTextAlignment(TextAlignment.LEFT)));
                    table.AddCell(new Cell().Add(new Paragraph("TOTAL").SetFont(bold).SetFontSize(10).SetTextAlignment(TextAlignment.CENTER)));
                    table.AddCell(new Cell().Add(new Paragraph(headerDto.Proveedor).SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT));
                    table.AddCell(new Cell().Add(new Paragraph(headerDto.N_Proveedor).SetFontSize(9)).SetTextAlignment(TextAlignment.LEFT));
                    table.AddCell(new Cell().Add(new Paragraph(headerDto.Banco).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().Add(new Paragraph(headerDto.CtaCte_Proveedor).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().Add(new Paragraph(headerDto.Num_Docus).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)));
                    string listaCorreos = string.Join("\n", headerDto.Correos);
                    table.AddCell(new Cell().Add(new Paragraph(listaCorreos).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)));
                    table.AddCell(new Cell().Add(new Paragraph($" ${headerDto.Total:N0}").SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)));

                    document.Add(table);
                    var tipoPago="";
                    var prov = ObtenerProveedor(headerDto.Proveedor);
                    
                    if (prov.ModoPago != null)
                    {
                        tipoPago= prov.ModoPago;
                        
                        switch (prov.ModoPago)
                        {
                            case "0":
                                tipoPago = "CANCELACION DE FACTURAS VIA CHEQUE";
                                break;
                            case "1":
                                tipoPago = "CANCELACION DE FACTURAS VIA VALE VISTA";
                                break;
                            case "3":
                                tipoPago = "CANCELACION DE FACTURAS VIA TRANSFERENCIA BANCARIA";
                                break;
                            default:
                                tipoPago = "CANCELACION DE FACTURAS";
                                break;
                        }
                    }
                    else
                    {
                        tipoPago = "CANCELACION DE FACTURAS";
                    }

                    document.Add(new Paragraph($"{tipoPago}\n").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph($"DEPARTAMENTO PAGO DE PROVEEDORES\n").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph($" {headerDto.Empresa}").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));

                    document.Flush();
                    document.Close();

                    /// aki region envio docuemnto por corre ////
                    /// 
                    System.Threading.Thread.Sleep(3000);

                    _logger.Log($"Enviando correo para: {string.Join(", ", headerDto.Correos)}...", LogLevel.Info);
                    EmailService _emailService = new EmailService(_config);
                    var mensaje = armaBody(headerDto.Mensaje, headerDto);
                    //await _emailService.SendEmailViaMailchimpTransactionalAsync("Envío comprobantes contables", headerDto.Mensaje, headerDto.Correos, exportFile);
                    await _emailService.SendEmailAsync("Envío comprobantes contables",mensaje,headerDto.Correos, exportFile);
                    _logger.Log($"Enviado con éxito...", LogLevel.Success);

                }

                return exportFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando PDF {nombreArchivo}: {ex.Message}");
                _logger.Log($"No se pudo enviar el correo, Error: {ex.Message} .", LogLevel.Error);
                return null;
            }
        }

        public ProveedorDTO  ObtenerProveedor(string xRut)
        {
            ProveedorDatos _proveedorNegocio = new ProveedorDatos(_icon);
            return _proveedorNegocio.ObtenerProveedorPorRut(xRut);
        }

        public string armaBody(string mensaje, ComprobantesDTO header   )
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            string base64Logo = "";
            if (File.Exists(logoPath))
            {
                byte[] imageBytes = File.ReadAllBytes(logoPath);
                base64Logo = Convert.ToBase64String(imageBytes);
            }

            return $@"
                <html>
                  <body style='font-family: Arial, Helvetica, sans-serif; background-color: #f9fafb; padding: 30px;'>
                    <table style='max-width: 700px; margin: auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 3px 10px rgba(0,0,0,0.1);'>
                      <tr>
                        <td style='background-color: #85CFF2; padding: 20px; border-top-left-radius: 10px; border-top-right-radius: 10px; text-align: center;'>
                         <img src='cid:eltitLogo' alt='Logo Eltit' width='120' style='margin-bottom:15px;'/>
                          <h2 style='color: #ffffff; margin: 0;'>Comprobante de Pago</h2>
                        </td>
                      </tr>

                      <tr>
                        <td style='padding: 30px; color: #333;'>
                          <p style='font-size: 15px; margin-bottom: 10px;'>
                            <strong>Señores:</strong><br>
                            <span style='font-size: 16px; color: #E31E24;'>{header.N_Proveedor}</span><br>
                            <em>RUT: {header.Proveedor}</em>
                          </p>

                          <p style='font-size: 14px; line-height: 1.6;'>
                            Por medio de la presente informamos a usted que ha sido abonado en su cuenta el siguiente monto correspondiente a comprobantes procesados por nuestro sistema de pagos.
                          </p>

                          <div style='background-color: #f6fff0; border-left: 5px solid #8BC53F; padding: 15px; margin: 20px 0;'>
                            <p style='font-size: 16px; color: #333; margin: 0;'>
                              <strong>Monto abonado:</strong>
                              <span style='color: #008000;'>${header.Total:N0}</span>
                            </p>
                            <p style='font-size: 14px; margin: 5px 0 0 0;'>
                              <strong>Cuenta:</strong> {header.CtaCte_Proveedor}
                            </p>
                            <p style='font-size: 14px; margin: 5px 0 0 0;'>
                              <strong>N° Documentos:</strong> {header.Num_Docus}
                            </p>
                          </div>

                          <p style='font-size: 14px; margin-top: 25px;'>
                            En caso de dudas o consultas, puede contactarse con el departamernto de proveedores!
                          </p>
                    

                          <p style='font-size: 14px; margin-top: 25px; line-height: 1.4;'>
                            Atentamente,<br>
                            <strong>Departamento de Pago de Proveedores</strong><br>
                            {header.Empresa}
                          </p>
                        </td>
                      </tr>

                      <tr>
                        <td style='background-color: #f4f6f8; text-align: center; padding: 15px; border-bottom-left-radius: 10px; border-bottom-right-radius: 10px;'>
                          <p style='font-size: 12px; color: #777; margin: 0;'>
                            Este correo fue generado automáticamente. Por favor, no responder directamente a este mensaje.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </body>
                </html>";

        }
    }
}
