using System;
using System.Collections.Generic;

using System.Text;

using iTextSharp.text;

using iTextSharp.text.pdf;
using System.IO;
using NR2K3Results_MVVM.Model;

namespace NR2K3Results_MVVM.PDFGeneration
{
    class PracticePDFGenerators
    {
        /// <summary>
        /// Outputs a practice session's PDF.
        /// </summary>
        /// <param name="drivers">List of drivers that participated in the session.</param>
        /// <param name="series">The series information.</param>
        /// <param name="selectedSession">String regarding the selected session.</param>
        /// <param name="raceName">Name of the race. Passing a null value indicates the race has no name.</param>
        /// <param name="race">Current race session information.</param>
        /// <param name="saveFile">Path to save the PDF to.</param>
        public static void OutputPDF(List<Driver> drivers, Series series, string selectedSession, string raceName, Race race, string saveFile)
        {
            //setup output information.
            Document document = new Document(PageSize.A4, 15, 25, 15, 30);
            FileStream fs = new FileStream(saveFile, FileMode.Create, FileAccess.Write, FileShare.None);
            PdfWriter write = PdfWriter.GetInstance(document, fs);
            document.Open();

            //check if the user selected a series logo, and if so, open it and insert it into the PDF.
            if (!String.IsNullOrEmpty(series.SeriesLogo) && !String.IsNullOrWhiteSpace(series.SeriesLogo))
            {
                Image img = Image.GetInstance(new Uri(series.SeriesLogo));
                img.ScaleToFit(125f, 125f);
                img.SetAbsolutePosition(20f, document.PageSize.Height - img.ScaledHeight - 20f);
                img.Alignment = Image.TEXTWRAP;
                document.Add(img);
            }

            //check if the user selected a sanctioning body logo, and if so, open it and insert it into the PDF.
            if (!String.IsNullOrEmpty(series.SancLogo) && !String.IsNullOrWhiteSpace(series.SancLogo))
            {
                Image img = Image.GetInstance(new Uri(series.SancLogo));
                img.ScaleToFit(125f, 125f);
                img.SetAbsolutePosition(document.PageSize.Width - img.ScaledWidth - 20f, document.PageSize.Height - img.ScaledHeight - 20f);
                img.Alignment = Image.TEXTWRAP;
                document.Add(img);
            }

            //build title
            StringBuilder title = new StringBuilder();
            title.Append(series.SeriesShort);
            title.AppendLine(" " + selectedSession);
            title.AppendLine(race.name);
            if (!String.IsNullOrEmpty(raceName) && !String.IsNullOrWhiteSpace(raceName)) title.AppendLine(raceName);

            //title
            Paragraph session = new Paragraph(title.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 14, Font.BOLD))
            {
                Alignment = Element.ALIGN_CENTER,
                Leading = 17,
                SpacingAfter = (String.IsNullOrEmpty(raceName) || String.IsNullOrWhiteSpace(raceName)) ? 47 : 30
            };
            document.Add(session);

            //add "created by" sentence above the results table
            title = new StringBuilder();
            title.Append("Provided by ");
            title.Append(series.SeriesName);
            title.Append(" Statistics");

            Paragraph providedBy = new Paragraph(title.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.ITALIC))
            {
                SpacingAfter = 5f,
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(providedBy);

            //add the top row of information (column names, essentially) to the PDF.
            document.Add(GenerateTopRow(ref TableData.PRACTICECOLUMNWIDTHS, ref TableData.PRACTICECOLUMNS));

            //add the results to the table.
            document.Add(GenerateDriverRows(drivers, ref TableData.PRACTICECOLUMNWIDTHS));

            //close doc
            document.Close();
        }

