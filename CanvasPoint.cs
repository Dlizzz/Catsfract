using System;
using System.Numerics;
using Windows.UI.Xaml;
using Windows.Foundation;

namespace Catsfract
{
    class CanvasPoint
    {
        private Size _sizeCanvas;
        private Point _OriginComplex;
        private double _zoom;

        public CanvasPoint(Size sizeCanvas, Point originComplex, double zoom)
        {
            SizeCanvas = sizeCanvas;
            OriginComplex = originComplex;
            Zoom = zoom;
        }

        public Size SizeCanvas 
        { 
            get => _sizeCanvas; 
            set
            {
                if (value.Width < 0 || value.Height < 0) throw new ArgumentOutOfRangeException(nameof(SizeCanvas), "Value must be positive.");
                _sizeCanvas = value;
            }
        }

        public Point OriginComplex
        {
            get => _OriginComplex;
            set
            {
                if (value.X < 0 || value.Y < 0) throw new ArgumentOutOfRangeException(nameof(OriginComplex), "Value must be positive.");
                _OriginComplex = value;
            }
        }

        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Zoom), "Value must be positive.");
                _zoom = value;
            }
        }

        public int PointsCount
        {
            get => Convert.ToInt32(_sizeCanvas.Width * _sizeCanvas.Height);
        }

        public int IndexFromComplex(Complex c)
        {
            return Convert.ToInt32((_OriginComplex.Y - _zoom * c.Imaginary) * _sizeCanvas.Width + _zoom * c.Real + _OriginComplex.X);
        }

        public int IndexFromCanvas(Point point)
        {
            return Convert.ToInt32(point.Y * _sizeCanvas.Width + point.X);
        }

        public Complex ComplexFromIndex(int index)
        {
            int y = index / Convert.ToInt32(_sizeCanvas.Width);

            return new Complex
            (
                (double)(index - y * _sizeCanvas.Width - _OriginComplex.X) / _zoom,
                (double)(_OriginComplex.Y - y) / _zoom
            );
        }

        public Complex ComplexFromCanvas(Point point)
        {
            return new Complex
            (
                (point.X - _OriginComplex.X) / _zoom,
                (_OriginComplex.Y - point.Y) / _zoom
            );
        }

        public Point CanvasFromIndex(int index)
        {
            int y = index / Convert.ToInt32(_sizeCanvas.Width);

            return new Point
            (
                (double)(index - y * _sizeCanvas.Width),
                (double)y
            );
        }

        public Point CanvasFromComplex(Complex c)
        {
            return new Point
            (
                _zoom * c.Real + _OriginComplex.X,
                -_zoom * c.Imaginary + _OriginComplex.Y
            );
        }

    }
}
