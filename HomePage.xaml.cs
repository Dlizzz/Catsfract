using System;
using System.Collections.Generic;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Foundation;
using CatsHelpers.ColorMaps;
using CatsControls;
using Windows.UI.Input;
using System.Globalization;

namespace Catsfract
{
    /// <summary>
    /// Main page of the application, used as the starting point
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ColorMap colorMap;
        private DispatcherTimer menuBarTimer;
        private bool menuBarIsVisible;
        private List<AppBarToggleButton> appBarToggleButtonsColorMap;

        private readonly Dictionary<string, IPointsSet> PointsSets = new Dictionary<string, IPointsSet>
        {
            { "Mandelbrot", new MandelbrotSet(100, 5000) },
            { "Tricorn", new TricornSet(100, 5000) },
            { "Julia 1", new JuliaSet(100, 5000, (-0.4, 0.6)) },
            { "Julia 2", new JuliaSet(100, 5000, (0.3, 0.5)) },
            { "Julia 3", new JuliaSet(100, 5000, (0.285, 0.01)) },
            { "Julia 4", new JuliaSet(100, 5000, (-1.417022285618, 0.0099534)) },
            { "Julia 5", new JuliaSet(100, 5000, (-0.038088, 0.9754633)) },
            { "Julia 6", new JuliaSet(100, 5000, (-1.476, 0)) },
            { "Julia 7", new JuliaSet(100, 5000, (-0.8, 0.156)) }
        };

        public MainPage()
        {
            InitializeComponent();
        }

#pragma warning disable IDE0060, CA1801 // Supprimer le paramètre inutilisé
        private void PointsSet_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            // MenuBar timer
            menuBarTimer = new DispatcherTimer();
            menuBarTimer.Tick += MenuBarTimer_Tick;
            menuBarTimer.Interval = new TimeSpan(0, 0, 3);

            // Default colormap
            colorMap = NamedColorMaps.Turbo;

            // Populate command bar with named color maps
            appBarToggleButtonsColorMap = new List<AppBarToggleButton>();
            foreach (KeyValuePair<string, ColorMap> map in NamedColorMaps.ColorMaps)
            {
                AppBarToggleButton appBarToggleButton = new AppBarToggleButton
                {
                    Label = map.Key,
                    // Check current default colormap
                    IsChecked = (map.Value.Name == colorMap.Name)
                };
                appBarToggleButton.Checked += AppBarToggleButtonColorMap_Checked;
                appBarToggleButtonsColorMap.Add(appBarToggleButton);
                MenuBar.SecondaryCommands.Add(appBarToggleButton);
            }
          
            // Initialization
            menuBarIsVisible = true;
            PointsSet.SetColorMap(colorMap);
            PointsSet.SetWorker((IPointsSet)PointsSetsList.SelectedValue);
            menuBarTimer?.Start();
        }

        private void AppBarToggleButtonColorMap_Checked(object sender, RoutedEventArgs args)
        {
            foreach (AppBarToggleButton button in appBarToggleButtonsColorMap) if ((AppBarToggleButton)sender != button) button.IsChecked = false;

            colorMap = NamedColorMaps.ColorMaps[((AppBarToggleButton)sender).Label];
            PointsSet.SetColorMap(colorMap);
        }

        private void MenuBarTimer_Tick(object sender, object args)
        {
            if (MenuBar.IsOpen || PointsSetsList.IsDropDownOpen || ResolutionSlider.FocusState == FocusState.Pointer) return;

            if (menuBarIsVisible)
            {
                menuBarIsVisible = false;
                HideMenuBarStoryboard.Begin();
            }
        }

        private void PagHome_Unloaded(object sender, RoutedEventArgs args)
        {
            if (menuBarTimer.IsEnabled) menuBarTimer.Stop();
        }

        private void PagHome_PointerEntered(object sender, PointerRoutedEventArgs args)
        {
            if (!menuBarIsVisible)
            {
                menuBarIsVisible = true; 
                ShowMenuBarStoryboard.Begin();
                menuBarTimer?.Start();
            }
        }

        private void PagHome_PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (!menuBarIsVisible)
            {
                if (menuBarTimer.IsEnabled) menuBarTimer.Stop();
                menuBarIsVisible = true;
                ShowMenuBarStoryboard.Begin();
                menuBarTimer?.Start();
            }

            if(BorderValues.Visibility == Visibility.Visible)
            {
                PointerPoint pointerPoint = args.GetCurrentPoint(PointsSet);
                PointValues pointValues =  PointsSet.GetValues(pointerPoint.Position);

                TextValues.Text =
                    "Za: " + pointValues.PointReal.ToString("G", CultureInfo.InvariantCulture) + Environment.NewLine
                    + "Zb: " + pointValues.PointImaginary.ToString("G", CultureInfo.InvariantCulture) + Environment.NewLine
                    + "Value: " + pointValues.PointValue.ToString("G", CultureInfo.InvariantCulture) + Environment.NewLine
                    + "Time: " + pointValues.ExecutionTime.ToString("G", CultureInfo.InvariantCulture);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs args)
        {
            PointsSet.SaveImageFileAsync();
        }

        private void ToggleInversed_Click(object sender, RoutedEventArgs args)
        {
            colorMap.Inversed = !colorMap.Inversed;
        }

        private void ToggleFullScreen_Click(object sender, RoutedEventArgs args)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) view.ExitFullScreenMode();
            else view.TryEnterFullScreenMode();
        }

        private void GridMenuBar_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            DropShadowMask.Width = GridMenuBar.ActualWidth;
            DropShadowMask.Height = GridMenuBar.ActualHeight;
        }

        private void ResolutionSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs args)
        {
            PointsSet.Resolution = args.NewValue;
        }

        private void Reset_Click(object sender, RoutedEventArgs args)
        {
            PointsSet.Origin = new Point(PointsSet.ActualWidth / 2, PointsSet.ActualHeight / 2);
            PointsSet.ScaleFactor = 0.005;
            ResolutionSlider.Value = ResolutionSlider.Minimum;
        }

        private void PointsSetsList_SelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            PointsSet.SetWorker((IPointsSet)PointsSetsList.SelectedValue);
        }

        private void ToggleSatistics_Click(object sender, RoutedEventArgs args)
        {
            AppBarToggleButton button = sender as AppBarToggleButton;

            BorderValues.Visibility = (bool)button.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ToggleMenuParamPanel_Click(object sender, RoutedEventArgs args)
        {
            AppBarToggleButton button = sender as AppBarToggleButton;

            ParamPanel.IsPaneOpen = (bool)button.IsChecked;
        }
#pragma warning restore IDE0060, CA1801 // Supprimer le paramètre inutilisé
    }
}
