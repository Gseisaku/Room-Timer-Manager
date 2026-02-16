using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MultiRoomTimer.Services;

namespace MultiRoomTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string getCastScheduleExe = Path.Combine(baseDir, "GetCastSchedule", "GetCastSchedule.exe");
                string castListPath = Path.Combine(baseDir, "GetCastSchedule", "CastSchedule", "cast_list.txt");

                var configService = new ConfigurationService();
                var config = configService.LoadConfig();

                if (File.Exists(castListPath))
                {
                    var result = MessageBox.Show("既存のキャスト出勤データを上書きしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        RunGetCastSchedule(getCastScheduleExe, config.CastDataUrl);
                    }
                }
                else
                {
                    MessageBox.Show("本日のキャスト出勤データを作成します", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
                    RunGetCastSchedule(getCastScheduleExe, config.CastDataUrl);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"起動時のキャストデータ処理中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            base.OnStartup(e);
        }

        private void RunGetCastSchedule(string exePath, string castDataUrl)
        {
            if (!File.Exists(exePath))
            {
                MessageBox.Show($"取得ツールが見つかりません:\n{exePath}", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{castDataUrl}\"",
                    WorkingDirectory = Path.GetDirectoryName(exePath),
                    UseShellExecute = false,
                    CreateNoWindow = true // Console window will not be shown
                };

                using (Process? process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"取得ツールの実行中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
