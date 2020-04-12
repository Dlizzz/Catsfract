﻿using System;
using System.Numerics;
using CatsControls;
using Windows.ApplicationModel.Resources;

namespace Catsfract
{
    class MandelbrotSet: IPointsSet 
    {
        private int _threshold;
        private readonly ResourceLoader resourceLoader;


        public MandelbrotSet(int threshold)
        {
            Threshold = threshold;
            resourceLoader = ResourceLoader.GetForCurrentView("ErrorMessages");
        }

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
            double za = 0;
            double zb = 0;
            double zasq, zbsq, magnsq;

            while (n < _threshold)
            {
                zasq = za * za;
                zbsq = zb * zb;

                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + c.Imaginary;
                za = zasq - zbsq + c.Real;

                magnsq = za * za + zb * zb;

                if (magnsq > 4) break;

                n++;
            }

            return (double)(_threshold - n) / _threshold;
        }
    }
}
