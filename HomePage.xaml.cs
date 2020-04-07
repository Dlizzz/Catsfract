using System;
using System.Diagnostics;
using System.Numerics;
using System.Globalization;
using System.Threading.Tasks;
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
        private const int MOUSE_WHEEL = 120;
        private readonly float wheelMagnifierRatio = 0.1F;
        private readonly float scale = 0.0001F;
        private MandelbrotSet mandelbrotSet;

        #region Page        
        public MainPage()
        {
            this.InitializeComponent();
        }

#pragma warning disable CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        private void PagHome_Unloaded(object sender, RoutedEventArgs e)
#pragma warning restore CS1998 // Cette méthode async n'a pas d'opérateur 'await' et elle s'exécutera de façon synchrone
        {
            Canvas.RemoveFromVisualTree();
            Canvas = null;
        }

        #endregion

        #region Canvas
        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(Canvas_CreateResourcesAsync(sender).AsAsyncAction());
        }

#pragma warning disable CS1998 // Cette méthode async n'a pas d'opérateur 'await' et elle s'exécutera de façon synchrone
        private async Task Canvas_CreateResourcesAsync(CanvasControl sender)
#pragma warning restore CS1998 // Cette méthode async n'a pas d'opérateur 'await' et elle s'exécutera de façon synchrone
        {
            Size size = new Size(sender.ActualWidth, sender.ActualHeight);

            mandelbrotSet?.Dispose();
            Vector2 origin = new Vector2(Convert.ToSingle(size.Width / 2), Convert.ToSingle(size.Height / 2));
            mandelbrotSet = new MandelbrotSet(sender, size, origin, scale);
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (sender.ReadyToDraw) args.DrawingSession.DrawImage(mandelbrotSet.RenderTarget);
        }

#pragma warning disable CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs args)
#pragma warning restore CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        {
            if (mandelbrotSet != null && args.NewSize != mandelbrotSet.SizeCanvas) mandelbrotSet.SizeCanvas = args.NewSize;
        }

        private void Canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            if (((CanvasControl)sender).ReadyToDraw)
            {
                Vector2 clickedPoint = args.GetPosition((CanvasControl)sender).ToVector2();

                Vector2 translation = mandelbrotSet.Center - clickedPoint; 

                mandelbrotSet.Origin += translation;

                ((CanvasControl)sender).Invalidate();
            }
        }

#pragma warning disable CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
#pragma warning restore CA1801 // Le paramètre sender de la méthode n'est jamais utilisé
        {
            //Debug.WriteLine(args.Delta.ToString());
        }

        private void Canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint pointerPoint = args.GetCurrentPoint((CanvasControl)sender);

            int magnifierPower = Math.Abs(pointerPoint.Properties.MouseWheelDelta) / MOUSE_WHEEL;
            float magnifier = pointerPoint.Properties.MouseWheelDelta > 0 ? 1 - wheelMagnifierRatio : 1 + wheelMagnifierRatio;
            mandelbrotSet.Scale *= Convert.ToSingle(Math.Pow(magnifier, magnifierPower));

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
