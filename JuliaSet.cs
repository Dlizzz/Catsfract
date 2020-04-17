using System;
using System.Numerics;
using CatsControls;
using Windows.ApplicationModel.Resources;

namespace Catsfract
{
    class JuliaSet : IPointsSet
    {
        private readonly ResourceLoader resourceLoader;
        private int _maxValue;
        private double _sa, _sb;

        public JuliaSet(int maxValue, (double, double) seed)
        {
            MaxValue = maxValue;
            resourceLoader = ResourceLoader.GetForCurrentView("ErrorMessages");
            (_sa, _sb) = seed;
        }

        public (double, double) Seed
        {
            get => (_sa, _sb);
            set
            {
                (_sa, _sb) = value;
            }
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
            double za = ca;
            double zb = cb;
            double zasq, zbsq, magnsq;

            while (n < _maxValue)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + _sb;
                za = zasq - zbsq + _sa;

                magnsq = za * za + zb * zb;

                if (magnsq > 4) break;

                n++;
            }
            // n from 0 to _maxValue (included)
            return n;
        }
    }
}
