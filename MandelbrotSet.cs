using System;
using System.Numerics;
using Windows.UI;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;

namespace Catsfract
{
    class MandelbrotSet: CanvasPoints
    {
        private int _threshold = 100;
        private Color _inSetColor = Colors.Black;

        public MandelbrotSet(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Point originComplex, double zoom)
            : base(resourceCreator, sizeCanvas, originComplex, zoom) { }

        public int Threshold
        {
            get => _threshold;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Threshold), "Value must be strictly positive");
                _threshold = value;
            }
        }

        public Color InSetColor
        {
            get => _inSetColor;
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(InSetColor));
                _inSetColor = value;
            }
        }

        // TODO: Remove direct call to ColorScale - Put it as property        
        protected override void Worker(int pointIndex)
        {
            Complex c = ComplexFromIndex(pointIndex);

            renderPixels[pointIndex] =
                Diverging(c.Real, c.Imaginary, out double speed)
                ? ColorScale.Viridis[Convert.ToInt32(speed)].ARGBValue
                : InSetColor;
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
