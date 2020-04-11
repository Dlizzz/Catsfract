using System;
using System.Numerics;
using System.Buffers;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using CatsnetHelper;

namespace Catsfract
{
    public interface IComplexSet
    {
        double PointSetWorker(Complex c); 
    }

    public class CanvasPoints: IDisposable
    {
        private bool disposed = false;

        private Size _sizeCanvas;
        private Point _origin;
        private double _scale;

        private readonly ICanvasResourceCreatorWithDpi renderResourceCreator;
        private ColorsCollection _colorScale;
        private IComplexSet _pointsSet;

        private readonly ArrayPool<Color> colorArrayPool;
        private Color[] renderPixels;

        private readonly ArrayPool<double> doubleArrayPool;
        private double[] renderValues;

        public CanvasPoints(ICanvasResourceCreatorWithDpi resourceCreator, Size sizeCanvas)
        {
            renderResourceCreator = resourceCreator ?? throw new ArgumentNullException(nameof(resourceCreator));

            colorArrayPool = ArrayPool<Color>.Shared;
            doubleArrayPool = ArrayPool<double>.Shared;

            _sizeCanvas = sizeCanvas;
            _origin = new Point(_sizeCanvas.Width / 2, _sizeCanvas.Height / 2); ;
            _scale = 1;

            // Don't calculate within the constructor as the Worker thread is not yet implemented
            AllocateRenderTarget();
        }

        public CanvasRenderTarget RenderTarget { get; private set; }

        public void SetColorScale(ColorsCollection colorScale)
        {
            _colorScale = colorScale ?? throw new ArgumentNullException(nameof(colorScale));
            Render();
        }

        public void SetPointsSet(IComplexSet pointsSet)
        {
            _pointsSet = pointsSet ?? throw new ArgumentNullException(nameof(pointsSet));
            Calculate();
            Render();
        }

        public Point Origin
        {
            get => _origin;
            set
            {
                _origin = value;
                Calculate();
                Render();
            }
        }

        public Size SizeCanvas 
        { 
            get => _sizeCanvas; 
            set
            {
                _sizeCanvas = value;
                AllocateRenderTarget();
                Calculate();
                Render();
            }
        }

        public double Scale
        {
            get => _scale;
            set
            {
                if (value == 0) throw new ArgumentOutOfRangeException(nameof(Scale), ((App)App.Current).AppResourceLoader.GetString("ValueNotZero"));
                _scale = value;
                Calculate();
                Render();
            }
        }

        public void Zoom(Point center, double newScale)
        {
            // Transalte the origin to have the complex at the center of the canevas staying at the center
            _origin = new Point(
                (newScale - _scale) / newScale * center.X + _scale / newScale * _origin.X,
                (newScale - _scale) / newScale * center.Y + _scale / newScale * _origin.Y);
            _scale = newScale;
            Calculate();
            Render();
        }

        private int PointsCount => Convert.ToInt32(_sizeCanvas.Width * _sizeCanvas.Height);

        private void Calculate()
        {
            if (_pointsSet == null) return;
            Parallel.For(0, PointsCount, Worker);
        }

        private void Worker(int index)
        {
            Complex c = ToComplex(index);

            renderValues[index] = _pointsSet.PointSetWorker(c);
        }

        private void Render()
        {
            if (_colorScale == null)
            {
                Parallel.For(0, PointsCount, (index) => renderPixels[index] = ColorsCollection.Transparent);
            }
            else
            {
                Parallel.For(0, PointsCount, (index) =>
                    renderPixels[index] = (renderValues[index] == 0)
                    ? ColorsCollection.Transparent
                    : _colorScale.ScaleToColorInverse(renderValues[index]));
            }

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
