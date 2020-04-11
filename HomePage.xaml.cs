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
using CatsnetHelper;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Catsfract
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDisposable
    {
        private const int MOUSE_WHEEL = 120;
        private readonly double wheelMagnifierRatio = 0.1;
        private CanvasPoints canvasPoints;
        private MandelbrotSet mandelbrotSet;

#pragma warning disable CA1801, IDE0060 // Le paramètre sender de la méthode n'est jamais utilisé
        #region Page        
        public MainPage()
        {
            this.InitializeComponent();
        }
        #endregion

        #region Canvas
        private void Canvas_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            if (args.Reason == CanvasCreateResourcesReason.FirstTime) mandelbrotSet = new MandelbrotSet(100);
            args.TrackAsyncAction(Canvas_CreateResourcesAsync(Canvas).AsAsyncAction());
        }

#pragma warning disable CS1998 // Cette méthode async n'a pas d'opérateur 'await' et elle s'exécutera de façon synchrone
        private async Task Canvas_CreateResourcesAsync(CanvasControl sender)
#pragma warning restore CS1998 // Cette méthode async n'a pas d'opérateur 'await' et elle s'exécutera de façon synchrone
        {
            Size size = Canvas.ActualSize.ToSize();

            canvasPoints?.Dispose();
            canvasPoints = new CanvasPoints(Canvas, size) { Scale = 0.01 };
            canvasPoints.SetColorScale(ColorScales.ColorScale["Viridis"]);
            canvasPoints.SetPointsSet(mandelbrotSet);
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            if (Canvas.ReadyToDraw) args.DrawingSession.DrawImage(canvasPoints.RenderTarget);
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (mandelbrotSet != null && args.NewSize != canvasPoints.SizeCanvas) canvasPoints.SizeCanvas = args.NewSize;
        }

        private void Canvas_DoubleTapped(object sender, DoubleTappedRoutedEventArgs args)
        {
            if (Canvas.ReadyToDraw)
            {
                Point clickedPoint = args.GetPosition(Canvas);
                    
                canvasPoints.Origin = new Point( 
                    canvasPoints.Origin.X + canvasPoints.SizeCanvas.Width / 2 - clickedPoint.X,
                    canvasPoints.Origin.Y + canvasPoints.SizeCanvas.Height / 2 - clickedPoint.Y);

                Canvas.Invalidate();
            }
        }

        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            canvasPoints.Origin = new Point(
                canvasPoints.Origin.X + args.Delta.Translation.X,
                canvasPoints.Origin.Y + args.Delta.Translation.Y);

            Canvas.Invalidate();
        }

        private void Canvas_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            PointerPoint pointerPoint = args.GetCurrentPoint(Canvas);

            int magnifierPower = Math.Abs(pointerPoint.Properties.MouseWheelDelta) / MOUSE_WHEEL;
            double magnifier = pointerPoint.Properties.MouseWheelDelta > 0 ? 1 - wheelMagnifierRatio : 1 + wheelMagnifierRatio;
            for (int i = 2; i <= magnifierPower; i++) magnifier *= magnifier;
            
            canvasPoints.Zoom(pointerPoint.Position, canvasPoints.Scale * magnifier);

            Canvas.Invalidate();
        }
        #endregion
#pragma warning restore CA1801, IDE0060 // Supprimer le paramètre inutilisé

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
                canvasPoints?.Dispose();
            }

            disposed = true;
        }
        #endregion
    }
}
