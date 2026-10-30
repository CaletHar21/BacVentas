using BacVentas.Web.Models;
using BacVentas.Web.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BacVentas.Web.Controllers;

// Reportes de ventas: solo administrador.
[Authorize(Roles = "ADMIN")]
public class ReportesController : Controller
{
    private readonly ApiClient _api;

    public ReportesController(ApiClient api) => _api = api;

    // GET /Reportes?desde=2026-10-01&hasta=2026-10-31
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
    {
        var model = new ReporteVentasVm { Desde = desde, Hasta = hasta };
        try
        {
            model.Ventas = await _api.ListarVentasAsync(desde, hasta);
        }
        catch (ApiException ex)
        {
            TempData["Error"] = ex.Message;
        }
        return View(model);
    }

    // Detalle de una venta (JSON para mostrarlo en un modal).
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        try
        {
            return Json(await _api.DetalleVentaAsync(id));
        }
        catch (ApiException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    // ---------- PDF con QuestPDF ----------
    [HttpGet]
    public async Task<IActionResult> Pdf(DateTime? desde, DateTime? hasta)
    {
        List<VentaResumenVm> ventas;
        try { ventas = await _api.ListarVentasAsync(desde, hasta); }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return RedirectToAction(nameof(Index), new { desde, hasta }); }

        var periodo = TextoPeriodo(desde, hasta);

        // QuestPDF arma el documento con una API "fluida": página -> encabezado / contenido / pie.
        var pdf = Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.Letter);
                pagina.Margin(30);
                pagina.DefaultTextStyle(t => t.FontSize(10));

                pagina.Header().Column(col =>
                {
                    col.Item().Text("BACVENTAS - REPORTE DE VENTAS").Bold().FontSize(16);
                    col.Item().Text($"Período: {periodo}");
                    col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm} por {User.FindFirst("NombreCompleto")?.Value}");
                });

                pagina.Content().PaddingVertical(10).Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.ConstantColumn(55);   // No. venta
                        c.ConstantColumn(100);  // Fecha
                        c.RelativeColumn();     // Vendedor
                        c.ConstantColumn(70);   // Subtotal
                        c.ConstantColumn(60);   // IVA
                        c.ConstantColumn(70);   // Total
                    });

                    tabla.Header(h =>
                    {
                        foreach (var titulo in new[] { "No.", "Fecha", "Vendedor", "Subtotal", "IVA", "Total" })
                            h.Cell().Background(Colors.Grey.Lighten2).Padding(4).Text(titulo).Bold();
                    });

                    foreach (var v in ventas)
                    {
                        tabla.Cell().BorderBottom(0.5f).Padding(4).Text(v.IdVenta.ToString("D5"));
                        tabla.Cell().BorderBottom(0.5f).Padding(4).Text(v.Fecha.ToString("dd/MM/yyyy HH:mm"));
                        tabla.Cell().BorderBottom(0.5f).Padding(4).Text(v.Vendedor);
                        tabla.Cell().BorderBottom(0.5f).Padding(4).AlignRight().Text($"${v.Subtotal:N2}");
                        tabla.Cell().BorderBottom(0.5f).Padding(4).AlignRight().Text($"${v.Iva:N2}");
                        tabla.Cell().BorderBottom(0.5f).Padding(4).AlignRight().Text($"${v.Total:N2}");
                    }

                    // Fila de totales
                    tabla.Cell().ColumnSpan(3).Padding(4).AlignRight().Text($"TOTALES ({ventas.Count} ventas)").Bold();
                    tabla.Cell().Padding(4).AlignRight().Text($"${ventas.Sum(v => v.Subtotal):N2}").Bold();
                    tabla.Cell().Padding(4).AlignRight().Text($"${ventas.Sum(v => v.Iva):N2}").Bold();
                    tabla.Cell().Padding(4).AlignRight().Text($"${ventas.Sum(v => v.Total):N2}").Bold();
                });

                pagina.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Página ");
                    t.CurrentPageNumber();
                    t.Span(" de ");
                    t.TotalPages();
                });
            });
        }).GeneratePdf();

        return File(pdf, "application/pdf", $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmm}.pdf");
    }

    // ---------- Excel con ClosedXML ----------
    [HttpGet]
    public async Task<IActionResult> Excel(DateTime? desde, DateTime? hasta)
    {
        List<VentaResumenVm> ventas;
        try { ventas = await _api.ListarVentasAsync(desde, hasta); }
        catch (ApiException ex) { TempData["Error"] = ex.Message; return RedirectToAction(nameof(Index), new { desde, hasta }); }

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Ventas");

        hoja.Cell(1, 1).Value = "BACVENTAS - REPORTE DE VENTAS";
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 14;
        hoja.Cell(2, 1).Value = $"Período: {TextoPeriodo(desde, hasta)}";

        // Encabezados en la fila 4
        var titulos = new[] { "No. Venta", "Fecha", "Vendedor", "Subtotal", "IVA", "Total" };
        for (var i = 0; i < titulos.Length; i++)
            hoja.Cell(4, i + 1).Value = titulos[i];

        var encabezado = hoja.Range(4, 1, 4, titulos.Length);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Datos desde la fila 5. Se guardan como número/fecha (no texto) para que Excel pueda sumarlos.
        var fila = 5;
        foreach (var v in ventas)
        {
            hoja.Cell(fila, 1).Value = v.IdVenta;
            hoja.Cell(fila, 2).Value = v.Fecha;
            hoja.Cell(fila, 3).Value = v.Vendedor;
            hoja.Cell(fila, 4).Value = v.Subtotal;
            hoja.Cell(fila, 5).Value = v.Iva;
            hoja.Cell(fila, 6).Value = v.Total;
            fila++;
        }

        // Fila de totales con fórmulas de Excel (si alguien edita una celda, el total se recalcula).
        hoja.Cell(fila, 3).Value = "TOTALES";
        if (ventas.Count > 0)
        {
            hoja.Cell(fila, 4).FormulaA1 = $"SUM(D5:D{fila - 1})";
            hoja.Cell(fila, 5).FormulaA1 = $"SUM(E5:E{fila - 1})";
            hoja.Cell(fila, 6).FormulaA1 = $"SUM(F5:F{fila - 1})";
        }
        hoja.Range(fila, 1, fila, 6).Style.Font.Bold = true;

        hoja.Range(5, 2, fila, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        hoja.Range(5, 4, fila, 6).Style.NumberFormat.Format = "$#,##0.00";
        hoja.Range(4, 1, fila, 6).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        hoja.Range(4, 1, fila, 6).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ReporteVentas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }

    private static string TextoPeriodo(DateTime? desde, DateTime? hasta) => (desde, hasta) switch
    {
        (null, null) => "Todas las ventas",
        (not null, null) => $"Desde {desde:dd/MM/yyyy}",
        (null, not null) => $"Hasta {hasta:dd/MM/yyyy}",
        _ => $"Del {desde:dd/MM/yyyy} al {hasta:dd/MM/yyyy}"
    };
}
