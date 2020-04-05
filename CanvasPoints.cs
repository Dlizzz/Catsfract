using System;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.Foundation;

namespace Catsfract
{
    class CanvasPoints
    {
        private Size _sizeCanvas;
        private Point _OriginComplex;
        private double _zoom;
        private int _sliceCount;
        protected List<(int start, int end)> slices;

        public CanvasPoints(Size sizeCanvas, Point originComplex, double zoom, int sliceCount = 16)
        {
            SizeCanvas = sizeCanvas;
            OriginComplex = originComplex;
            Zoom = zoom;
            SliceCount = sliceCount;
        }

        public Color[] Points { get; private set; }

        // TODO: Move Color[] Points array to ArrayPool
        public Size SizeCanvas 
        { 
            get => _sizeCanvas; 
            set
            {
                if (value.Width < 0 || value.Height < 0) throw new ArgumentOutOfRangeException(nameof(SizeCanvas), "Value must be positive.");
                _sizeCanvas = value;
                Points = null;
                Points = new Windows.UI.Color[PointsCount];
                MakeSlices();
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
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Zoom), "Value must be strictly positive.");
                _zoom = value;
            }
        }

        public int PointsCount => Convert.ToInt32(_sizeCanvas.Width * _sizeCanvas.Height);

        public int SliceCount
        {
            get => _sliceCount;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(SliceCount), "Value must be strictly positive.");
                _sliceCount = value;

                MakeSlices();
            }
        }

        public int IndexFromComplex(Complex c) => Convert.ToInt32((_OriginComplex.Y - _zoom * c.Imaginary) * _sizeCanvas.Width + _zoom * c.Real + _OriginComplex.X);

        public int IndexFromCanvas(Point point) => Convert.ToInt32(point.Y * _sizeCanvas.Width + point.X);

        public Complex ComplexFromIndex(int index)
        {
            int y = index / Convert.ToInt32(_sizeCanvas.Width);

            return new Complex
            (
                (double)(index - y * _sizeCanvas.Width - _OriginComplex.X) / _zoom,
                (double)(_OriginComplex.Y - y) / _zoom
            );
        }

        public Complex ComplexFromCanvas(Point point) => new Complex((point.X - _OriginComplex.X) / _zoom, (_OriginComplex.Y - point.Y) / _zoom);

        public Point CanvasFromIndex(int index)
        {
            int y = index / Convert.ToInt32(_sizeCanvas.Width);

            return new Point
            (
                (double)(index - y * _sizeCanvas.Width),
                (double)y
            );
        }

        public Point CanvasFromComplex(Complex c) => new Point(_zoom * c.Real + _OriginComplex.X, -_zoom * c.Imaginary + _OriginComplex.Y);

        public void Calculate()
        {
            Task[] workerTasks = new Task[_sliceCount];

            for (int i = 0; i < _sliceCount; i++)
            {
                workerTasks[i] = Task.Run(() => Worker(slices[i]));
            }
            Task.WaitAll(workerTasks);
        }

        protected virtual void Worker((int start, int end) slice) { }

        private void MakeSlices()
        {
            if (_sliceCount == 0) return;

            slices?.Clear();
            slices ??= new List<(int start, int end)>();

            int sliceSize = PointsCount / _sliceCount;
            for (int i = 0; i < _sliceCount - 1; i++)
            {
                slices.Add((start: i * sliceSize, end: (i + 1) * sliceSize));
            }
            slices.Add((start: (_sliceCount - 1) * sliceSize, end: PointsCount));
        }
    }
}
