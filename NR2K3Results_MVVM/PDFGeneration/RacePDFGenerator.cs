using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using NR2K3Results_MVVM.Model;

namespace NR2K3Results_MVVM.PDFGeneration
{
    class RacePDFGenerator
    {
        public static void OutputPDF(List<Driver> drivers, Series series, string raceName, Track track)
        {
            Document document = new Document(PageSize.A4, 15, 25, 15, 30);
            FileStream fs = null;

            try
            {
                fs = new FileStream("test.pdf", FileMode.Create, FileAccess.Write, FileShare.None);
                
                //build title
                StringBuilder title = new StringBuilder();
                title.AppendLine(series.SeriesName);
                title.AppendLine("Unofficial Race Results for the " + raceName);
                title.AppendLine(track.name + " - " + track.city + ", " + track.state + " - " + track.description);
                title.AppendLine("Total Race Length - " + track.laps + " Laps - " + Decimal.Round(track.laps*track.length) + " Miles");
                title.AppendLine();
                

                PdfWriter write = PdfWriter.GetInstance(document, fs);
                document.Open();

                //title
                Paragraph session = new Paragraph(title.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 11, Font.BOLDITALIC))
                {
                    Alignment = Element.ALIGN_LEFT,
                    Leading = 17,
                    SpacingAfter = 25
                };
                document.Add(session);



                document.Add(GenerateTopRow(ref SessionData.RACECOLUMNWIDTHS, ref SessionData.RACECOLUMNS));

                document.Add(GenerateDriverRows(drivers, ref SessionData.RACECOLUMNWIDTHS));
                document.Close();
            }
            catch (IOException e)
            {
                return;
            }
            document.Close();
        }

        private static PdfPTable GenerateTopRow(ref float[] widths, ref List<Tuple<string, int>> tableData)
        {

            PdfPTable table = new PdfPTable(tableData.Count)
            {
                //set table to be total width of document excluding margins
                WidthPercentage = 100f,
            };
            table.SetWidths(widths);


            foreach (Tuple<string, int> column in tableData)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.Item1, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD)))
                {
                    //sets only top border visible
                    Border = Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER,
                    BorderWidthTop = 3f,
                    Colspan = 1,
                    //negate padding
                    PaddingTop = 4,
                    PaddingBottom = 1,
                    HorizontalAlignment = column.Item2
                };
                table.AddCell(cell);
            }

            return table;

        }

        private static PdfPTable GenerateDriverRows(List<Driver> drivers, ref float[] widths)
        {
            PdfPTable table = new PdfPTable(widths.Length)
            {
                //set table to be total width of document excluding margins
                WidthPercentage = 100f,
            };
            table.SetWidths(widths);

            foreach (Driver driver in drivers)
            {
                table.AddCell(GenerateDriverCell(driver.GetFinish().ToString(), driver.GetFinish(), 0, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.result.start.ToString(), driver.GetFinish(), 1, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.number.ToString(), driver.GetFinish(), 2, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.firstName + " " + driver.lastName, driver.GetFinish(), 3, Element.ALIGN_LEFT, ref widths));
                table.AddCell(GenerateDriverCell(driver.sponsor, driver.GetFinish(), 4, Element.ALIGN_LEFT, ref widths));
                table.AddCell(GenerateDriverCell(driver.team, driver.GetFinish(), 5, Element.ALIGN_LEFT, ref widths));
                table.AddCell(GenerateDriverCell(driver.result.laps.ToString(), driver.GetFinish(), 6, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.result.status, driver.GetFinish(), 7, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.result.lapsLed.ToString(), driver.GetFinish(), 8, Element.ALIGN_RIGHT, ref widths));
            }

            return table;
        }

        private static PdfPCell GenerateDriverCell(string data, int verPos, int horizPos, int justify, ref float[] widths)
        {
            float fontSize = 9f;

            if (data != null)
            {
                while ((FontFactory.GetFont(FontFactory.HELVETICA, fontSize).BaseFont.GetWidthPoint(data, fontSize)) > widths[horizPos] * 4.25)
                {
                    fontSize -= .1f;
                }
            }

            return new PdfPCell(new Phrase(data, FontFactory.GetFont(FontFactory.HELVETICA, fontSize)))
            {
                Border = Rectangle.BOTTOM_BORDER,
                Colspan = 1,
                PaddingTop = 2,
                PaddingBottom = 2,
                HorizontalAlignment = justify,

            };
        }


    }
}
