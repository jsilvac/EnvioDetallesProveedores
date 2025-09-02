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



namespace Negocio
{
    public class ProcesaComprobantesNegocio
    {
        private readonly ManejoComprobantesDatos _comprobanteData;
        private IConexion _icon;
        private PDF _pdf;
        public ProcesaComprobantesNegocio(IConexion icon)
        {   
            _icon = icon;
            _pdf = new PDF();
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

                   
                    var comprobantesFiltrados = comprobantes.Where(c => c.Numeros.Contains(c.Numero.ToString())).ToList();

                    comprobantes = comprobantesFiltrados;
                 
                    // Obtener el primer comprobante para el header (todos comparten mismo folio)
                    var headerDto = comprobantes.First();


                    // --- AGREGAR LOGO PRIMERO (como header) ---
                    string rutaLogo = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "logo.png");
                    if (File.Exists(rutaLogo))
                    {
                        // Agregar logo al inicio del documento
                        AddLogoAsHeader(document, rutaLogo);
                       
                    }

                    // --- Generar contenido del PDF ---
                    var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                    // Cabecera de la tabla de detalle
                    var table = new Table(7).UseAllAvailableWidth();
                    var headers = new[] { "EGRESO", "PROVEEDOR", "GLOSA", "TD", "NUMERO", "MONTO" };
                    foreach (var h in headers)
                        if(h == "GLOSA")
                        {
                            table.AddHeaderCell(new Cell(1,2).Add(new Paragraph(h).SetFont(bold).SetFontSize(9)));
                        }
                        else 
                        { 
                            table.AddHeaderCell(new Cell().Add(new Paragraph(h).SetFont(bold).SetFontSize(9)));
                        }

                    // Filas detalle
                    foreach (var c in comprobantes)
                    {

                        table.AddCell(new Cell().Add(new Paragraph(c.Egreso).SetFontSize(9)));
                        table.AddCell(new Cell().Add(new Paragraph(c.Proveedor).SetFontSize(9)));
                        table.AddCell(new Cell(1, 2).Add(new Paragraph(c.Glosa).SetFontSize(9)));
                        table.AddCell(new Cell().Add(new Paragraph(c.Td).SetFontSize(9)));
                        table.AddCell(new Cell().Add(new Paragraph(c.Numero.ToString()).SetFontSize(9)));
                        table.AddCell(new Cell().Add(new Paragraph($"${c.Monto:N0}").SetFontSize(9)).SetTextAlignment(TextAlignment.RIGHT));
                    }

                    document.Add(table);

                    // Calcular total sumando todos los montos del grupo
                    decimal totalGrupo = (decimal)comprobantes.Sum(c => c.Monto);
                    document.Add(new Paragraph($"Total: ${totalGrupo:N0}").SetTextAlignment(TextAlignment.RIGHT).SetFontSize(12));


                    // Añadir resumen/header de cierre
                    document.Add(new Paragraph("\n"));
                    document.Add(new Paragraph($" {headerDto.Rut_Proveedor} - {headerDto.N_Proveedor}").SetFontSize(10)).SetTextAlignment(TextAlignment.CENTER);
                    document.Add(new Paragraph($" {headerDto.Empresa}").SetFontSize(10)).SetTextAlignment(TextAlignment.CENTER);

                   
                    // --- IMPORTANTE: Flush del document ---
                    document.Flush();

                    //// 2) STAMP: agregar header (logo) sobre cada página
                    //string rutaLogo = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "logo.png");
                    //if (File.Exists(rutaLogo))
                    //{

                    //    AddHeaderToAllPages(pdf, rutaLogo, 100f, 50f, 36f);
                    //}

                    // 3) Cerrar document
                    document.Close();
                }

                return exportFile;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generando PDF {nombreArchivo}: {ex.Message}");
                return null;
            }
        }

        private void AddLogoAsHeader(Document document, string rutaLogo)
        {
            try
            {
                ImageData imageData = ImageDataFactory.Create(rutaLogo);
                Image logo = new Image(imageData);

                // Ajustar tamaño del logo (opcional)
                logo.SetWidth(100f);
                logo.SetHeight(40f);
                logo.SetAutoScaleHeight(false);

                // Centrar el logo
                logo.SetHorizontalAlignment(HorizontalAlignment.RIGHT);

                // Agregar logo al documento
                document.Add(logo);

                // Agregar espacio después del logo
                document.Add(new Paragraph("\n"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error agregando logo: {ex.Message}");
            }
        }

        // Método para agregar header en todas las páginas (debe estar definido)
        private void AddHeaderToAllPages(PdfDocument pdf, string imagePath, float maxWidth, float maxHeight, float marginLeft)
        {
            // Tu implementación existente para agregar el logo
            ImageData imageData = ImageDataFactory.Create(imagePath);
            Image image = new Image(imageData);
            image.ScaleToFit(maxWidth, maxHeight);

            for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
            {
                PdfPage page = pdf.GetPage(i);
                PdfCanvas pdfCanvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdf);
                Canvas canvas = new Canvas(pdfCanvas, page.GetPageSize());

                image.SetFixedPosition(marginLeft, page.GetPageSize().GetHeight() - maxHeight - 20);
                canvas.Add(image);
                canvas.Close();
            }
        }

    }
}
