using System;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using CatsHelpers.ColorMaps;


// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Catsfract
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MandelbrotSet mandelbrotSet;
        private readonly JuliaSet juliaSet;
        private ColorMap colorMap;
        private DispatcherTimer dispatcherTimer;

        public MainPage()
        {
            InitializeComponent();
            mandelbrotSet = new MandelbrotSet(100);
            juliaSet = new JuliaSet(200, (-0.4, 0.6));
        }

#pragma warning disable IDE0060, CA1801 // Supprimer le paramètre inutilisé
        private void PointsSet_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            colorMap = NamedColorMaps.Heat;
            PointsSet.SetColorMap(colorMap);
            PointsSet.SetWorker(mandelbrotSet);
            SetTimer();
        }

        private void SetTimer()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(5000000);
            dispatcherTimer.Start();

        }

        private void DispatcherTimer_Tick(object sender, object e)
        {
            colorMap.Inversed = !colorMap.Inversed;
        }

        private void PagHome_Unloaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
        }
#pragma warning restore IDE0060, CA1801 // Supprimer le paramètre inutilisé
    }
}
