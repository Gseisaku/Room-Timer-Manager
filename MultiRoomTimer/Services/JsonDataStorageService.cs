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
            // The storage path will be a "SessionData" folder in the application's directory.
            _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SessionData");
            Directory.CreateDirectory(_storagePath);
        }

        /// <summary>
        /// Appends a session to a JSON file named after the session's end date.
        /// </summary>
        /// <param name="session">The session to save.</param>
        public void AppendSession(RoomSession session)
        {
            if (!session.EndTime.HasValue)
            {
                // Cannot save a session without an end time.
                return;
            }

            var date = session.EndTime.Value.ToString("yyyy-MM-dd");
            var filePath = Path.Combine(_storagePath, $"{date}.json");

            List<RoomSession> sessions = new List<RoomSession>();

            // If a file for this date already exists, read its content.
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                // Make sure the file is not empty before deserializing.
                if (!string.IsNullOrWhiteSpace(json))
                {
                    sessions = JsonSerializer.Deserialize<List<RoomSession>>(json) ?? new List<RoomSession>();
                }
            }

            // Add the new session and write the updated list back to the file.
            sessions.Add(session);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var newJson = JsonSerializer.Serialize(sessions, options);

            File.WriteAllText(filePath, newJson);
        }

        /// <summary>
        /// Gets all session data for a specific year.
        /// </summary>
        /// <param name="year">The year to get data for.</param>
        /// <returns>A dictionary where keys are dates (yyyy-MM-dd) and values are lists of sessions.</returns>
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

        /// <summary>
        /// Checks if any session data exists.
        /// </summary>
        public bool HasAnyData()
        {
            return Directory.GetFiles(_storagePath, "*.json").Any();
        }
    }
}