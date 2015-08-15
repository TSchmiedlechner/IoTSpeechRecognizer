using System;
using System.Collections.Generic;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IO.Speech;

namespace IoTSpeechRecognizer
{
  public sealed partial class MainPage
  {
    private readonly CoreDispatcher _dispatcher;
    private readonly CommandSpeechListener _listener;

    private bool _isListening;

    public MainPage()
    {
      InitializeComponent();
      _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

      _listener = new CommandSpeechListener(SpeechRecognizer.SystemSpeechLanguage, "Hey Kira",
        new List<string>
        {
          "Hintergrund wechseln",
          "Firefox starten",
          "Müll hinausbringen"
        });

      _listener.CommandReceived += _listener_CommandReceived;
      _listener.CatchwordReceived += _listener_CatchwordReceived;
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      _listener.StopAsync();
    }

    private async void _listener_CatchwordReceived(object sender, EventArgs e)
    {
      // TODO extract speech output
      var synthesizer = new SpeechSynthesizer();
      var synthesisStream = await synthesizer.SynthesizeTextToStreamAsync("Ja?");
      
      await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
      {
        media = new MediaElement { AutoPlay = true };
        media.SetSource(synthesisStream, synthesisStream.ContentType);
        media.Play();
      });
    }
    
    private async void _listener_CommandReceived(object sender, CommandEventArgs e)
    {
      await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
      {
        dictationTextBox.Text += $" {e.Name}";
      });
    }

    private void ContinuousRecognize_Click(object sender, RoutedEventArgs e)
    {
      if (!_isListening)
      {
        _isListening = true;
        DictationButtonText.Text = " Stop Dictation";
        discardedTextBlock.Visibility = Visibility.Collapsed;
        _listener.StartAsync();

      }
      else
      {
        _isListening = false;
        DictationButtonText.Text = " Start Dictation";
        _listener.StopAsync();
      }
    }

    private void btnClearText_Click(object sender, RoutedEventArgs e)
    {
      btnClearText.IsEnabled = false;
      dictationTextBox.Text = "";
      discardedTextBlock.Visibility = Visibility.Collapsed;
      
      // Avoid setting focus on the text box, since it's a non-editable control.
      btnContinuousRecognize.Focus(FocusState.Programmatic);
    }

    private void dictationTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      var grid = (Grid)VisualTreeHelper.GetChild(dictationTextBox, 0);
      for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
      {
        object obj = VisualTreeHelper.GetChild(grid, i);
        if (!(obj is ScrollViewer))
        {
          continue;
        }

        ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
        break;
      }
    }
  }
}
