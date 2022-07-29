using System;

namespace ImageCompressorLib
{
    public struct ProgressStatus
    {
        public int Current { get; private set; }
        public int Total { get; private set; }

        public int Left => Math.Max(Total - Current, 0);

        public double Percent
        {
            get
            {
                return Total > 0 ? (double)((double)Current / Total) : 0.00d;
            }
        }

        public ProgressStatus(int current, int total)
        {
            Current = current;
            Total = total;
        }
    }
}
