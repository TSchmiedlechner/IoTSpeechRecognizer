using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace IoTSpeechRecognizer
{
  public sealed partial class MainPage : Page
  {
    private CoreDispatcher _dispatcher;
    private SpeechRecognizer _speechRecognizer;
    private bool _isListening;
    private readonly StringBuilder _dictatedTextBuilder;

    public MainPage()
    {
      InitializeComponent();
      _isListening = false;
      _dictatedTextBuilder = new StringBuilder();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
      _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
      PopulateLanguageDropdown();
      await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
    }

    private void PopulateLanguageDropdown()
    {
      var defaultLanguage = SpeechRecognizer.SystemSpeechLanguage;
      IEnumerable<Language> supportedLanguages = SpeechRecognizer.SupportedTopicLanguages;
      foreach (var lang in supportedLanguages)
      {
        var item = new ComboBoxItem
        {
          Tag = lang,
          Content = lang.DisplayName
        };

        cbLanguageSelection.Items.Add(item);
        if (lang.LanguageTag == defaultLanguage.LanguageTag)
        {
          item.IsSelected = true;
          cbLanguageSelection.SelectedItem = item;
        }
      }
    }

    private async void cbLanguageSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (_speechRecognizer != null)
      {
        ComboBoxItem item = (ComboBoxItem)(cbLanguageSelection.SelectedItem);
        Language newLanguage = (Language)item.Tag;
        if (_speechRecognizer.CurrentLanguage != newLanguage)
        {
          // trigger cleanup and re-initialization of speech.
          try
          {
            await InitializeRecognizer(newLanguage);
          }
          catch (Exception exception)
          {
            var messageDialog = new MessageDialog(exception.Message, "Exception");
            await messageDialog.ShowAsync();
          }
        }
      }
    }

    private async Task InitializeRecognizer(Language recognizerLanguage)
    {
      if (_speechRecognizer != null)
      {
        // cleanup prior to re-initializing this scenario.
        _speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;
        _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
        _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
        _speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

        _speechRecognizer.Dispose();
        _speechRecognizer = null;
      }

      _speechRecognizer = new SpeechRecognizer(recognizerLanguage);

      // Provide feedback to the user about the state of the recognizer. This can be used to provide visual feedback in the form
      // of an audio indicator to help the user understand whether they're being heard.
      _speechRecognizer.StateChanged += SpeechRecognizer_StateChanged;

      // Apply the dictation topic constraint to optimize for dictated freeform speech.
      var dictationConstraint = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "dictation");
      //_speechRecognizer.Constraints.Add(dictationConstraint);
      _speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<string> { "Hey Kira" }));
      SpeechRecognitionCompilationResult result = await _speechRecognizer.CompileConstraintsAsync();
      if (result.Status != SpeechRecognitionResultStatus.Success)
      {
        btnContinuousRecognize.IsEnabled = false;
      }

      // Handle continuous recognition events. Completed fires when various error states occur. ResultGenerated fires when
      // some recognized phrases occur, or the garbage rule is hit. HypothesisGenerated fires during recognition, and
      // allows us to provide incremental feedback based on what the user's currently saying.
      _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
      _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
      _speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
    }

    protected override async void OnNavigatedFrom(NavigationEventArgs e)
    {
      if (_speechRecognizer != null)
      {
        if (_isListening)
        {
          await _speechRecognizer.ContinuousRecognitionSession.CancelAsync();
          _isListening = false;
          DictationButtonText.Text = " Dictate";
          cbLanguageSelection.IsEnabled = true;
        }

        dictationTextBox.Text = "";

        _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
        _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;
        _speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;
        _speechRecognizer.StateChanged -= SpeechRecognizer_StateChanged;

        _speechRecognizer.Dispose();
        _speechRecognizer = null;
      }
    }

    private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender,
      SpeechContinuousRecognitionCompletedEventArgs args)
    {
      if (args.Status != SpeechRecognitionResultStatus.Success)
      {
        // If TimeoutExceeded occurs, the user has been silent for too long. We can use this to 
        // cancel recognition if the user in dictation mode and walks away from their device, etc.
        // In a global-command type scenario, this timeout won't apply automatically.
        // With dictation (no grammar in place) modes, the default timeout is 20 seconds.
        if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
        {
          await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
          {
            DictationButtonText.Text = " Dictate";
            cbLanguageSelection.IsEnabled = true;
            dictationTextBox.Text = _dictatedTextBuilder.ToString();
            _isListening = false;
          });
        }
        else
        {
          await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
          {
            DictationButtonText.Text = " Dictate";
            cbLanguageSelection.IsEnabled = true;
            _isListening = false;
          });
        }
      }
    }

    private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender,
      SpeechRecognitionHypothesisGeneratedEventArgs args)
    {
      string hypothesis = args.Hypothesis.Text;

      // Update the textbox with the currently confirmed text, and the hypothesis combined.
      string textboxContent = _dictatedTextBuilder + " " + hypothesis + " ...";
      await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
      {
        dictationTextBox.Text = textboxContent;
        btnClearText.IsEnabled = true;
      });
    }

    private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender,
      SpeechContinuousRecognitionResultGeneratedEventArgs args)
    {
      // We may choose to discard content that has low confidence, as that could indicate that we're picking up
      // noise via the microphone, or someone could be talking out of earshot.
      if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
          args.Result.Confidence == SpeechRecognitionConfidence.High)
      {
        _dictatedTextBuilder.Append(args.Result.Text + " ");

        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
          discardedTextBlock.Visibility = Visibility.Collapsed;

          dictationTextBox.Text = _dictatedTextBuilder.ToString();
          btnClearText.IsEnabled = true;
        });
      }
      else
      {
        // In some scenarios, a developer may choose to ignore giving the user feedback in this case, if speech
        // is not the primary input mechanism for the application.
        // Here, just remove any hypothesis text by resetting it to the last known good.
        await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        {
          dictationTextBox.Text = _dictatedTextBuilder.ToString();
          string discardedText = args.Result.Text;
          if (!string.IsNullOrEmpty(discardedText))
          {
            discardedText = discardedText.Length <= 25 ? discardedText : (discardedText.Substring(0, 25) + "...");

            discardedTextBlock.Text = "Discarded due to low/rejected Confidence: " + discardedText;
            discardedTextBlock.Visibility = Visibility.Visible;
          }
        });
      }
    }

    private async void SpeechRecognizer_StateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
    {
      await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
      {
      });
    }

    private async void ContinuousRecognize_Click(object sender, RoutedEventArgs e)
    {
      if (_isListening == false)
      {
        // The recognizer can only start listening in a continuous fashion if the recognizer is currently idle.
        // This prevents an exception from occurring.
        if (_speechRecognizer.State == SpeechRecognizerState.Idle)
        {
          DictationButtonText.Text = " Stop Dictation";
          cbLanguageSelection.IsEnabled = false;
          discardedTextBlock.Visibility = Visibility.Collapsed;


          try
          {
            _isListening = true;
            await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
          }
          catch (Exception ex)
          {

            var messageDialog = new MessageDialog(ex.Message, "Exception");
            await messageDialog.ShowAsync();


            _isListening = false;
            DictationButtonText.Text = " Dictate";
            cbLanguageSelection.IsEnabled = true;

          }
        }
      }
      else
      {
        _isListening = false;
        DictationButtonText.Text = " Dictate";
        cbLanguageSelection.IsEnabled = true;

        if (_speechRecognizer.State != SpeechRecognizerState.Idle)
        {
          // Cancelling recognition prevents any currently recognized speech from
          // generating a ResultGenerated event. StopAsync() will allow the final session to 
          // complete.
          await _speechRecognizer.ContinuousRecognitionSession.StopAsync();

          // Ensure we don't leave any hypothesis text behind
          dictationTextBox.Text = _dictatedTextBuilder.ToString();
        }
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
