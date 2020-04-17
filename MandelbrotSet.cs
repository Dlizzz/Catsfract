using System;
using System.Numerics;
using CatsControls;
using Windows.ApplicationModel.Resources;

namespace Catsfract
{
    class MandelbrotSet: IPointsSet 
    {
        private int _maxValue;
        // Get resource loader for the library
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("ErrorMessages");


        public MandelbrotSet(int maxValue)
        {
            MaxValue = maxValue;
        }

        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(MaxValue), resourceLoader.GetString("ValueNotStrictlyPositive"));
                _maxValue = value;
            }
        }

        public int PointSetWorker(double ca, double cb)
        {
            int n = 0;
            double za = 0;
            double zb = 0;
            double zasq, zbsq, magnsq;

            while (n < _maxValue)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + cb;
                za = zasq - zbsq + ca;

                magnsq = za * za + zb * zb;

                if (magnsq > 4) break;

                n++;
            }
            // n from 0 to _maxValue (included)
            return n;
        }
    }
}
