using System;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace Catsfract
{
    abstract class CanvasPoints: IDisposable
    {
        private bool disposed = false;

        private readonly ICanvasResourceCreatorWithDpi renderResourceCreator;
        private Size _sizeCanvas;
        private Point _OriginComplexPlan;
        private double _zoom;
        protected Color[] renderPixels;

        public CanvasPoints(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Point originComplexPlan, double zoom)
        {
            renderResourceCreator = resourceCreator;
            SizeCanvas = sizeCanvas;
            OriginComplexPlan = originComplexPlan;
            Zoom = zoom;
        }

        public CanvasRenderTarget RenderTarget { get; private set; }

        // TODO: Move renderPixels array to ArrayPool
        public Size SizeCanvas 
        { 
            get => _sizeCanvas; 
            set
            {
                if (value.Width < 0 || value.Height < 0) throw new ArgumentOutOfRangeException(nameof(SizeCanvas), "Value must be positive.");
                _sizeCanvas = value;
                
                RenderTarget?.Dispose();
                float width = Convert.ToSingle(_sizeCanvas.Width);
                float height = Convert.ToSingle(_sizeCanvas.Height);
                RenderTarget = new CanvasRenderTarget(renderResourceCreator, width, height);
                renderPixels = RenderTarget.GetPixelColors();
            }
        }

        public Point OriginComplexPlan
        {
            get => _OriginComplexPlan;
            set
            {
                if (value.X < 0 || value.Y < 0) throw new ArgumentOutOfRangeException(nameof(OriginComplexPlan), "Value must be positive.");
                _OriginComplexPlan = value;
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

        public int IndexFromComplex(Complex c) => Convert.ToInt32((_OriginComplexPlan.Y - _zoom * c.Imaginary) * _sizeCanvas.Width + _zoom * c.Real + _OriginComplexPlan.X);

        public int IndexFromCanvas(Point point) => Convert.ToInt32(point.Y * _sizeCanvas.Width + point.X);

        public Complex ComplexFromIndex(int index)
        {
            int y = index / Convert.ToInt32(_sizeCanvas.Width);

            return new Complex((index - y * _sizeCanvas.Width - _OriginComplexPlan.X) / _zoom, (_OriginComplexPlan.Y - y) / _zoom);
        }

        public Complex ComplexFromCanvas(Point point) => new Complex((point.X - _OriginComplexPlan.X) / _zoom, (_OriginComplexPlan.Y - point.Y) / _zoom);

        public Point CanvasFromIndex(int index)
        {
            int y = index / Convert.ToInt32(_sizeCanvas.Width);

            return new Point(index - y * _sizeCanvas.Width, y);
        }

        public Point CanvasFromComplex(Complex c) => new Point(_zoom * c.Real + _OriginComplexPlan.X, -_zoom * c.Imaginary + _OriginComplexPlan.Y);

        public void Calculate()
        {
            Parallel.For(0, PointsCount, Worker);

            RenderTarget.SetPixelColors(renderPixels);
        }

        protected abstract void Worker(int i);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Do nothing if already disposed
            if (disposed) return;

            // If the call is from Dispose, free managed resources.
            if (disposing)
            {
                RenderTarget?.Dispose();
            }

            disposed = true;
        }
    }
}
