using ClosedXML.Excel;
using MultiRoomTimer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiRoomTimer.Services
{
    public class ExcelExportService
    {
        private readonly string[] _headers = {
            "Room Number", "Cast Name", "Course (min)", "Type",
            "Start Time", "End Time", "Overtime"
        };

        private void SetHeaders(IXLWorksheet worksheet)
        {
            for (int i = 0; i < _headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = _headers[i];
            }
            worksheet.Row(1).Style.Font.Bold = true;
        }

        private void WriteSessionRow(IXLWorksheet worksheet, int row, RoomSession session)
        {
            worksheet.Cell(row, 1).Value = session.RoomNumber;
            worksheet.Cell(row, 2).Value = session.CastName;
            worksheet.Cell(row, 3).Value = session.CourseMinutes;
            worksheet.Cell(row, 4).Value = session.Type?.ToString();
            worksheet.Cell(row, 5).Value = session.StartTime?.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 6).Value = session.EndTime?.ToString("yyyy-MM-dd HH:mm:ss");
            worksheet.Cell(row, 7).Value = session.Overtime.TotalMinutes > 0 ? session.Overtime.ToString(@"hh\:mm\:ss") : "";
        }

        /// <summary>
        /// Exports all currently active sessions to a new file.
        /// </summary>
        public void ExportAll(IEnumerable<RoomSession> sessions, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Active Sessions");
                SetHeaders(worksheet);

                int row = 2;
                foreach (var session in sessions)
                {
                    WriteSessionRow(worksheet, row++, session);
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        /// <summary>
        /// Appends a single completed session to a daily report file.
        /// Creates the file and adds headers if it doesn't exist.
        /// </summary>
        public void AppendSession(RoomSession session, string filePath)
        {
            XLWorkbook workbook;
            IXLWorksheet worksheet;

            if (File.Exists(filePath))
            {
                workbook = new XLWorkbook(filePath);
                worksheet = workbook.Worksheet(1);
            }
            else
            {
                workbook = new XLWorkbook();
                worksheet = workbook.Worksheets.Add("Daily Report");
                SetHeaders(worksheet);
            }

            int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            WriteSessionRow(worksheet, lastRow + 1, session);

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(filePath);
        }
    }
}