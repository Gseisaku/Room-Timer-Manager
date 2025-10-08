using ClosedXML.Excel;
using MultiRoomTimer.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiRoomTimer.Services
{
    public class ExcelExportService
    {
        public void Export(IEnumerable<RoomSession> sessions, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Timer Data");

                // Add headers
                worksheet.Cell(1, 1).Value = "Room Number";
                worksheet.Cell(1, 2).Value = "Cast Name";
                worksheet.Cell(1, 3).Value = "Course Type";
                worksheet.Cell(1, 4).Value = "Start Time";
                worksheet.Cell(1, 5).Value = "End Time";
                worksheet.Cell(1, 6).Value = "Overtime";

                // Add data
                int row = 2;
                foreach (var session in sessions)
                {
                    if (session.StartTime.HasValue) // Only export completed or active sessions
                    {
                        worksheet.Cell(row, 1).Value = session.RoomNumber;
                        worksheet.Cell(row, 2).Value = session.CastName;
                        worksheet.Cell(row, 3).Value = session.CourseType;
                        worksheet.Cell(row, 4).Value = session.StartTime;
                        worksheet.Cell(row, 5).Value = session.EndTime;
                        worksheet.Cell(row, 6).Value = session.Overtime;
                        row++;
                    }
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }
    }
}