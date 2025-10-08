using MultiRoomTimer.Models;
using MultiRoomTimer.Commands;
using MultiRoomTimer.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;

namespace MultiRoomTimer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<RoomViewModel> Rooms { get; }
        public ICommand ExportCommand { get; }

        private readonly ExcelExportService _excelExportService;

        public MainViewModel()
        {
            Rooms = new ObservableCollection<RoomViewModel>(
                Enumerable.Range(1, 19)
                          .Select(i => new RoomViewModel(new RoomSession(i)))
            );

            _excelExportService = new ExcelExportService();
            ExportCommand = new RelayCommand(ExportData);
        }

        private void ExportData(object? parameter)
        {
            var sessions = Rooms.Select(vm => vm.GetSession());
            var fileName = $"TimerReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            _excelExportService.Export(sessions, fileName);
            // In a real app, we'd show a confirmation message.
        }
    }
}