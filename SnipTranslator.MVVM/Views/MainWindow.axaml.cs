using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using SnipTranslator.Handlers;
using SnipTranslator.Handlers.Keyboard;
using SnipTranslator.MVVM.Extensions;
using SnipTranslator.MVVM.Graphics;
using SnipTranslator.MVVM.Models;
using SnipTranslator.MVVM.Translators;
using SnipTranslator.MVVM.Translators.Google;
using Tesseract;

namespace SnipTranslator.MVVM.Views;

public partial class MainWindow : Window
{
    private ImageContext _img;
    private TesseractEngine _tesseractEngine;
    private Translator Translator;
    private OSKeyboardHandler _osKeyboardHandler;
    private NotifyWindow _notifyWindow = new();
    
    public MainWindow()
    {
        InitializeComponent();
        
        Translator = new Translator(new GoogleTranslator());
        
        _tesseractEngine = new TesseractEngine(@"C:/Program Files/Tesseract-OCR/tessdata", "eng", EngineMode.Default);
        _osKeyboardHandler = new OSKeyboardHandler();
        _osKeyboardHandler.KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object sender, KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case Keys.PrintScreen:
                _img = new ImageContext(BitmapExtension.TakeScreenShot(Screens.Primary), _tesseractEngine);
                OpenImageInFullScreen();
                break;
            case Keys.Escape:
            {
                if (_notifyWindow.IsVisible)
                {
                    _notifyWindow.Hide();
                }

                break;
            }
        }
    }

    private async void ImportImage_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog fileDialog = new OpenFileDialog();
        string[]? files = await fileDialog.ShowAsync(this);
        string? file = files?.FirstOrDefault(IsNotEmpty);
        if (file != null)
        {
            _img = new ImageContext(file, _tesseractEngine);
            TextFromImage.Text = _img.GetText();
        }
    }
    
    private void OpenImageInFullScreen()
    {
        FullScreenImage fullScreenImage = new FullScreenImage(_img);
        fullScreenImage.OnImageCropped += (sender, e) =>
        {
            _img = new ImageContext(e.CroppedBitmap, _tesseractEngine);
            TextFromImage.Text = _img.GetText();
            Translate(e.StartPoint);
        };
        fullScreenImage.Show();
    }

    private async void Translate(Point pos)
    {
        Translator.SetLanguage("ru");
        string? translatedText = await Translator.TranslateAsync(_img.GetText());
        TranslatedText.Text = translatedText;
        _notifyWindow.Show(translatedText, pos);
    }

    private static bool IsNotEmpty(string? str)
    {
        return !string.IsNullOrEmpty(str);
    }
}

