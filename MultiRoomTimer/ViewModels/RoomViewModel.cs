using MultiRoomTimer.Models;
using MultiRoomTimer.Commands;
using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Collections.Generic;
using System.Globalization;

namespace MultiRoomTimer.ViewModels
{
    public class RoomViewModel : ViewModelBase
    {
        private readonly RoomSession _session;
        private readonly DispatcherTimer _timer;
        private TimeSpan _displayTime;
        private Brush _statusBrush = Brushes.LightGray;
        private string _pauseButtonContent = "一時停止";
        private string _estimatedEndTimeString = "";
        private bool _isBlinkingAfterCall = false;
        private string _manualEndHourString = "";
        private string _manualEndMinuteString = "";

        // --- Commands ---
        public RelayCommand StartCommand { get; }
        public RelayCommand TogglePauseCommand { get; }
        public RelayCommand EndCommand { get; }
        public RelayCommand ResetCommand { get; }
        public RelayCommand AddTimeCommand { get; }
        public RelayCommand CallCommand { get; }

        public event Action<RoomSession>? SessionEnded;

        // --- Properties for UI Binding ---
        public int RoomNumber => _session.RoomNumber;
        public string TenMinuteCallStatusText => _session.TenMinuteCallMade ? "済" : "-";
        public string EndCallStatusText => _session.EndCallMade ? "済" : "-";

