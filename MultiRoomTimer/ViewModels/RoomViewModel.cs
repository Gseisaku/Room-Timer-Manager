using MultiRoomTimer.Models;
using MultiRoomTimer.Commands;
using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Collections.Generic;

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
            if (_session.Status == TimerStatus.Finished)
            {
                IsBlinkingAfterCall = true;
            }
            UpdateStatusBrush();
            CallCommand.RaiseCanExecuteChanged();
        }

        private bool CanStart(object? p) => _session.Status == TimerStatus.Available && !string.IsNullOrWhiteSpace(CastName) && _session.CourseMinutes > 0 && _session.Type.HasValue;
        private void ExecuteStart(object? p)
        {
            _session.Status = TimerStatus.Running;
            _session.StartTime = DateTime.Now;
            _session.EndTime = _session.StartTime.Value.AddMinutes(_session.CourseMinutes);
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
                if (_session.Status != TimerStatus.Finished)
                {
                    _session.EndTime = DateTime.Now.Add(_session.RemainingTimeOnPause);
                }
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
                    if (_session.StatusBeforePause != TimerStatus.Finished)
                    {
                        _session.RemainingTimeOnPause = _session.EndTime.Value - DateTime.Now;
                        if (_session.RemainingTimeOnPause.TotalSeconds < 0)
                        {
                            _session.RemainingTimeOnPause = TimeSpan.Zero;
                        }
                        DisplayTime = _session.RemainingTimeOnPause;
                    }
                    else
                    {
                        // Overtime is already the remaining time
                        _session.RemainingTimeOnPause = _session.Overtime;
                    }
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

            _session.Reset();
            DisplayTime = TimeSpan.Zero;
            EstimatedEndTimeString = "";
            IsBlinkingAfterCall = false;
            UpdateStatus();
        }

        private bool CanReset(object? p) => _session.Status != TimerStatus.Available;
        private void ExecuteReset(object? p)
        {
            _timer.Stop();
            _session.Reset();
            DisplayTime = TimeSpan.Zero;
            EstimatedEndTimeString = "";
            IsBlinkingAfterCall = false;
            UpdateStatus();
        }

        private bool CanAddTime(object? p) => _session.Status == TimerStatus.Paused;
        private void ExecuteAddTime(object? p)
        {
            if (_session.RemainingTimeOnPause.TotalSeconds > 0)
            {
                _session.RemainingTimeOnPause = _session.RemainingTimeOnPause.Add(TimeSpan.FromMinutes(30));
                DisplayTime = _session.RemainingTimeOnPause;
            }
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