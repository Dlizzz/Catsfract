using System;
using System.Collections.Generic;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using CatsHelpers.ColorMaps;



// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Catsfract
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MandelbrotSet mandelbrotSet;
        private JuliaSet juliaSet;
        private ColorMap colorMap;
        private DispatcherTimer menuBarTimer;
        private FileSavePicker saveImagePicker;
        private ContentDialog saveFileDialog;

        public MainPage()
        {
            InitializeComponent();

        }

#pragma warning disable IDE0060, CA1801 // Supprimer le paramètre inutilisé
        private void PointsSet_CreateResources(CanvasControl sender, CanvasCreateResourcesEventArgs args)
        {
            // Points sets
            mandelbrotSet = new MandelbrotSet(100);
            juliaSet = new JuliaSet(200, (-0.4, 0.6));

            // Colormap
            colorMap = NamedColorMaps.Turbo;

            // Saved image file picker
            saveImagePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "New Image"
            };
            // Dropdown of file types the user can save the file as
            saveImagePicker.FileTypeChoices.Add("Image Png", new List<string>() { ".png" });
            saveImagePicker.FileTypeChoices.Add("Image Jpeg", new List<string>() { ".jpg", ".jpeg" });
            saveImagePicker.FileTypeChoices.Add("Image Bmp", new List<string>() { ".bmp" });
            saveImagePicker.FileTypeChoices.Add("Image Gif", new List<string>() { ".gif" });
            saveImagePicker.FileTypeChoices.Add("Image Tiff", new List<string>() { ".tif", ".tiff" });
            saveImagePicker.FileTypeChoices.Add("Image Jpeg XR", new List<string>() { ".jxr" });
            // Default file name if the user does not type one in or select a file to replace

            // MenuBar timer
            menuBarTimer = new DispatcherTimer();
            menuBarTimer.Tick += MenuBarTimer_Tick;
            menuBarTimer.Interval = new TimeSpan(0, 0, 3);

            // Image file save error dialog
            saveFileDialog = new ContentDialog { CloseButtonText = "Ok" };

            // Initialization
            PointsSet.SetColorMap(colorMap);
            PointsSet.SetWorker(mandelbrotSet);
            menuBarTimer.Start();
        }

        private void MenuBarTimer_Tick(object sender, object e)
        {
            menuBarTimer.Stop();
            HideMenuBarStoryboard.Begin();
        }

        private void PagHome_Unloaded(object sender, RoutedEventArgs e)
        {
            if (menuBarTimer.IsEnabled) menuBarTimer.Stop();
        }

        private void PagHome_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            MoveAndShowMenuBarStoryboard.Begin();
            menuBarTimer.Start();
        }

        private void PagHome_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            HideMenuBarStoryboard.Begin();
        }

        private void PagHome_PointerMoved(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (menuBarTimer.IsEnabled) menuBarTimer.Stop();
            ShowMenuBarStoryboard.Begin();
            menuBarTimer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SaveImageFile();
        }

        private async void SaveImageFile()
        {
            StorageFile file = await saveImagePicker.PickSaveFileAsync();
            if (file != null)
            {
                var fileFormat = file.FileType switch
                {
                    "Image Png" => CanvasBitmapFileFormat.Png,
                    "Image Jpeg" => CanvasBitmapFileFormat.Jpeg,
                    "Image Bmp" => CanvasBitmapFileFormat.Bmp,
                    "Image Gif" => CanvasBitmapFileFormat.Gif,
                    "Image Tiff" => CanvasBitmapFileFormat.Tiff,
                    "Image Jpeg XR" => CanvasBitmapFileFormat.JpegXR,
                    _ => CanvasBitmapFileFormat.Png
                };
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Pass a stream to the control to stay in the security context of the FilePicker
                    await PointsSet.SaveImageAsync(stream, CanvasBitmapFileFormat.Png).ConfigureAwait(false);
                }
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status == FileUpdateStatus.Complete)
                {
                    // We are not in the UI thread
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                        saveFileDialog.Title = "Save Image Success";
                        saveFileDialog.Content = "Image was saved in " + file.Name;
                        await saveFileDialog.ShowAsync();
                    });
                }
                else
                {
                    // We are not in the UI thread
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => {
                        saveFileDialog.Title = "Save Image Error";
                        saveFileDialog.Content = "Unable to save the image in " + file.Name + " Check destination and filename, and try again.";
                        await saveFileDialog.ShowAsync();
                    });
                }
            }
        }

#pragma warning restore IDE0060, CA1801 // Supprimer le paramètre inutilisé
    }
}