        public string CastName
        {
            get => _session.CastName ?? "";
            set
            {
                if (_session.CastName != value)
                {
                    _session.CastName = value;
                    OnPropertyChanged();
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public List<int> CourseOptions { get; } = new List<int> { 45, 60, 70, 90, 120 };
        public int SelectedCourse
        {
            get => _session.CourseMinutes;
            set
            {
                if (_session.CourseMinutes != value)
                {
                    _session.CourseMinutes = value;
                    OnPropertyChanged();
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public List<SessionType> TypeOptions { get; } = new List<SessionType> { SessionType.F, SessionType.H, SessionType.N };
        public SessionType? SelectedType
        {
            get => _session.Type;
            set
            {
                if (_session.Type != value)
                {
                    _session.Type = value;
                    OnPropertyChanged();
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ManualEndHourString
        {
            get => _manualEndHourString;
            set
            {
                if (_manualEndHourString != value)
                {
                    _manualEndHourString = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCourseSelectionEnabled));
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string ManualEndMinuteString
        {
            get => _manualEndMinuteString;
            set
            {
                if (_manualEndMinuteString != value)
                {
                    _manualEndMinuteString = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCourseSelectionEnabled));
                    StartCommand.RaiseCanExecuteChanged();
                }
            }
        }


        public bool IsCourseSelectionEnabled => string.IsNullOrWhiteSpace(ManualEndHourString) && string.IsNullOrWhiteSpace(ManualEndMinuteString);

        public TimeSpan DisplayTime
        {
            get => _displayTime;
            set { _displayTime = value; OnPropertyChanged(nameof(DisplayTimeString)); }
        }

        public string DisplayTimeString =>
            _session.Status == TimerStatus.Finished
            ? $"+{(int)_displayTime.TotalMinutes:00}:{_displayTime.Seconds:00}"
            : $"{(int)_displayTime.TotalMinutes:00}:{_displayTime.Seconds:00}";

        public string EndTimeString => _session.EndTime.HasValue && _session.Status != TimerStatus.Available ? _session.EndTime.Value.ToString("HH:mm") : "";

        public Brush StatusBrush
        {
            get => _statusBrush;
            set { _statusBrush = value; OnPropertyChanged(); }
        }

        public string PauseButtonContent
        {
            get => _pauseButtonContent;
            set { _pauseButtonContent = value; OnPropertyChanged(); }
        }

        public bool IsAvailable => _session.Status == TimerStatus.Available;

        public string EstimatedEndTimeString
        {
            get => _estimatedEndTimeString;
            set { _estimatedEndTimeString = value; OnPropertyChanged(); }
        }

        public bool IsBlinkingAfterCall
        {
            get => _isBlinkingAfterCall;
            set
            {
                if (_isBlinkingAfterCall != value)
                {
                    _isBlinkingAfterCall = value;
                    OnPropertyChanged();
                }
            }
        }

        // --- Constructor ---
        public RoomViewModel(RoomSession session)
        {
            _session = session;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            StartCommand = new RelayCommand(ExecuteStart, CanStart);
            TogglePauseCommand = new RelayCommand(ExecuteTogglePause, CanTogglePause);
            EndCommand = new RelayCommand(ExecuteEnd, CanEnd);
            ResetCommand = new RelayCommand(ExecuteReset, CanReset);
            AddTimeCommand = new RelayCommand(ExecuteAddTime, CanAddTime);
            CallCommand = new RelayCommand(ExecuteCall, CanCall);

            UpdateStatus();
        }

        // --- Command Logic ---

        private bool CanCall(object? p) => (_session.Status == TimerStatus.Warning || _session.Status == TimerStatus.Finished) && !_session.IsCallButtonPressed;
        private void ExecuteCall(object? p)
        {
            _session.IsCallButtonPressed = true;

            if (_session.Status == TimerStatus.Warning)
            {
                _session.TenMinuteCallMade = true;
                OnPropertyChanged(nameof(TenMinuteCallStatusText));
            }
            else if (_session.Status == TimerStatus.Finished)
            {
                _session.EndCallMade = true;
                OnPropertyChanged(nameof(EndCallStatusText));
                IsBlinkingAfterCall = true;
            }

            UpdateStatusBrush();
            CallCommand.RaiseCanExecuteChanged();
        }

        private bool CanStart(object? p)
        {
            // Base conditions must always be met.
            if (_session.Status != TimerStatus.Available || string.IsNullOrWhiteSpace(CastName) || !_session.Type.HasValue)
            {
                return false;
            }

            // Determine the entry mode. If any manual time field has input, it's manual mode.
            bool isManualMode = !string.IsNullOrWhiteSpace(ManualEndHourString) || !string.IsNullOrWhiteSpace(ManualEndMinuteString);

            if (isManualMode)
            {
                // In manual mode, both hour and minute must be filled and form a valid time.
                if (string.IsNullOrWhiteSpace(ManualEndHourString) || string.IsNullOrWhiteSpace(ManualEndMinuteString))
                {
                    return false; // Incomplete time
                }
                var manualTimeString = $"{ManualEndHourString}:{ManualEndMinuteString}";
                return TimeSpan.TryParseExact(manualTimeString, new[] { "H:m", "H:mm", "HH:m", "HH:mm" }, CultureInfo.InvariantCulture, TimeSpanStyles.None, out _);
            }
            else
            {
                // In course mode, a course must be selected.
                return _session.CourseMinutes > 0;
            }
        }
        private void ExecuteStart(object? p)
        {
            _session.Status = TimerStatus.Running;
            _session.StartTime = DateTime.Now;

            var manualTimeString = $"{ManualEndHourString}:{ManualEndMinuteString}";
            if (!IsCourseSelectionEnabled && TimeSpan.TryParseExact(manualTimeString, new[] { "H:m", "H:mm", "HH:m", "HH:mm" }, CultureInfo.InvariantCulture, TimeSpanStyles.None, out var manualTime))
            {
                var now = DateTime.Now;
                var startTimeWithSecondsReset = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
                var manualEndTime = now.Date + manualTime;

                if (manualEndTime < startTimeWithSecondsReset)
                {
                    manualEndTime = manualEndTime.AddDays(1);
                }

                var duration = manualEndTime - startTimeWithSecondsReset;
                _session.EndTime = now.Add(duration);
            }
            else
            {
                _session.EndTime = _session.StartTime.Value.AddMinutes(_session.CourseMinutes);
            }

            EstimatedEndTimeString = $"終了予定: {_session.EndTime:HH:mm}";
            _timer.Start();
            UpdateStatus();
        }

        private bool CanTogglePause(object? p) => _session.Status == TimerStatus.Running || _session.Status == TimerStatus.Warning || _session.Status == TimerStatus.Paused || _session.Status == TimerStatus.Finished;
        private void ExecuteTogglePause(object? p)
        {
            if (_session.Status == TimerStatus.Paused) // Resume
            {
                _session.Status = _session.StatusBeforePause;
                // Recalculate EndTime based on the current time and the time that was remaining.
                _session.EndTime = DateTime.Now.Add(_session.RemainingTimeOnPause);
                _session.RemainingTimeOnPause = TimeSpan.Zero;
                _timer.Start();
            }
            else // Pause
            {
                _timer.Stop();
                _session.StatusBeforePause = _session.Status;
                _session.Status = TimerStatus.Paused;
                if (_session.EndTime.HasValue)
                {
                    // Always calculate remaining time from EndTime.
                    // This will be negative if in overtime.
                    _session.RemainingTimeOnPause = _session.EndTime.Value - DateTime.Now;
                    DisplayTime = _session.StatusBeforePause == TimerStatus.Finished
                                ? _session.Overtime
                                : _session.RemainingTimeOnPause;
                }
            }
            UpdateStatus();
        }

        private bool CanEnd(object? p) => _session.Status != TimerStatus.Available;
        private void ExecuteEnd(object? p)
        {
            _timer.Stop();
            if (_session.Status != TimerStatus.Finished)
            {
                 _session.EndTime = DateTime.Now;
            }

            var sessionSnapshot = new RoomSession(_session.RoomNumber) {
                CastName = _session.CastName,
                CourseMinutes = _session.CourseMinutes,
                Type = _session.Type,
                StartTime = _session.StartTime,
                EndTime = _session.EndTime,
                ScheduledEndTime = _session.StartTime?.AddMinutes(_session.CourseMinutes),
                Overtime = _session.Overtime
            };
            SessionEnded?.Invoke(sessionSnapshot);

            ResetState();
        }

        private bool CanReset(object? p) => _session.Status == TimerStatus.Paused;
        private void ExecuteReset(object? p)
        {
            _timer.Stop();
            ResetState();
        }

        private void ResetState()
        {
            _session.Reset();
            DisplayTime = TimeSpan.Zero;
            EstimatedEndTimeString = "";
            IsBlinkingAfterCall = false;
            ManualEndHourString = "";
            ManualEndMinuteString = "";
            OnPropertyChanged(nameof(TenMinuteCallStatusText));
            OnPropertyChanged(nameof(EndCallStatusText));
            UpdateStatus();
        }

        private bool CanAddTime(object? p) => _session.Status == TimerStatus.Paused;
        private void ExecuteAddTime(object? p)
        {
            if (!_session.EndTime.HasValue) return;

            // First, always extend the final end time.
            _session.EndTime = _session.EndTime.Value.AddMinutes(30);

            // Add 30 minutes to the remaining time, which could be negative (overtime).
            _session.RemainingTimeOnPause += TimeSpan.FromMinutes(30);

            // If the timer was in overtime, but now has positive time remaining, its state needs to change.
            if (_session.StatusBeforePause == TimerStatus.Finished)
            {
                IsBlinkingAfterCall = false; // Stop the blinking

                // Reset call flags
                _session.TenMinuteCallMade = false;
                _session.EndCallMade = false;
                _session.IsCallButtonPressed = false;
                OnPropertyChanged(nameof(TenMinuteCallStatusText));
                OnPropertyChanged(nameof(EndCallStatusText));
                CallCommand.RaiseCanExecuteChanged();

                // Determine the new state based on the updated remaining time.
                if (_session.RemainingTimeOnPause.TotalSeconds > 0)
                {
                    _session.StatusBeforePause = _session.RemainingTimeOnPause.TotalMinutes < 10
                        ? TimerStatus.Warning
                        : TimerStatus.Running;
                }
                // If it's still negative, it remains in the Finished state upon resume.
            }

            // Update display with the newly calculated remaining time.
            DisplayTime = _session.RemainingTimeOnPause > TimeSpan.Zero
                ? _session.RemainingTimeOnPause
                : TimeSpan.Zero;
            EstimatedEndTimeString = $"終了予定: {_session.EndTime:HH:mm}";
        }


        // --- Timer Logic ---
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_session.EndTime.HasValue && _session.Status != TimerStatus.Paused)
            {
                var now = DateTime.Now;
                if (now < _session.EndTime.Value)
                {
                    DisplayTime = _session.EndTime.Value - now;
                    var newStatus = (DisplayTime.TotalMinutes < 10) ? TimerStatus.Warning : TimerStatus.Running;
                    if (newStatus != _session.Status)
                    {
                        _session.Status = newStatus;
                        UpdateStatusBrush();
                        CallCommand.RaiseCanExecuteChanged();
                    }
                }
                else
                {
                    DisplayTime = now - _session.EndTime.Value;
                    _session.Overtime = DisplayTime;
                    if (_session.Status != TimerStatus.Finished)
                    {
                         _session.Status = TimerStatus.Finished;
                         _session.IsCallButtonPressed = false;
                         UpdateStatusBrush();
                         CallCommand.RaiseCanExecuteChanged();
                    }
                }
            }
        }

        private void UpdateStatus()
        {
            UpdateStatusBrush();
            PauseButtonContent = _session.Status == TimerStatus.Paused ? "再開" : "一時停止";

            OnPropertyChanged(nameof(IsAvailable));

            StartCommand.RaiseCanExecuteChanged();
            TogglePauseCommand.RaiseCanExecuteChanged();
            EndCommand.RaiseCanExecuteChanged();
            ResetCommand.RaiseCanExecuteChanged();
            AddTimeCommand.RaiseCanExecuteChanged();
            CallCommand.RaiseCanExecuteChanged();

            if (_session.Status == TimerStatus.Available)
            {
                DisplayTime = TimeSpan.Zero;
                OnPropertyChanged(nameof(CastName));
                OnPropertyChanged(nameof(SelectedCourse));
                OnPropertyChanged(nameof(SelectedType));
            }
            OnPropertyChanged(nameof(EndTimeString));
        }

        private void UpdateStatusBrush()
        {
            if (_session.IsCallButtonPressed)
            {
                StatusBrush = Brushes.LightGreen;
                return;
            }
            StatusBrush = _session.Status switch
            {
                TimerStatus.Available => Brushes.LightGray,
                TimerStatus.Running => Brushes.LightGreen,
                TimerStatus.Warning => Brushes.Yellow,
                TimerStatus.Paused => Brushes.LightGreen,
                TimerStatus.Finished => Brushes.Salmon,
                _ => Brushes.LightGray,
            };
        }

        // This is needed for the "Export All" feature
        public RoomSession GetSession() => _session;
    }
}
