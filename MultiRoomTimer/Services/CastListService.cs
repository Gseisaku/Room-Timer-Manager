using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiRoomTimer.Services
{
    public class CastListService
    {
        public List<string> LoadCastList()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Path specified by user: GetCastSchedule\CastSchedule\cast_list.txt
                string filePath = Path.Combine(baseDir, "GetCastSchedule", "CastSchedule", "cast_list.txt");

                if (File.Exists(filePath))
                {
                    return File.ReadAllLines(filePath)
                               .Where(line => !string.IsNullOrWhiteSpace(line))
                               .Select(line => line.Trim())
                               .ToList();
                }
            }
            catch (Exception)
            {
                // Silently fail and return empty list if there's an error
            }

            return new List<string>();
        }
    }
}
