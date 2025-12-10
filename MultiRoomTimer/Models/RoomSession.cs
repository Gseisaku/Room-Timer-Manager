using System;

namespace MultiRoomTimer.Models
{
    /// <summary>
    /// 種別 (F, H, N)
    /// </summary>
    public enum SessionType
    {
        F,
        H,
        N
    }

    /// <summary>
    /// タイマーの状態
    /// </summary>
    public enum TimerStatus
    {
        Available,  // 待機中 (操作可能)
        Running,    // 実行中
        Paused,     // 一時停止中
        Finished,   // 終了 (超過時間カウント中)
        Warning     // 実行中 (残り10分未満)
    }

    public class RoomSession
    {
        public int Seq { get; set; }
        public int RoomNumber { get; set; }
        public string? CastName { get; set; }
        public int CourseMinutes { get; set; }
        public SessionType? Type { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public TimeSpan Overtime { get; set; }
        public TimeSpan RemainingTimeOnPause { get; set; }
        public bool WasFinishedWhenPaused { get; set; }
        public TimerStatus Status { get; set; }
        public TimerStatus StatusBeforePause { get; set; }
        public bool IsCallButtonPressed { get; set; }

        public RoomSession(int roomNumber)
        {
            RoomNumber = roomNumber;
            Status = TimerStatus.Available;
        }

        public void Reset()
        {
            Status = TimerStatus.Available;
            CastName = null;
            CourseMinutes = 0;
            Type = null;
            StartTime = null;
            EndTime = null;
            ScheduledEndTime = null;
            Seq = 0;
            Overtime = TimeSpan.Zero;
            RemainingTimeOnPause = TimeSpan.Zero;
            WasFinishedWhenPaused = false;
            StatusBeforePause = TimerStatus.Available;
            IsCallButtonPressed = false;
        }
    }
}