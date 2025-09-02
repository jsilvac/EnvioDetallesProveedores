using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;


namespace Helpers
{
    public class PDF
    {
        public byte[] CrearPdfEnMemoria(string contenido)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Crear escritor PDF
                using (var writer = new PdfWriter(memoryStream))
                {
                    // Crear documento PDF
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);

                        // Agregar contenido
                        document.Add(new Paragraph(contenido));
                        document.Add(new Paragraph("Este PDF fue generado..."));

                        document.Close();
                    }
                }
                // Retornar PDF como arreglo de bytes
                return memoryStream.ToArray();
            }
        }
        public void GuardarPdf(string ruta, string contenido)
        {
            var bytes = CrearPdfEnMemoria(contenido);
            File.WriteAllBytes(ruta, bytes);
        }
    }
}
