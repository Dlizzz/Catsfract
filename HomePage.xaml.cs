using System;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Catsfract
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDisposable
    {
        private bool disposed = false;

        private readonly CanvasDevice device;
        private CanvasRenderTarget offscreen;
        private Windows.UI.Color[] offscreenPoints;
        private Windows.UI.Color backgroundColor;
        private int maxIterations;
        private Point originComplex = new Point(400, 400);
        private double zoom = 300;
        
        public MainPage()
        {
            this.InitializeComponent();
            device = CanvasDevice.GetSharedDevice();

            maxIterations = 100;

            backgroundColor = Colors.Black;
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(offscreen);
        }

        private void CanOnScreen_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            int pointsCount = Convert.ToInt32(args.NewSize.Width * args.NewSize.Height);

            offscreenPoints = null;
            offscreenPoints = new Windows.UI.Color[pointsCount];
            offscreen?.Dispose();
            offscreen = new CanvasRenderTarget(
                device, 
                Convert.ToSingle(args.NewSize.Width), 
                Convert.ToSingle(args.NewSize.Height), 
                ((CanvasControl)sender).Dpi
            );

            CanvasPoint canvasPoint = new CanvasPoint(args.NewSize, originComplex, zoom);

            for (int pointIndex = 0; pointIndex < pointsCount; pointIndex++)
            {
                Complex c = canvasPoint.ComplexFromIndex(pointIndex);

                offscreenPoints[pointIndex] = backgroundColor;
                if (Diverging(c.Real, c.Imaginary, out int speed)) offscreenPoints[pointIndex] = ColorScale.Viridis[maxIterations - speed].ARGBValue;

            }

            offscreen.SetPixelColors(offscreenPoints);
        }

        private Boolean Diverging(double ca, double cb, out int speed)
        {
            int n = 0;
            double za = 0;
            double zb = 0;

            while (n < maxIterations)
            {
                double zasq = za * za;
                double zbsq = zb * zb;

                zb = 2 * za * zb + cb;
                za = zasq - zbsq + ca;

                if (zasq + zbsq > 4) break;
                n++;
            }

            speed = Convert.ToInt32(((double)maxIterations - (double)n) / (double)maxIterations * 100);

            return (n != maxIterations);
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
                device?.Dispose();
                offscreen?.Dispose();
            }

            disposed = true;
        }

    }
}
