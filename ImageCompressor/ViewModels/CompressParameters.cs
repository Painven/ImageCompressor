namespace ImageCompressor.ViewModels
{
    public class CompressParameters : ViewModelBase
    {

        int _watermarkWidth = 220;
        public int WatermarkWidth
        {
            get => _watermarkWidth;
            set => Set(ref _watermarkWidth, value);
        }

        int _watermarkHeight = 50;
        public int WatermarkHeight
        {
            get => _watermarkHeight;
            set => Set(ref _watermarkHeight, value);
        }

        int _selectedQuality = 75;
        public int SelectedQuality
        {
            get => _selectedQuality;
            set => Set(ref _selectedQuality, value);
        }

        int _resizeWidth = 800;
        public int ResizeWidth
        {
            get => _resizeWidth;
            set => Set(ref _resizeWidth, value);
        }

        int _resizeHeight = 800;
        public int ResizeHeight
        {
            get => _resizeHeight;
            set => Set(ref _resizeHeight, value);
        }

        int _minimumSizeToCompressInKb = 400;
        public int MinimumSizeToCompressInKb
        {
            get => _minimumSizeToCompressInKb;
            set => Set(ref _minimumSizeToCompressInKb, value);
        }

    }
}
