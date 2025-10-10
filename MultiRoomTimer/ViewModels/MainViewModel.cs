using MultiRoomTimer.Models;
using MultiRoomTimer.Commands;
using MultiRoomTimer.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;
using System.Collections.Generic;

namespace MultiRoomTimer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<RoomViewModel> Rooms { get; }
        public ICommand ExportAllCommand { get; }

        private readonly ExcelExportService _excelExportService;

        public MainViewModel()
        {
            _excelExportService = new ExcelExportService();

            Rooms = new ObservableCollection<RoomViewModel>();
            for (int i = 1; i <= 19; i++)
            {
                var roomVM = new RoomViewModel(new RoomSession(i));
                roomVM.SessionEnded += OnSessionEnded; // Subscribe to the event
                Rooms.Add(roomVM);
            }

            ExportAllCommand = new RelayCommand(ExportAllData, CanExportAllData);
        }

        private void OnSessionEnded(RoomSession endedSession)
        {
            var fileName = $"TimerReport_{DateTime.Now:yyyyMMdd}.xlsx";
            _excelExportService.AppendSession(endedSession, fileName);
            // Optionally, show a success message to the user
        }

        private bool CanExportAllData(object? parameter)
        {
            return Rooms.Any(r => r.GetSession().Status != TimerStatus.Available);
        }

        private void ExportAllData(object? parameter)
        {
            var activeSessions = Rooms.Select(vm => vm.GetSession())
                                      .Where(s => s.Status != TimerStatus.Available);

            var fileName = $"TimerReport_All_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            _excelExportService.ExportAll(activeSessions, fileName);
            // Optionally, show a success message to the user
        }
    }
}