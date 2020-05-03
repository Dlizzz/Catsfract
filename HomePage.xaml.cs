using System;
using System.Globalization;
using System.Collections.Generic;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Data;
using Windows.UI.ViewManagement;
using Windows.UI.Input;
using Windows.UI.Core;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.ApplicationModel.Resources;
using CatsHelpers.ColorMaps;
using CatsControls.PointsSet;
using System.Numerics;

namespace Catsfract
{
    public class BooleanToVisibility : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    /// <summary>
    /// Main page of the application, used as the starting point
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        private DispatcherTimer menuBarTimer;
        private bool menuBarIsVisible;
        private bool pointerOverMenuBar = false;
        private CoreCursor currentCursor;
        private readonly CoreCursor waitCursor = new CoreCursor(CoreCursorType.Wait, 0);
        private int menuPanelControlCount = 0;

        // Dictionarry with all the points sets linked to combobox in param pane
        private readonly Dictionary<string, IPointsSetWorker> PointsSets = new Dictionary<string, IPointsSetWorker>
        {
            { "Mandelbrot", new MandelbrotSet(100, 5000) },
            { "Generalized Mandelbrot", new GeneralizedMandelbrotSet(100, 5000) },
            { "Tricorn", new TricornSet(100, 5000) },
            { "Julia", new JuliaSet(100, 5000) }
        };

