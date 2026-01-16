using MultiRoomTimer.Models;
using MultiRoomTimer.Commands;
using MultiRoomTimer.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MultiRoomTimer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<RoomViewModel> Rooms { get; }
        public ObservableCollection<string> CastNames { get; }
        public ICommand GenerateReportCommand { get; }
        public string AppVersion => "Ver.3.1.6";

        private readonly JsonDataStorageService _jsonDataStorageService;
        private readonly ExcelExportService _excelExportService;
        private readonly CastListService _castListService;

        public MainViewModel()
        {
            _jsonDataStorageService = new JsonDataStorageService();
            _excelExportService = new ExcelExportService();
            _castListService = new CastListService();

            var castList = _castListService.LoadCastList();
            CastNames = new ObservableCollection<string>(castList);

            Rooms = new ObservableCollection<RoomViewModel>();
            for (int i = 1; i <= 19; i++)
            {
                var roomVM = new RoomViewModel(new RoomSession(i), CastNames);
                roomVM.SessionEnded += OnSessionEnded;
                Rooms.Add(roomVM);
            }

            GenerateReportCommand = new RelayCommand(GenerateReport, CanGenerateReport);
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, e) => (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
            timer.Start();
        }

        private void OnSessionEnded(RoomSession endedSession)
        {
            _jsonDataStorageService.AppendSession(endedSession);
            (GenerateReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private bool CanGenerateReport(object? parameter)
        {
            return _jsonDataStorageService.HasAnyData();
        }

        private void GenerateReport(object? parameter)
        {
            try
            {
                var year = DateTime.Now.Year;
                var yearlyData = _jsonDataStorageService.GetSessionsForYear(year);
                if (yearlyData.Any())
                {
                    _excelExportService.GenerateYearlyReport(yearlyData, year);
                    MessageBox.Show($"Report for {year} generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"No data found for {year}.", "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while generating the report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}