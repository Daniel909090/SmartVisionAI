using Microsoft.Maui.Storage;            // FilePicker / MediaPicker
using System.Diagnostics;                // Debug output
using SmartVisionAI.Services;

namespace SmartVisionAI;

public partial class MainPage : ContentPage
{
    private byte[]? _previewBytes;
    private readonly BackendClient _backend = new();

    FileResult? _selectedFile;           // holds the picked photo

    public MainPage()
    {
        InitializeComponent();
    }

    // ---------------------------
    // 1. Upload from gallery /
    //    file system (cross-platform)
    // ---------------------------
    private async void OnUploadClicked(object sender, EventArgs e)
    {
        try
        {
            // Windows → FilePicker | Mobile → MediaPicker
#if WINDOWS
            _selectedFile = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Choose an image",
                FileTypes   = FilePickerFileType.Images
            });
#else
            _selectedFile = await MediaPicker.PickPhotoAsync();
#endif
            if (_selectedFile == null) return;          // user cancelled

            await LoadAndShowImage(_selectedFile);
            ResultLabel.Text = "Ready for analysis";

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // ---------------------------
    // 2. Camera – placeholder
    //    (we will implement later)
    // ---------------------------
    private async void OnCameraClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Info",
            "Camera capture will be added after upload works.",
            "OK");
    }

    // ---------------------------
    // 3. Analyse – placeholder
    //    (Vision API integration later)
    // ---------------------------
    private async void OnAnalyseClicked(object sender, EventArgs e)
    {
        if (_selectedFile == null)
        {
            await DisplayAlert("No image", "Upload or capture an image first.", "OK");
            return;
        }

        try
        {
            await using var stream = new MemoryStream(_previewBytes);
            var (sentence, labels) = await _backend.AnalyseAsync(stream);
            ResultLabel.Text = sentence + Environment.NewLine +
                               "Labels: " + string.Join(", ", labels);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Analysis failed", ex.Message, "OK");
        }
    }

    // ---------------------------
    // Helper: load image into UI
    // ---------------------------
  

    private async Task LoadAndShowImage(FileResult file)
    {
        await using var original = await file.OpenReadAsync();

        // copy once
        using var ms = new MemoryStream();
        await original.CopyToAsync(ms);
        _previewBytes = ms.ToArray();          // keep for preview
        ms.Position = 0;

        // feed the UI from a fresh stream
        SelectedImage.Source = ImageSource.FromStream(() => new MemoryStream(_previewBytes));
    }

}