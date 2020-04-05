using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
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

        private Point originComplex = new Point(400, 400);
        private double zoom = 500;
        private MandelbrotSet mandelbrotSet;
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void CanOnScreen_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            Size size = new Size(sender.ActualWidth, sender.ActualHeight);

            mandelbrotSet?.Dispose();
            mandelbrotSet = new MandelbrotSet(sender, size, originComplex, zoom);
            mandelbrotSet.Calculate();
        }

        private void CanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (sender.ReadyToDraw) args.DrawingSession.DrawImage(mandelbrotSet.RenderTarget);
        }

        private void CanOnScreen_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (mandelbrotSet != null && args.NewSize != mandelbrotSet.SizeCanvas)
            {
                mandelbrotSet.SizeCanvas = args.NewSize;
                mandelbrotSet.Calculate();
            }
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
                mandelbrotSet?.Dispose();
            }

            disposed = true;
        }
    }
}
