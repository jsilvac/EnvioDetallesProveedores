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
            _logger.Log($"Iniciando obtención de comprobantes...",LogLevel.Info);
            ManejoComprobantesDatos _comprobanteData = new ManejoComprobantesDatos(_icon);
            return _comprobanteData.ListarComprobantesDTO();
        }
        public string procesaComprobantes()
        {
            try
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
            catch (Exception ex)
            {
                _logger.Log($"Error en el proceso de generación de comprobantes: {ex.Message}", LogLevel.Error);
                return $"Error en el proceso de generación de comprobantes: {ex.Message}";
            }
        }

        private async Task<string> GenerarPdfConHeader(string rutaSalida, string nombreArchivo, List<ComprobantesDTO> comprobantes)
        {
            string exportFile = "";
            var headerDto = comprobantes.First();
            string tipoPago = "";

            try
            {
                _logger.Log($"Generando PDF: {nombreArchivo}...", LogLevel.Info);
                Directory.CreateDirectory(rutaSalida);
                exportFile = Path.Combine(rutaSalida, nombreArchivo);
                string fecha = DateTime.Now.ToString("dd-MM-yyyy");
                string rutaLogo = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "logo.png");
                var headers = new[] { "EGRESO", "PROVEEDOR", "GLOSA", "TD", "NUMERO", "MONTO" };
                float[] cellWidth = { 20f, 80f, 80f, 20f, 80f, 50f, 60f };

                var comprobantesFiltrados = comprobantes.Where(c => c.Numeros.Contains(c.Numero.ToString())).ToList();
                comprobantes = comprobantesFiltrados;

                using (var writer = new PdfWriter(exportFile))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {
                    var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

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

                    Table table = GeneraTablaComprobantes(comprobantes, headerDto, bold, headers, cellWidth);

                    document.Add(table);

                    var prov = ObtenerProveedor(headerDto.Proveedor);

                    document.Add(new Paragraph($"{prov.TipoPago}\n").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph($"DEPARTAMENTO PAGO DE PROVEEDORES\n").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));
                    document.Add(new Paragraph($" {headerDto.Empresa}").SetFontSize(10).SetTextAlignment(TextAlignment.CENTER));

                    document.Flush();
                    document.Close();
                }

                _logger.Log("PDF generado y guardado exitosamente", LogLevel.Info);
                System.Threading.Thread.Sleep(3000);
                _logger.Log($"Enviando correo para: {string.Join(", ", headerDto.Correos)}...", LogLevel.Info);

                int retorno = await EnviaMail(exportFile, headerDto);



                _logger.Log($"Enviado con éxito...", LogLevel.Success);

                return exportFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando PDF {nombreArchivo}: {ex.Message}");
                _logger.Log($"No se pudo enviar el correo, Error: {ex.Message} .", LogLevel.Error);
                return null;
            }
        }

        private async Task<int> EnviaMail(string exportFile, ComprobantesDTO headerDto)
        {
            EmailService _emailService = new EmailService(_config);
            var mensaje = armaBody(headerDto.Mensaje, headerDto);
            string tipo = (_config["EmailSettings:TipoEnvio"] ?? "").ToLower();

            try
            {
                if (tipo.Equals("smtp"))
                {
                    // Enviar por SMTP
                    _logger.Log("Iniciando envío por SMTP...", LogLevel.Info);
                    await _emailService.SendEmailAsync("Envío comprobantes contables", mensaje, headerDto.Correos, exportFile);
                    _logger.Log("✅ SMTP - Envío completado, ahora el log de éxito...", LogLevel.Info);
                    _logger.Log($"Enviado con éxito...", LogLevel.Success);
                    return 0;
                }
                else if (tipo.Equals("api"))
                {
                    // Enviar por api 
                    _logger.Log("Iniciando envío por API...", LogLevel.Info);
                    await _emailService.SendEmailViaMailchimpTransactionalAsync("Envío comprobantes contables", mensaje, headerDto.Correos, exportFile);
                    _logger.Log("✅ API - Envío completado, ahora el log de éxito...", LogLevel.Info);
                    _logger.Log($"Enviado con éxito...", LogLevel.Success);
                    return 0;
                }
                else
                {
                    throw new Exception("Tipo de envío no reconocido en configuración.");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                _logger.Log($"❌ ERROR durante el proceso de envío: {ex.Message}", LogLevel.Error);
                throw new Exception("Tipo de envío no reconocido en configuración:" + ex.Message);
                return 1;
            }
        }

        private static Table GeneraTablaComprobantes(List<ComprobantesDTO> comprobantes, ComprobantesDTO headerDto, PdfFont bold, string[] headers, float[] cellWidth)
        {
            Table table = new Table(UnitValue.CreatePercentArray(cellWidth))
                                    .UseAllAvailableWidth();

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
            
            return table;
        }

        public ProveedorDTO  ObtenerProveedor(string xRut)
        {
            ProveedorDatos _proveedorNegocio = new ProveedorDatos(_icon);
            return _proveedorNegocio.ObtenerProveedorPorRut(xRut);
        }

        public string armaBody(string mensaje, ComprobantesDTO header)
        {
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            string base64Logo = "";

            if (File.Exists(logoPath))
            {
                byte[] imageBytes = File.ReadAllBytes(logoPath);
                base64Logo = Convert.ToBase64String(imageBytes);
            }

            string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Resources", "email_body_template.html");

            if (!File.Exists(templatePath))
            {
                return "Error: Plantilla de correo no encontrada.";
            }

            string bodyTemplate = File.ReadAllText(templatePath);

            string finalBody = bodyTemplate
                .Replace("{{N_PROVEEDOR}}", header.N_Proveedor)
                .Replace("{{RUT_PROVEEDOR}}", header.Proveedor)
                .Replace("{{TOTAL_ABONADO}}", $"${header.Total:N0}")
                .Replace("{{NUM_DOCUS}}", header.Num_Docus.ToString())
                .Replace("{{EMPRESA}}", header.Empresa)
                .Replace("{{MENSAJE_ADICIONAL}}", mensaje);

            return finalBody;
        }

    }
}
