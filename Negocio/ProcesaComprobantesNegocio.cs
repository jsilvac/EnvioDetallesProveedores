using Dto;
using Repository;
using iText.Kernel.Geom;
using Path = System.IO.Path;
using Helpers;
using iText.Kernel.Pdf.Event;
using iText.IO.Image;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using static System.Net.Mime.MediaTypeNames;
using Image = iText.Layout.Element.Image;
using Newtonsoft.Json.Linq;
using Datos;



namespace Negocio
{
    public class ProcesaComprobantesNegocio
    {
        private readonly ManejoComprobantesDatos _comprobanteData;
        private readonly ProveedorNegocio _proveedorNegocio;
        private IConexion _icon;
        private PDF _pdf;
        public ProcesaComprobantesNegocio(IConexion icon)
        {
            _icon = icon;
            //_pdf = new PDF();
        }

        public List<ComprobantesDTO> ObtenerComprobantes()
        {
            ManejoComprobantesDatos _comprobanteData = new ManejoComprobantesDatos(_icon);
            return _comprobanteData.ListarComprobantesDTO();
        }


        public string procesaComprobantes()
        {
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

            return $"Proceso finalizado. Se generaron {comprobantesAgrupados.Count} archivos PDF.";
        }

        private string GenerarPdfConHeader(string rutaSalida, string nombreArchivo, List<ComprobantesDTO> comprobantes)
        {
            try
            {
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
                    table.AddCell(new Cell().Add(new Paragraph("$" + headerDto.Total.ToString()).SetFontSize(9).SetTextAlignment(TextAlignment.RIGHT)));

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

                }

                return exportFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando PDF {nombreArchivo}: {ex.Message}");
                return null;
            }
        }

        public ProveedorDTO  ObtenerProveedor(string xRut)
        {
            ProveedorDatos _proveedorNegocio = new ProveedorDatos(_icon);
            return _proveedorNegocio.ObtenerProveedorPorRut(xRut);
        }

    }
}
