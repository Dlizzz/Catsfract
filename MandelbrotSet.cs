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

        public MandelbrotSet(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Point originComplex, double zoom,  Color inSetColor, int threshold = 100)
            : base(resourceCreator, sizeCanvas, originComplex, zoom)
        {
            if (inSetColor == null) throw new ArgumentNullException(nameof(inSetColor));

            InSetColor = inSetColor;
            Threshold = threshold;
        }

        public MandelbrotSet(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Point originComplex, double zoom) 
            : base(resourceCreator, sizeCanvas, originComplex, zoom)
        {
            InSetColor = Colors.Black;
            Threshold = 100;
        }

        public Color InSetColor { get; set; }

        public int Threshold
        {
            get => _threshold;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Threshold), "Value must be strictly positive");
                _threshold = value;
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

                if (zasq + zbsq > 4) break;
                n++;
            }

            speed = (double)(_threshold - n) / _threshold * 100;

            return (n != _threshold);
        }

    }
}
