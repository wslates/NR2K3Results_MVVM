using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using NR2K3Results_MVVM.Model;
using System.Linq;

namespace NR2K3Results_MVVM.PDFGeneration
{
    class RacePDFGenerator
    {
        public static void OutputPDF(List<Driver> drivers, Series series, string raceName, Race track, string saveFile)
        {
            Document document = new Document(PageSize.A4, 15, 25, 15, 30);
            FileStream fs = null;

            try
            {
                fs = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
                
                //build title
                StringBuilder title = new StringBuilder();
                title.AppendLine(series.SeriesName);
                if (String.IsNullOrEmpty(raceName) || String.IsNullOrWhiteSpace(raceName))
                {
                    title.AppendLine("Unofficial Race Results");
                } else
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
                    SpacingAfter = 7
                };
                document.Add(session);

                if (!String.IsNullOrEmpty(series.SeriesLogo) && !String.IsNullOrWhiteSpace(series.SeriesLogo))
                {
                    Image img = Image.GetInstance(new Uri(series.SeriesLogo));
                    img.ScaleToFit(125f, 125f);
                    img.SetAbsolutePosition(document.PageSize.Width - img.ScaledWidth - 20f, document.PageSize.Height - img.ScaledHeight - 20f);
                    img.Alignment = Image.TEXTWRAP;
                    document.Add(img);
                }




                document.Add(GenerateTopRow(ref SessionData.RACECOLUMNWIDTHS, ref SessionData.RACECOLUMNS));
                document.Add(GenerateDriverRows(drivers, ref SessionData.RACECOLUMNWIDTHS));
                document.Add(GenerateRaceStats(drivers, track));
                document.Add(GenerateCautionsLeadersAndPositionalChanges(drivers, track));

                document.Close();
            }
            catch (IOException e)
            {
                document.Close();
                return;
            }
            
        }
        private static PdfPTable GenerateCautionsLeadersAndPositionalChanges(List<Driver> drivers, Race track)
        {
            PdfPTable table = new PdfPTable(new float[] { 50f, 50f })
            {
                WidthPercentage = 100f
            };

            Paragraph full = new Paragraph("Caution Flags:    ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT
            };
            Chunk secondElement = new Chunk(track.cautions + " for " + track.cautionLaps + " laps", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });

            var topDrivers = drivers.OrderByDescending(d => (d.result.start - d.result.finish));
            full = new Paragraph("Biggest gain:    ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT
            };
            secondElement = new Chunk(topDrivers.First().GetName() + "; +" + (topDrivers.First().result.start - topDrivers.First().result.finish) + " positions", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });


            full = new Paragraph("Lead Changes:    ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT
            };
            secondElement = new Chunk(track.leadChanges + " among " + track.leaders + " drivers", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });

            int loss = (topDrivers.Last().result.finish - topDrivers.Last().result.start);
            loss = (loss < 0) ? loss * -1 : loss;
            full = new Paragraph("Biggest loss:    ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT
            };
            secondElement = new Chunk(topDrivers.Last().GetName() + "; -" + loss + " positions", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });

            return table;
        }
    
        private static PdfPTable GenerateRaceStats(List<Driver> drivers, Race track)
        {

            PdfPTable table = new PdfPTable(new float[] { 50f, 50f, 50f })
            {
                WidthPercentage = 100f, 
                SpacingAfter = 10f
            };

            TimeSpan timeSpan = TimeSpan.FromHours((double)((track.length * track.laps) / track.speed));
            String time = String.Format("{0} Hrs, {1} Mins, {2} Secs.", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            Paragraph full = new Paragraph("Time of Race:   ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT,
                
            };
            Chunk secondElement = new Chunk(time, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });
            

            full = new Paragraph("Average Speed:    ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT
            };
            secondElement = new Chunk(track.speed.ToString() + " MPH", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });


            full = new Paragraph("Margin of Victory:    ", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD))
            {
                Alignment = Element.ALIGN_LEFT
            };
            secondElement = new Chunk(drivers[1].GetOffLeader().Replace("-", String.Empty) + " Seconds", FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL));
            full.Add(secondElement);
            table.AddCell(new PdfPCell(full) { Border = Rectangle.NO_BORDER });

            

            return table;

           
            

         
            


            
            
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
                string timeOff = (driver.result.lapsDown == 0) ? driver.GetOffLeader() : driver.result.lapsDown.ToString() + "L";
                table.AddCell(GenerateDriverCell(timeOff, driver.GetFinish(), 7, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.result.status, driver.GetFinish(), 8, Element.ALIGN_RIGHT, ref widths));
                table.AddCell(GenerateDriverCell(driver.result.lapsLed.ToString(), driver.GetFinish(), 9, Element.ALIGN_RIGHT, ref widths));
            }

            return table;
        }

        private static PdfPCell GenerateDriverCell(string data, int verPos, int horizPos, int justify, ref float[] widths)
        {
            float fontSize = 8.5f;

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