        /// <summary>
        /// Generates the top row of the table, which is the column names.
        /// </summary>
        /// <param name="widths">Widths of the columns to be created.</param>
        /// <param name="tableData">Data about the column, the string is the column contents and the integer is the column justification.</param>
        /// <returns></returns>
        private static PdfPTable GenerateTopRow(ref float[] widths, ref List<Tuple<string, int>> tableData)
        {
            
            PdfPTable table = new PdfPTable(tableData.Count)
            {
                //set table to be total width of document excluding margins
                WidthPercentage = 100f,
            };
            table.SetWidths(widths);

          
            foreach(Tuple<string, int> column in tableData)
            {  
                PdfPCell cell = new PdfPCell(new Phrase(column.Item1, FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.BOLD)))
                {
                    //sets only top and bottom border visible
                    Border = Rectangle.BOTTOM_BORDER | Rectangle.TOP_BORDER,
                    Colspan = 1,
                    //negate padding
                    PaddingTop = -2,
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
                table.AddCell(GenerateDriverCell(driver.GetFinish().ToString(), driver.GetFinish(), 0, Element.ALIGN_RIGHT, widths[0]));
                table.AddCell(GenerateDriverCell(driver.number, driver.GetFinish(), 1, Element.ALIGN_RIGHT, widths[1]));
                table.AddCell(GenerateDriverCell(driver.firstName + " " + driver.lastName, driver.GetFinish(), 2, Element.ALIGN_LEFT, widths[2]));
                table.AddCell(GenerateDriverCell(driver.sponsor, driver.GetFinish(), 3, Element.ALIGN_LEFT, widths[3]));
                table.AddCell(GenerateDriverCell(driver.team, driver.GetFinish(), 4, Element.ALIGN_LEFT, widths[4]));
                table.AddCell(GenerateDriverCell(driver.GetTime(), driver.GetFinish(), 5, Element.ALIGN_RIGHT, widths[5]));
                table.AddCell(GenerateDriverCell(driver.GetSpeed(), driver.GetFinish(), 6, Element.ALIGN_RIGHT, widths[6]));
                table.AddCell(GenerateDriverCell(driver.GetOffLeader(), driver.GetFinish(), 7, Element.ALIGN_RIGHT, widths[7]));
                table.AddCell(GenerateDriverCell(driver.GetOffNext(), driver.GetFinish(), 8, Element.ALIGN_RIGHT, widths[8]));
            }

            return table;
        }

        /// <summary>
        /// Generates a specific cell for a result row.
        /// </summary>
        /// <param name="data">The data that should be placed into the cell.</param>
        /// <param name="verPos">The vertical position of the cell. (The row number.)</param>
        /// <param name="horizPos">The horizontal position of the cell</param>
        /// <param name="justify">The justification of the clel.</param>
        /// <param name="width">The width of the cell.</param>
        /// <returns></returns>
        private static PdfPCell GenerateDriverCell(string data, int verPos, int horizPos, int justify, float width)
        {
            int border = 0;
            float fontSize = 9f;

            if (data!=null)
            {
                /**
                 * A bit of a hack, but... this piece of code shrinks the font size until it can fit within the confines of its cell.
                 * On average, the width point of a string is about 4.25 times larger than the width of a cell.
                 * So, we must keep the widthpoint smaller than 4.25 times the size of the cell width, and will decrease the font size until
                 * we get there.
                 **/
                while ((FontFactory.GetFont(FontFactory.HELVETICA, fontSize).BaseFont.GetWidthPoint(data, fontSize)) > width * 4.25)
                {
                    fontSize -= .1f;       
                }
            }
            
            //determine whether or not to have line underneath this row.
            if (verPos % 3 == 0)
            {
                border = Rectangle.BOTTOM_BORDER;
            }
            
           
            
            return new PdfPCell(new Phrase(data, FontFactory.GetFont(FontFactory.HELVETICA, fontSize)))
            {
                Border = border,
                Colspan = 1,
                PaddingTop = 2,
                PaddingBottom = 2,
                HorizontalAlignment = justify, 
                
            };
        }

    }
}
