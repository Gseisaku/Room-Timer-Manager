using MultiRoomTimer.Models;
using MultiRoomTimer.Commands;
using System;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;

namespace MultiRoomTimer.ViewModels
{
    public class RoomViewModel : ViewModelBase
    {
        private readonly RoomSession _session;
        private readonly DispatcherTimer _timer;
        private TimeSpan _remainingTime;
        private Brush _statusBrush = Brushes.LightGray;

        public int RoomNumber => _session.RoomNumber;

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }

        public string CastName
        {
            get => _session.CastName ?? "";
            set
            {
                if (_session.CastName != value)
                {
                    _session.CastName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CourseType
        {
            get => _session.CourseType ?? "";
            set
            {
                if (_session.CourseType != value)
                {
                    _session.CourseType = value;
                    OnPropertyChanged();
                }
            }
        }

        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set
            {
                _remainingTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingTimeString));
            }
        }

        public string RemainingTimeString => $"{(int)RemainingTime.TotalMinutes:00}:{RemainingTime.Seconds:00}";

        public Brush StatusBrush
        {
            get => _statusBrush;
            set
            {
                _statusBrush = value;
                OnPropertyChanged();
            }
        }

        public RoomViewModel(RoomSession session)
        {
            _session = session;
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            StartCommand = new RelayCommand(ExecuteStartTimer, CanStartTimer);
            StopCommand = new RelayCommand(ExecuteStopTimer, CanStopTimer);
        }

        private bool CanStartTimer(object? parameter) => _session.Status == TimerStatus.Available;
        private bool CanStopTimer(object? parameter) => _session.Status != TimerStatus.Available;

        private void ExecuteStartTimer(object? parameter)
        {
            if (int.TryParse(parameter?.ToString(), out int durationMinutes))
            {
                _session.StartTime = DateTime.Now;
                _session.EndTime = _session.StartTime.Value.AddMinutes(durationMinutes);
                _session.Status = TimerStatus.InUse;
                UpdateStatusBrush();
                _timer.Start();
            }
        }

        private void ExecuteStopTimer(object? parameter)
        {
            _session.Status = TimerStatus.Available;
            _session.CastName = null;
            _session.CourseType = null;
            _session.StartTime = null;
            _session.EndTime = null;
            RemainingTime = TimeSpan.Zero;
            UpdateStatusBrush();
            OnPropertyChanged(nameof(CastName));
            OnPropertyChanged(nameof(CourseType));
            _timer.Stop();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_session.EndTime.HasValue)
            {
                var now = DateTime.Now;
                if (now < _session.EndTime.Value)
                {
                    RemainingTime = _session.EndTime.Value - now;
                    if (RemainingTime.TotalMinutes < 10 && _session.Status != TimerStatus.Warning)
                    {
                        _session.Status = TimerStatus.Warning;
                        UpdateStatusBrush();
                    }
                }
                else
                {
                    RemainingTime = TimeSpan.Zero;
                    _session.Overtime = now - _session.EndTime.Value;
                    _session.Status = TimerStatus.Finished;
                    UpdateStatusBrush();
                    _timer.Stop();
                }
            }
        }

        private void UpdateStatusBrush()
        {
            StatusBrush = _session.Status switch
            {
                TimerStatus.Available => Brushes.LightGray,
                TimerStatus.InUse => Brushes.LightGreen,
                TimerStatus.Warning => Brushes.Yellow,
                TimerStatus.Finished => Brushes.Salmon,
                _ => Brushes.LightGray,
            };
        }

        public RoomSession GetSession() => _session;
    }
}