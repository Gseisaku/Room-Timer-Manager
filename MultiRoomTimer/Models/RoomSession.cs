using System;

namespace MultiRoomTimer.Models
{
    public enum TimerStatus
    {
        Available,
        InUse,
        Warning,
        Finished
    }

    public class RoomSession
    {
        public int RoomNumber { get; set; }
        public string? CastName { get; set; }
        public string? CourseType { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Overtime { get; set; }
        public TimerStatus Status { get; set; }

        public RoomSession(int roomNumber)
        {
            RoomNumber = roomNumber;
            Status = TimerStatus.Available;
        }
    }
}