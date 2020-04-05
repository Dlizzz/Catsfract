using System;
using System.Numerics;
using System.Buffers;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;
using Microsoft.Graphics.Canvas;

namespace Catsfract
{
    abstract class CanvasPoints: IDisposable
    {
        private bool disposed = false;
        private const double MOUSE_WHEEL = 120;
        private readonly double wheelMagnifierRatio = 0.1;
        protected readonly ResourceLoader resourceLoader = ((App)Application.Current).AppResourceLoader;

        private readonly ICanvasResourceCreatorWithDpi renderResourceCreator;
        private readonly ArrayPool<Color> colorArrayPool;
        private Size _sizeCanvas;
        private Point _OriginComplexPlan;
        private double _zoom;
        protected Color[] renderPixels;

        public CanvasPoints(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Point originComplexPlan, double zoom)
        {
            renderResourceCreator = resourceCreator;

            colorArrayPool = ArrayPool<Color>.Shared;

            // Don't use public properties to avoid multiple calculation at construction time
            if (originComplexPlan.X < 0 || originComplexPlan.Y < 0) throw new ArgumentOutOfRangeException(nameof(originComplexPlan), resourceLoader.GetString("ValueNotPositive"));
            _OriginComplexPlan = originComplexPlan;
            if (zoom <= 0) throw new ArgumentOutOfRangeException(nameof(zoom), resourceLoader.GetString("ValueNotStrictlyPositive"));
            _zoom = zoom;
            if (sizeCanvas.Width < 0 || sizeCanvas.Height < 0) throw new ArgumentOutOfRangeException(nameof(sizeCanvas), resourceLoader.GetString("ValueNotPositive"));
            _sizeCanvas = sizeCanvas;

            AllocateRenderTarget();

            Calculate();
        }

        public CanvasRenderTarget RenderTarget { get; private set; }

        // TODO: Move renderPixels array to ArrayPool
        public Size SizeCanvas 
        { 
            get => _sizeCanvas; 
            set
            {
                if (value.Width < 0 || value.Height < 0) throw new ArgumentOutOfRangeException(nameof(SizeCanvas), resourceLoader.GetString("ValueNotPositive"));
                _sizeCanvas = value;

                AllocateRenderTarget();

                Calculate();
            }
        }

        public Point OriginComplexPlan
        {
            get => _OriginComplexPlan;
            set
            {
                if (value.X < 0 || value.Y < 0) throw new ArgumentOutOfRangeException(nameof(OriginComplexPlan), resourceLoader.GetString("ValueNotPositive"));
                _OriginComplexPlan = value;

                Calculate();
            }
        }

        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Zoom), resourceLoader.GetString("ValueNotStrictlyPositive"));
                _zoom = value;

                Calculate();
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

        public double ZoomFromMouseWheelDelta(PointerPointProperties pointerPointProperties) => _zoom * (1 + pointerPointProperties.MouseWheelDelta / MOUSE_WHEEL * wheelMagnifierRatio);

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Calculate()
        {
            Parallel.For(0, PointsCount, Worker);

            RenderTarget.SetPixelColors(renderPixels);
        }
        
        protected abstract void Worker(int i);

        private void AllocateRenderTarget()
        {
            RenderTarget?.Dispose();
            float width = Convert.ToSingle(_sizeCanvas.Width);
            float height = Convert.ToSingle(_sizeCanvas.Height);
            RenderTarget = new CanvasRenderTarget(renderResourceCreator, width, height);

            if (renderPixels != null) colorArrayPool.Return(renderPixels);
            renderPixels = colorArrayPool.Rent(PointsCount);
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
