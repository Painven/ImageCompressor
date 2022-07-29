using ImageCompressor.Commands;
using ImageCompressorLib;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ImageCompressor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly ImageMultiCompressor compressor;
        readonly ImageWatermarkEraser watermarkEraser;
        
        public ObservableCollection<string> Log { get; }

        string title = "Обработчик изображений";
        public string Title
        {
            get => title;
            set => Set(ref title, value);
        }

        bool inProgress;
        public bool InProgress
        {
            get => inProgress;
            set => Set(ref inProgress, value);
        }

        public CompressParameters CompressParameters { get; }

        private ProgressStatus _progressStatus = new ProgressStatus(0, 0);
        public ProgressStatus ProgressStatus 
        { 
            get => _progressStatus; 
            set => Set(ref _progressStatus, value); 
        }

        string _workingFolder = "";
        public string WorkingFolder 
        { 
            get => _workingFolder; 
            set  
            {
                if (Set(ref _workingFolder, value))
                {
                    RaisePropertyChanged(nameof(WorkingFolderExists));
                }
            } 
        }

        private bool WorkingFolderExists
        {
            get
            {
                return Directory.Exists(WorkingFolder);
            }
        }

        public ICommand CopyErrorsTextCommand { get; }
        public ICommand ResizeImagesCommand { get; }
        public ICommand ConvertImagesCommand { get; }
        public ICommand CompressImagesCommand { get; }
        public ICommand EraseWatermarksCommand { get; }
        public ICommand SelectImagesFolderCommand { get; }

        public MainWindowViewModel()
        {
            CompressParameters = new CompressParameters();
            Log = new ObservableCollection<string>();          

            SelectImagesFolderCommand = new LambdaCommand(SelectImagesFolder);
            CopyErrorsTextCommand = new LambdaCommand((param) => Clipboard.SetText(string.Join(Environment.NewLine, Log)), (p) => Log.Count > 0);
            ResizeImagesCommand = new LambdaCommand(ResizeImages, (p) => WorkingFolderExists);
            ConvertImagesCommand = new LambdaCommand(ConvertImages, (p) => WorkingFolderExists);
            CompressImagesCommand = new LambdaCommand(CompressImages, (p) => WorkingFolderExists);
            EraseWatermarksCommand = new LambdaCommand(EraseWatermakrs, (p) => WorkingFolderExists);
        }

        public MainWindowViewModel(ImageMultiCompressor compressor, ImageWatermarkEraser watermarkEraser) : this()
        {
            this.compressor = compressor;
            this.watermarkEraser = watermarkEraser;

            compressor.OnError += (error) => App.Current.Dispatcher.Invoke(() => Log.Add(error));
        }

        private void SelectImagesFolder(object obj)
        {
            var ofd = new System.Windows.Forms.FolderBrowserDialog();
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                WorkingFolder = ofd.SelectedPath;
            }
        }

        private async void EraseWatermakrs(object obj)
        {
            try
            {
                var images = Directory.GetFiles(WorkingFolder, "*.*", SearchOption.AllDirectories).ToList();

                SizeF watermarkSize = new SizeF(CompressParameters.WatermarkWidth, CompressParameters.WatermarkHeight);
                await watermarkEraser.EraseWatermarks(images, watermarkSize, CreateIndicatorCallback());

                MessageBox.Show($"Изменение размеров выполнено ({images.Count}) файлов", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ConvertImages(object param)
        {
            Log.Clear();

            try
            {
                InProgress = true;
              
                await compressor.SaveAllAsJpg(WorkingFolder, CompressParameters.SelectedQuality, CreateIndicatorCallback());

                MessageBox.Show($"Конвертация в JPG выполнена", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                InProgress = false;
            }
        }

        private async void ResizeImages(object param)
        {
            Log.Clear();

            InProgress = true;
            try
            {               
                if (CompressParameters.ResizeWidth > 32 && CompressParameters.ResizeHeight > 32)
                {
                    await compressor.ResizeImages(WorkingFolder, 
                        new System.Drawing.Size(CompressParameters.ResizeWidth, CompressParameters.ResizeHeight), CreateIndicatorCallback());

                    MessageBox.Show($"Изменение размеров выполнено", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                InProgress = false;
            }
        }

        private async void CompressImages(object param)
        {           
            InProgress = true;
            try
            {
                await compressor.CompressImages(WorkingFolder, 
                    CompressParameters.SelectedQuality, 
                    CompressParameters.MinimumSizeToCompressInKb, 
                    CreateIndicatorCallback());

                MessageBox.Show($"Сжатие изображений выполнено", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n\r\n" + ex.StackTrace, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                InProgress = false;
            }
        }

        public IProgress<ProgressStatus> CreateIndicatorCallback()
        {
            return new Progress<ProgressStatus>((v) =>
            {
                ProgressStatus = v;
                Title = $"Выполнение операции: {v.Current} / {v.Total} ({v.Percent:P0})";
            });
        }
    }
}