        public MainPage()
        {
            InitializeComponent();
        }

#pragma warning disable IDE0060, CA1801 // Supprimer le paramètre inutilisé
        private void PointsSet_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs e)
        {
            // MenuBar timer
            menuBarTimer = new DispatcherTimer();
            menuBarTimer.Tick += MenuBarTimer_Tick;
            menuBarTimer.Interval = new TimeSpan(0, 0, 3);

            // Initialization
            menuBarIsVisible = true;
            PointsSet.SetColorMap((ColorMap)ColorMapsList.SelectedValue);
            SetWorker((PointsSetWorker)PointsSetsList.SelectedValue);
            menuBarTimer?.Start();
        }

        private void MenuBarTimer_Tick(object sender, object e)
        {
            if (menuBarIsVisible && !pointerOverMenuBar)
            {
                menuBarIsVisible = false;
                HideMenuBarStoryboard.Begin();
            }
        }

        private void PagHome_Unloaded(object sender, RoutedEventArgs e)
        {
            if (menuBarTimer.IsEnabled) menuBarTimer.Stop();
        }

        private void PagHome_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (!menuBarIsVisible)
            {
                menuBarIsVisible = true;
                ShowMenuBarStoryboard.Begin();
                menuBarTimer?.Start();
            }
        }

        private void PagHome_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!menuBarIsVisible)
            {
                if (menuBarTimer.IsEnabled) menuBarTimer.Stop();
                menuBarIsVisible = true;
                ShowMenuBarStoryboard.Begin();
                menuBarTimer?.Start();
            }

            if (BorderValues.Visibility == Visibility.Visible)
            {
                PointerPoint pointerPoint = e.GetCurrentPoint(PointsSet);
                PointValues pointValues = PointsSet.GetValues(pointerPoint.Position);

                TextValues.Text =
                    "Za: " + pointValues.PointReal.ToString("G", CultureInfo.InvariantCulture) + Environment.NewLine
                    + "Zb: " + pointValues.PointImaginary.ToString("G", CultureInfo.InvariantCulture) + Environment.NewLine
                    + "Value: " + pointValues.PointValue.ToString("G", CultureInfo.InvariantCulture);
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            PointsSet.SaveImageFileAsync();
        }

        private void ToggleInversed_Click(object sender, RoutedEventArgs e)
        {
            var colorMap = (ColorMap)ColorMapsList.SelectedValue;

            colorMap.Inversed = !colorMap.Inversed;
        }

        private void ToggleFullScreen_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode) view.ExitFullScreenMode();
            else view.TryEnterFullScreenMode();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            PointsSet.RenderEnabled = false;
            
            var worker = (PointsSetWorker)PointsSetsList.SelectedValue;
            foreach (Control control in MenuParamPanel.Children)
            {
                if (control is Slider slider && slider.Name != "ResolutionSlider")
                {
                    var (key, part) = ((string, string))slider.Tag;
                    var parameter = worker.Parameters[key];
                    if (parameter is PointsSetComplexParameter paramComplex)
                    {
                        if (part == "a") slider.Value = paramComplex.Default.Real;
                        else if (part == "b") slider.Value = paramComplex.Default.Imaginary;
                    }
                    else if (parameter is PointsSetDoubleParameter paramDouble) slider.Value = paramDouble.Default;
                }
            }
            
            PointsSet.RenderEnabled = true;
            PointsSet.Reset();
        }

        private void PointsSetsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetWorker((PointsSetWorker)PointsSetsList.SelectedValue);
        }

        private void ColorMapsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var colorMap = (ColorMap)ColorMapsList.SelectedValue;

            PointsSet.SetColorMap(colorMap);
        }

        private void MenuBar_Loaded(object sender, RoutedEventArgs e)
        {
            DropShadowMask.Width = MenuBar.ActualWidth;
            DropShadowMask.Height = MenuBar.ActualHeight;
            MenuBar.MinWidth = MenuBar.ActualWidth;
        }

        private void ShadowMenuBar_PointerEntered(object sender, PointerRoutedEventArgs e) => pointerOverMenuBar = true;

        private void ShadowMenuBar_PointerExited(object sender, PointerRoutedEventArgs e) => pointerOverMenuBar = false;

        private void PointsSet_Rendered(object sender, RenderEventArgs e)
        {
            if (TextFPS != null)
            {
                if (e.FramesPerSecond > 1000) TextFPS.Text = resourceLoader.GetString("MaxFPS");
                else TextFPS.Text = e.FramesPerSecond.ToString("F3", CultureInfo.InvariantCulture) + " fps";
            }
            // Restore cursor
            Window.Current.CoreWindow.PointerCursor = currentCursor;
        }

        private void PointsSet_StartRendering(object sender, EventArgs e)
        {
            // Cache the cursor set before pointer enter on button.
            currentCursor = Window.Current.CoreWindow.PointerCursor;
            // Set wait cursor.
            Window.Current.CoreWindow.PointerCursor = waitCursor;
        }

        private void SetWorker(PointsSetWorker worker)
        {
            // Remove previous controls
            for (int i = 0; i < menuPanelControlCount; i++) MenuParamPanel.Children.RemoveAt(MenuParamPanel.Children.Count - 1);

            menuPanelControlCount = 0;
            foreach (KeyValuePair<string, PointsSetParameter> parameter in worker.Parameters)
            {
                if (parameter.Value is PointsSetComplexParameter paramComplex)
                {
                    // Real part slider
                    var sliderReal = CreateSlider((parameter.Key, "a"), paramComplex.Minimum.Real, paramComplex.Maximum.Real, paramComplex.Default.Real);
                    // Imaginary part slider
                    var sliderImaginary = CreateSlider((parameter.Key, "b"), paramComplex.Minimum.Imaginary, paramComplex.Maximum.Imaginary, paramComplex.Default.Imaginary);

                    MenuParamPanel.Children.Add(sliderReal);
                    MenuParamPanel.Children.Add(sliderImaginary);
                    menuPanelControlCount += 2;
                }
                else if (parameter.Value is PointsSetDoubleParameter paramDouble)
                {
                    var slider = CreateSlider((parameter.Key, ""), paramDouble.Minimum, paramDouble.Maximum, paramDouble.Default);

                    MenuParamPanel.Children.Add(slider);
                    menuPanelControlCount += 1;
                }
            }

            PointsSet.SetWorker((IPointsSetWorker)worker);
        }

        private Slider CreateSlider((string, string) tag, double min, double max, double value)
        {
            var slider = new Slider
            {
                Name = "Param" + tag.Item1, 
                Header = tag.Item1 + " " + tag.Item2 + ": " + value.ToString("G", CultureInfo.InvariantCulture),
                Minimum = min,
                Maximum = max,
                StepFrequency = 0.01,
                Value = value,
                Tag = tag
            };

            slider.ValueChanged += ParameterSlider_ValueChanged;

            return slider;
        }

        private void ParameterSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            var slider = (Slider)sender;
            var worker = (PointsSetWorker)PointsSetsList.SelectedValue;
            var (key, part) = ((string, string))slider.Tag;
            var parameter = worker.Parameters[key];

            slider.Header = key + " " + part + ": " + e.NewValue.ToString("G", CultureInfo.InvariantCulture);
            if (parameter is PointsSetComplexParameter paramComplex)
            {
                // Need to modify the full 
                if (part == "a") paramComplex.Real = e.NewValue;
                else if (part == "b") paramComplex.Imaginary = e.NewValue;
            }
            else if (parameter is PointsSetDoubleParameter paramDouble) paramDouble.Value = e.NewValue;
        }
    }
#pragma warning restore IDE0060, CA1801 // Supprimer le paramètre inutilisé
}
