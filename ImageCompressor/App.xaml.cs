using ImageCompressor.ViewModels;
using ImageCompressorLib;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImageCompressor
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var compressor = new ImageMultiCompressor();
            var watermarkEraser = new ImageWatermarkEraser();
            var settingsManager = new SettingsManager<UserSettings>("settings_local.json");

            var window = new MainWindow();
            window.DataContext = new MainWindowViewModel(compressor, watermarkEraser, settingsManager);
            window.Show();          
        }

        protected override void OnExit(ExitEventArgs e)
        {
            
        }
    }

    public class UserSettings
    {
        public string WorkingFolder { get; set; }
    }
}
