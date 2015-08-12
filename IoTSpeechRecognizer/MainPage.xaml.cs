using System;
using System.Collections.Generic;
using System.Text;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IoTSpeechRecognizer.Speech;

namespace IoTSpeechRecognizer
{
  public sealed partial class MainPage
  {
    private readonly CoreDispatcher _dispatcher;
    private readonly StringBuilder _dictatedTextBuilder;
    private readonly ISpeechListener _speechListener ;

    public MainPage()
    {
      InitializeComponent();
      _dictatedTextBuilder = new StringBuilder();
      _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
      _speechListener = new ContinuousSpeechListener(SpeechRecognizer.SystemSpeechLanguage, new List<string> { "Hey Kira" });
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
      _speechListener.SpeechReceived += _speechListener_SpeechReceived;
    }

    private async void _speechListener_SpeechReceived(object sender, SpeechEventArgs e)
    {
      await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
      {
        dictationTextBox.Text += $", {e.Text}";
      });
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
      _speechListener.StopAsync();
      _speechListener.Dispose();
    }
    
    

    private void ContinuousRecognize_Click(object sender, RoutedEventArgs e)
    {
      if (!_speechListener.IsListening)
      {
          DictationButtonText.Text = " Stop Dictation";
          discardedTextBlock.Visibility = Visibility.Collapsed;
          _speechListener.StartAsync();
        
      }
      else
      {
        DictationButtonText.Text = " Start Dictation";
        _speechListener.StopAsync();
      }
    }

    private void btnClearText_Click(object sender, RoutedEventArgs e)
    {
      btnClearText.IsEnabled = false;
      _dictatedTextBuilder.Clear();
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
