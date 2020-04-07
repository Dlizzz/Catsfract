using System;
using System.Numerics;
using Windows.UI;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace Catsfract
{
    class MandelbrotSet: CanvasPoints
    {
        private int _threshold;
        private readonly Color renderTransparent = new Color { A = 0, R = 0, G = 0, B = 0 };

        public MandelbrotSet(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Vector2 origin, float scale)
            : base(resourceCreator, sizeCanvas, origin, scale) 
        {
            _threshold = 100;

            // We can now calculate as the worker thread is implemented
            Calculate();
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

        // TODO: Remove direct call to ColorScale - Put it as property        
        protected override void Worker(int pointIndex)
        {
            Complex c = ToComplex(pointIndex);

            renderPixels[pointIndex] =
                Diverging(c.Real, c.Imaginary, out double speed)
                ? ColorScale.Viridis[Convert.ToInt32(speed)].ARGBValue
                : renderTransparent;
        }

        private bool Diverging(double ca, double cb, out double speed)
        {
            int n = 0;
            double za = 0;
            double zb = 0;

            while (n < _threshold)
            {
                double zasq = za * za;
                double zbsq = zb * zb;

                // new za must be calculated after new zb, as new zb is calculated from za
                zb = 2 * za * zb + cb;
                za = zasq - zbsq + ca;

                if (za * za + zb * zb > 4) break;

                n++;
            }

            speed = (double)(_threshold - n) / _threshold * 100;

            return (n != _threshold);
        }

    }
}
