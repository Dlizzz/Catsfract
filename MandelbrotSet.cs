using System;
using Windows.UI;
using System.Numerics;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catsfract
{
    class MandelbrotSet
    {
        private int _threshold;
        private CanvasPoint _canvasPoint;

        public MandelbrotSet(CanvasPoint canvasPoint, Color inSetColor, int threshold = 100)
        {
            if (inSetColor == null) throw new ArgumentNullException(nameof(inSetColor));

            CanvasPoint = canvasPoint ?? throw new ArgumentNullException(nameof(canvasPoint));
            InSetColor = inSetColor;
            Threshold = threshold;
        }

        public MandelbrotSet(CanvasPoint canvasPoint)
        {
            CanvasPoint = canvasPoint ?? throw new ArgumentNullException(nameof(canvasPoint));
            InSetColor = Colors.Black;
            Threshold = 100;
        }

        public CanvasPoint CanvasPoint 
        { 
            get => _canvasPoint;
            set
            {
                _canvasPoint = value;
                Points = null;
                Points = new Windows.UI.Color[_canvasPoint.PointsCount];
            }
        }

        public Color InSetColor { get; set; }

        public Color[] Points { get; private set; }

        public int Threshold
        {
            get => _threshold;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Threshold), "Value must be strictly positive");
                _threshold = value;
            }
        }

        public void Calculate()
        {
            for (int pointIndex = 0; pointIndex < _canvasPoint.PointsCount; pointIndex++)
            {
                Complex c = _canvasPoint.ComplexFromIndex(pointIndex);

                Points[pointIndex] = 
                    Diverging(c.Real, c.Imaginary, out double speed) 
                    ? ColorScale.Viridis[Convert.ToInt32(speed)].ARGBValue 
                    : InSetColor;
            }
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

                // new za must calculated after zb, as new zb is calculated from za
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
