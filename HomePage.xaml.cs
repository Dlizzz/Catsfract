using System.Numerics;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
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

        public MainPage()
        {
            InitializeComponent();
            mandelbrotSet = new MandelbrotSet(100);
            juliaSet = new JuliaSet(100, new Complex(-0.4, 0.6));
        }

#pragma warning disable IDE0060, CA1801 // Supprimer le paramètre inutilisé
        private void PointsSet_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            PointsSet.SetColorScale(NamedColorMaps.Viridis);
            PointsSet.SetWorker(mandelbrotSet);
        }
#pragma warning restore IDE0060, CA1801 // Supprimer le paramètre inutilisé
    }
}
