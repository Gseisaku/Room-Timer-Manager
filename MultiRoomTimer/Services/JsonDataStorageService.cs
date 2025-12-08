using MultiRoomTimer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace MultiRoomTimer.Services
{
    public class JsonDataStorageService
    {
        private readonly string _storagePath;

        public JsonDataStorageService()
        {
            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SessionData");
            Directory.CreateDirectory(_storagePath);
        }

        public void AppendSession(RoomSession session)
        {
            if (!session.EndTime.HasValue) return;

            var date = session.EndTime.Value.ToString("yyyy-MM-dd");
            var filePath = Path.Combine(_storagePath, $"{date}.json");

            List<RoomSession> sessions = new List<RoomSession>();
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    sessions = JsonSerializer.Deserialize<List<RoomSession>>(json) ?? new List<RoomSession>();
                }
            }

            session.Seq = sessions.Count + 1;
            sessions.Add(session);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = JsonSerializer.Serialize(sessions, options);
            File.WriteAllText(filePath, newJson);
        }

        public Dictionary<string, List<RoomSession>> GetSessionsForYear(int year)
        {
            var yearlySessions = new Dictionary<string, List<RoomSession>>();
            var yearPrefix = $"{year}-";

            var files = Directory.GetFiles(_storagePath, "*.json")
                                 .Where(file => Path.GetFileName(file).StartsWith(yearPrefix))
                                 .OrderBy(file => file);

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var sessions = JsonSerializer.Deserialize<List<RoomSession>>(json);
                    if (sessions != null && sessions.Any())
                    {
                        var dateKey = Path.GetFileNameWithoutExtension(file);
                        yearlySessions[dateKey] = sessions;
                    }
                }
            }

            return yearlySessions;
        }

        public bool HasAnyData()
        {
            return Directory.GetFiles(_storagePath, "*.json").Any();
        }
    }
}