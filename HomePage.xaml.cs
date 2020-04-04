using System;
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
        private Point originComplex = new Point(400, 400);
        private double zoom = 300;
        private MandelbrotSet mandelbrotSet;
        private CanvasPoint canvasPoint;
        
        public MainPage()
        {
            this.InitializeComponent();
            device = CanvasDevice.GetSharedDevice();
        }

        
        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(offscreen);
        }

        private void CanOnScreen_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (canvasPoint == null)
            {
                canvasPoint = new CanvasPoint(args.NewSize, originComplex, zoom);
                mandelbrotSet = new MandelbrotSet(canvasPoint);
            }
            else
            {
                canvasPoint.SizeCanvas = args.NewSize;
                mandelbrotSet.CanvasPoint = canvasPoint;
            }

            offscreen?.Dispose();
            offscreen = new CanvasRenderTarget(
                device, 
                Convert.ToSingle(args.NewSize.Width), 
                Convert.ToSingle(args.NewSize.Height), 
                ((CanvasControl)sender).Dpi
            );

            mandelbrotSet.Calculate();

            offscreen.SetPixelColors(mandelbrotSet.Points);
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
