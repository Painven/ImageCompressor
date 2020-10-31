using System;

namespace ImageCompressor
{
    public class ProgressStatus
    {
        public int Current { get; }
        public int Total { get; }
        public int Left => Math.Max(Total - Current, 0);
        public double Percent => (double)Current / Total;

        public ProgressStatus(int current, int total)
        {
            Current = current;
            Total = total;
        }
    }
}
