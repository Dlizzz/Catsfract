using System;
using System.Diagnostics;
using System.Globalization;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Catsfract
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDisposable
    {
        private Point originComplexPlan = new Point(400, 400);
        private double zoom = 500;
        private MandelbrotSet mandelbrotSet;

        #region Page        
        public MainPage()
        {
            this.InitializeComponent();
        }
        #endregion

        #region CanOnScreen
        private void CanOnScreen_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            Size size = new Size(sender.ActualWidth, sender.ActualHeight);

            if (args.Reason != CanvasCreateResourcesReason.FirstTime) mandelbrotSet.Dispose();

            mandelbrotSet = new MandelbrotSet(sender, size, originComplexPlan, zoom);
        }

        private void CanOnScreen_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (sender.ReadyToDraw) args.DrawingSession.DrawImage(mandelbrotSet.RenderTarget);
        }

#pragma warning disable CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        private void CanOnScreen_SizeChanged(object sender, SizeChangedEventArgs args)
#pragma warning restore CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        {
            if (mandelbrotSet != null && args.NewSize != mandelbrotSet.SizeCanvas) mandelbrotSet.SizeCanvas = args.NewSize;
        }

        private void CanOnScreen_DoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            if (((CanvasControl)sender).ReadyToDraw)
            {
                mandelbrotSet.OriginComplexPlan = args.GetPosition((CanvasControl)sender);
                ((CanvasControl)sender).Invalidate();
            }
        }

#pragma warning disable CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        private void CanOnScreen_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
#pragma warning restore CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        {
            Debug.WriteLine(args.Delta.ToString());
        }

        private void CanOnScreen_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint pointerPoint = args.GetCurrentPoint((CanvasControl)sender);

            mandelbrotSet.Zoom = mandelbrotSet.ZoomFromMouseWheelDelta(pointerPoint.Properties);

            ((CanvasControl)sender).Invalidate();
        }

        #endregion

        #region Dispose
        private bool disposed = false;

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
        #endregion

    }
}
