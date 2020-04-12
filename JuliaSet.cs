using System;
using System.Numerics;
using CatsControls;
using Windows.ApplicationModel.Resources;

namespace Catsfract
{
    class JuliaSet : IPointsSet
    {
        private readonly ResourceLoader resourceLoader;
        private int _threshold;

        public JuliaSet(int threshold, Complex seed)
        {
            Threshold = threshold;
            resourceLoader = ResourceLoader.GetForCurrentView("ErrorMessages");
            Seed = seed;
        }

        public Complex Seed { get; set;  }

        public int Threshold
        {
            get => _threshold;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Threshold), resourceLoader.GetString("ValueNotStrictlyPositive"));
                _threshold = value;
            }
        }

        public double PointSetWorker(Complex c)
        {
            int n = 0;
            double za = c.Real;
            double zb = c.Imaginary;
            double zasq, zbsq, magnsq;

            while (n < _threshold)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + Seed.Imaginary;
                za = zasq - zbsq + Seed.Real;

                magnsq = za * za + zb * zb;

                if (magnsq > 4) break;

                n++;
            }

            return (double)(_threshold - n) / _threshold;
        }
    }
}
