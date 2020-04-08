using System;
using System.Numerics;
using System.Diagnostics;
using System.Buffers;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;
using Microsoft.Graphics.Canvas;

namespace Catsfract
{
    abstract class CanvasPoints: IDisposable
    {
        private Size _sizeCanvas;
        private Vector3 _origin;
        private float _scale;

        private readonly ICanvasResourceCreatorWithDpi renderResourceCreator;
        private readonly ArrayPool<Color> colorArrayPool;
        private bool disposed = false;
        private Matrix4x4 transformation;

        protected Color[] renderPixels;
        protected readonly ResourceLoader resourceLoader = ((App)Application.Current).AppResourceLoader;

        public CanvasPoints(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas, Vector2 origin, float scale)
        {
            renderResourceCreator = resourceCreator;

            colorArrayPool = ArrayPool<Color>.Shared;

            // Don't use public properties to avoid multiple calculation at construction time
            _origin = new Vector3(origin, 0);
            if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), resourceLoader.GetString("ValueNotStrictlyPositive"));
            _scale = scale;
            if (sizeCanvas.Width < 0 || sizeCanvas.Height < 0) throw new ArgumentOutOfRangeException(nameof(sizeCanvas), resourceLoader.GetString("ValueNotPositive"));
            _sizeCanvas = sizeCanvas;

            UpdateTransformation();

            // Don't calculate within the constructor as the Worker thread is not yet implemented
            AllocateRenderTarget();
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

        public Vector2 Origin
        {
            // Origin is also modified by scaling
            get => new Vector2(_origin.X, _origin.Y);
            set
            {
                _origin = new Vector3(value, 0);

                UpdateTransformation();

                Calculate();
            }
        }

        public Vector2 Center => new Vector2(Convert.ToSingle(_sizeCanvas.Width / 2), Convert.ToSingle(_sizeCanvas.Height / 2));

        public float Scale
        {
            get => _scale;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Scale), resourceLoader.GetString("ValueNotStrictlyPositive"));

                // Transalte the origin to have the complex at the center of the canevas staying at the center
                _origin.X = (value - _scale) / value * Center.X + _scale / value * _origin.X;
                _origin.Y = (value - _scale) / value * Center.Y + _scale / value * _origin.Y;
                _scale = value;

                UpdateTransformation();

                Calculate();
            }
        }

        public int PointsCount => Convert.ToInt32(_sizeCanvas.Width * _sizeCanvas.Height);

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

        protected Complex ToComplex(Vector2 point)
        {
            // Point in complex coordinates
            Vector3 complex = Vector3.Transform(new Vector3(point, 0), transformation);

            return new Complex(complex.X, complex.Y);
        }

        protected Complex ToComplex(int index)
        {
            // Transform bitmap index (per line, left to right) 
            // to bitmap coordinates (x left to right, y top to bottom, origin top left) 
            int y = index / Convert.ToInt32(_sizeCanvas.Width); // Euclidean division
            int x = index - y * Convert.ToInt32(_sizeCanvas.Width);

            // Point in complex coordinates
            Vector3 complex = Vector3.Transform(new Vector3(x, y, 0), transformation);

            return new Complex(complex.X, complex.Y);
        }

        private void UpdateTransformation()
        {
            // Reflection plan (x symetry)
            Plane xz = new Plane(0, 1, 0, 0);

            // Elementary transformation matrixes 
            Matrix4x4 translation = Matrix4x4.CreateTranslation(-_origin);
            Matrix4x4 reflection = Matrix4x4.CreateReflection(xz);
            Matrix4x4 scaling = Matrix4x4.CreateScale(_scale);

            // Global transformation matrix (order matters): translation, then reflection, then scaling 
            transformation = translation * reflection * scaling;
        }

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
