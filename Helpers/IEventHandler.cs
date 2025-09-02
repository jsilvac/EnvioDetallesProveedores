using iText.Commons.Actions;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Geom;
using Dto;


public class HeaderEventHandler : IEventHandler
{
    private readonly ComprobantesDTO _headerDto; 

    public HeaderEventHandler(ComprobantesDTO headerDto)
    {
        _headerDto = headerDto;
    }

    public void HandleEvent(IEvent @event)
    {
        PdfDocumentEvent docEvent = (PdfDocumentEvent)@event;
        PdfDocument pdfDoc = docEvent.GetDocument();
        PdfPage page = docEvent.GetPage();

        Rectangle pageSize = page.GetPageSize();
        PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);

        using (var g = new Canvas(canvas, pageSize))
        {
            g.ShowTextAligned(
                new Paragraph($"Comprobante: {_headerDto.Numero} - Fecha: {_headerDto.Fecha}"),
                pageSize.GetWidth() / 2,
                pageSize.GetTop() - 20,
                TextAlignment.CENTER
            );
        }
    }

    public void OnEvent(IEvent @event)
    {
        throw new NotImplementedException();
    }
}
