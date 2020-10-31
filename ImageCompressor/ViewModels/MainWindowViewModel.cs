using ImageCompressoLib;
using ImageCompressor.Commands;
using ImageCompressorLib;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ImageCompressor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly ImageMultiCompressor compressor;
        public ObservableCollection<string> Log { get; }

        int _selectedQuality = 75;
        public int SelectedQuality { get => _selectedQuality; set => Set(ref _selectedQuality, value); }

        int _resizeWidth = 800;
        public int ResizeWidth { get => _resizeWidth; set => Set(ref _resizeWidth, value); }

        int _resizeHeight = 800;
        public int ResizeHeight { get => _resizeHeight; set => Set(ref _resizeHeight, value); }

        int _minimumSizeToCompressInKb = 400;
        public int MinimumSizeToCompressInKb { get => _minimumSizeToCompressInKb; set => Set(ref _minimumSizeToCompressInKb, value); }

        private ProgressStatus _progressStatus = new ProgressStatus(0, 0);
        public ProgressStatus ProgressStatus { get => _progressStatus; set => Set(ref _progressStatus, value); }

        string _workingFolder = DriveInfo.GetDrives().First().Name;
        public string WorkingFolder { get => _workingFolder; set  { Set(ref _workingFolder, value); RaisePropertyChanged(nameof(WorkingFolderExists)); } }


        private bool WorkingFolderExists
        {
            get
            {
                return Directory.Exists(WorkingFolder);
            }
        }

        private IProgress<Tuple<int, int>> indicator;

        public ICommand CopyErrorsTextCommand { get; }
        public ICommand ResizeImagesCommand { get; }
        public ICommand ConvertImagesCommand { get; }
        public ICommand CompressImagesCommand { get; }

        public MainWindowViewModel()
        {
            Log = new ObservableCollection<string>();
            compressor = new ImageMultiCompressor();
            compressor.OnError += (error) => Log.Add(error);

            indicator = new Progress<Tuple<int, int>>((v) => ProgressStatus = new ProgressStatus(v.Item1, v.Item2));

            CopyErrorsTextCommand = new LambdaCommand((param) => Clipboard.SetText(string.Join(Environment.NewLine, Log)), (p) => Log.Count > 0);
            ResizeImagesCommand = new LambdaCommand(ResizeImages, (p) => WorkingFolderExists && ResizeHeight > 0 && ResizeWidth > 0);
            ConvertImagesCommand = new LambdaCommand(ConvertImages, (p) => WorkingFolderExists);
            CompressImagesCommand = new LambdaCommand(CompressImages, (p) => WorkingFolderExists);
        }

        private async void ConvertImages(object param)
        {
            Log.Clear();

            try
            {
                var images = Directory.GetFiles(WorkingFolder, "*.*", SearchOption.AllDirectories).ToList();

                await compressor.SaveAllAsJpg(images, SelectedQuality, indicator);

                MessageBox.Show($"Конвертация в JPG выполнена для ({images.Count}) файлов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ResizeImages(object param)
        {
            Log.Clear();

            try
            {
                var images = Directory.GetFiles(WorkingFolder, "*.*", SearchOption.AllDirectories).ToList();

                if (ResizeWidth > 0 && ResizeHeight > 0)
                {
                    await compressor.ResizeImages(images, new System.Drawing.Size(ResizeWidth, ResizeHeight), indicator);

                    MessageBox.Show($"Изменение размеров выполнено ({images.Count}) файлов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка парсинга длины или ширины", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CompressImages(object param)
        {
            var images = Directory.GetFiles(WorkingFolder, "*.*", SearchOption.AllDirectories)
                .Select(file => new FileInfo(file))
                .Where(file => file.Length >= MinimumSizeToCompressInKb * 1000)
                .Where(file => new string[] { ".jpg", ".jpeg" }.Contains(file.Extension.ToLower()))
                .Select(fi => fi.FullName)
                .ToList();
            try
            {
                await compressor.CompressImages(images, SelectedQuality, indicator);

                MessageBox.Show($"Сжатие изображений выпонлено для ({images.Count}) файлов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
