using ClosedXML.Excel;
using MultiRoomTimer.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace MultiRoomTimer.Services
{
    public class ExcelExportService
    {
        private readonly string[] _headers = {
            "Seq", "Room Number", "Cast Name", "Course (min)", "Type",
            "Start Time", "End Time", "Sched EndTime", "Overtime", "Remarks"
        };

        public void GenerateYearlyReport(Dictionary<string, List<RoomSession>> yearlySessions, int year)
        {
            var reportsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
            Directory.CreateDirectory(reportsPath);
            var filePath = Path.Combine(reportsPath, $"TimerReport_{year}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                foreach (var date in yearlySessions.Keys)
                {
                    var worksheet = workbook.Worksheets.Add(date);
                    SetHeaders(worksheet);

                    int row = 2;
                    int seq = 1;
                    foreach (var session in yearlySessions[date])
                    {
                        WriteSessionRow(worksheet, row++, session, seq++);
                    }
                    worksheet.Columns().AdjustToContents();
                }

                if (workbook.Worksheets.Count > 0)
                {
                    workbook.SaveAs(filePath);
                }
            }
        }

        private void SetHeaders(IXLWorksheet worksheet)
        {
            for (int i = 0; i < _headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = _headers[i];
            }
            worksheet.Row(1).Style.Font.Bold = true;
        }

        private void WriteSessionRow(IXLWorksheet worksheet, int row, RoomSession session, int seq)
        {
            worksheet.Cell(row, 1).Value = seq;
            worksheet.Cell(row, 2).Value = session.RoomNumber;
            worksheet.Cell(row, 3).Value = session.CastName;
            worksheet.Cell(row, 4).Value = session.CourseMinutes;
            worksheet.Cell(row, 5).Value = session.Type?.ToString();
            worksheet.Cell(row, 6).Value = session.StartTime?.ToString("HH:mm:ss");
            worksheet.Cell(row, 7).Value = session.EndTime?.ToString("HH:mm:ss");
            worksheet.Cell(row, 8).Value = session.ScheduledEndTime?.ToString("HH:mm:ss");
            worksheet.Cell(row, 9).Value = session.Overtime.TotalSeconds > 0 ? session.Overtime.ToString(@"hh\:mm\:ss") : "";
            worksheet.Cell(row, 10).Value = ""; // Remarks
        }
    }
}