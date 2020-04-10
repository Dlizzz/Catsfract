using System;
using System.Numerics;
using System.Buffers;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace Catsfract
{
    public interface IComplexSet
    {
        double PointSetWorker(Complex c); 
    }

    public class CanvasPoints: IDisposable
    {
        private static readonly Color renderTransparent = new Color { A = 0, R = 0, G = 0, B = 0 };

        private bool disposed = false;

        private Size _sizeCanvas;
        private Vector2 _origin;
        private double _scale;

        private readonly ICanvasResourceCreatorWithDpi renderResourceCreator;
        private ColorsCollection _colorScale;

        private readonly ArrayPool<Color> colorArrayPool;
        private Color[] renderPixels;

        private readonly ArrayPool<double> doubleArrayPool;
        private double[] renderValues;

        public CanvasPoints(ICanvasResourceCreatorWithDpi resourceCreator, ColorsCollection colorScale, Size sizeCanvas, Vector2 origin, double scale)
        {
            renderResourceCreator = resourceCreator;

            _colorScale = colorScale;

            colorArrayPool = ArrayPool<Color>.Shared;
            doubleArrayPool = ArrayPool<double>.Shared;

            // Don't use public properties to avoid multiple calculation at construction time
            _origin = origin;
            if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale), ((App)App.Current).AppResourceLoader.GetString("ValueNotStrictlyPositive"));
            _scale = scale;
            if (sizeCanvas.Width < 0 || sizeCanvas.Height < 0) throw new ArgumentOutOfRangeException(nameof(sizeCanvas), ((App)App.Current).AppResourceLoader.GetString("ValueNotPositive"));
            _sizeCanvas = sizeCanvas;

            // Don't calculate within the constructor as the Worker thread is not yet implemented
            AllocateRenderTarget();
        }

        public CanvasRenderTarget RenderTarget { get; private set; }

        public ColorsCollection ColorScale 
        { 
            get => _colorScale; 
            set 
            {
                if (value == null) throw new ArgumentNullException(nameof(ColorScale));
                _colorScale = value;
                Render(); 
            }   
        }

        public Size SizeCanvas 
        { 
            get => _sizeCanvas; 
            set
            {
                if (value.Width < 0 || value.Height < 0) throw new ArgumentOutOfRangeException(nameof(SizeCanvas), ((App)App.Current).AppResourceLoader.GetString("ValueNotPositive"));
                _sizeCanvas = value;
                AllocateRenderTarget();
                Calculate();
                Render();
            }
        }

        public Vector2 Origin
        {
            // Origin is also modified by scaling
            get => _origin;
            set
            {
                _origin = value;
                Calculate();
                Render();
            }
        }

        public Vector2 Center => new Vector2(Convert.ToSingle(_sizeCanvas.Width / 2), Convert.ToSingle(_sizeCanvas.Height / 2));

        public double Scale
        {
            get => _scale;
            set
            {
                if (value <= 0) throw new ArgumentOutOfRangeException(nameof(Scale), ((App)App.Current).AppResourceLoader.GetString("ValueNotStrictlyPositive"));

                // Transalte the origin to have the complex at the center of the canevas staying at the center
                _origin.X = Convert.ToSingle((value - _scale) / value * Center.X + _scale / value * _origin.X);
                _origin.Y = Convert.ToSingle((value - _scale) / value * Center.Y + _scale / value * _origin.Y);

                _scale = value;
                Calculate();
                Render();
            }
        }

        public int PointsCount => Convert.ToInt32(_sizeCanvas.Width * _sizeCanvas.Height);

        private void Calculate() => Parallel.For(0, PointsCount, Worker);

        private void Worker(int index)
        {
            Complex c = ToComplex(index);

            renderValues[index] = 
        }

        private void Render()
        {
            Parallel.For(
                0, 
                PointsCount, 
                (index) => 
                    renderPixels[index] = (renderValues[index] == 0) 
                    ? renderTransparent 
                    : ColorScale.ScaleToColorInverse(renderValues[index])
            );
            RenderTarget.SetPixelColors(renderPixels);
        }

        private Complex ToComplex(Vector2 point) => new Complex(_scale * (point.X - _origin.X), -_scale * (point.Y - _origin.Y));

        private Complex ToComplex(int index)
        {
            // Transform bitmap index (per line, left to right) 
            // to bitmap coordinates (x left to right, y top to bottom, origin top left) 
            int lineCount = index / Convert.ToInt32(_sizeCanvas.Width); // Euclidean division
            
            float x = Convert.ToSingle(index - lineCount * _sizeCanvas.Width);
            float y = Convert.ToSingle(lineCount);

            return ToComplex(new Vector2(x, y));
        }

        private void AllocateRenderTarget()
        {
            RenderTarget?.Dispose();
            float width = Convert.ToSingle(_sizeCanvas.Width);
            float height = Convert.ToSingle(_sizeCanvas.Height);
            RenderTarget = new CanvasRenderTarget(renderResourceCreator, width, height);

            if (renderPixels != null) colorArrayPool.Return(renderPixels);
            renderPixels = colorArrayPool.Rent(PointsCount);

            if (renderValues != null) doubleArrayPool.Return(renderValues);
            renderValues = doubleArrayPool.Rent(PointsCount);
        }

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
