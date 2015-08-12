using System;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace IoTSpeechRecognizer.Speech
{
  class ContinuousSpeechListener : ISpeechListener
  {
    private readonly SpeechRecognizer _speechRecognizer;
    private IEnumerable<string> _keywords;

    public event EventHandler<SpeechEventArgs> SpeechReceived;

    public IEnumerable<string> Keywords
    {
      get { return _keywords; }
      set
      {
        _keywords = value;
        Initialize();
      }
    }

    public bool IsListening { get; set; }

    public ContinuousSpeechListener(Language language, IEnumerable<string> keywords)
    {
      _speechRecognizer = new SpeechRecognizer(language);
      _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += OnSpeechRecognizerResultGenerated;

      Keywords = keywords;
    }

    private void OnSpeechRecognizerResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
    {
      if (args.Result.Constraint != null)
      {
        var handler = SpeechReceived;
        handler?.Invoke(sender, new SpeechEventArgs(args.Result.Text));
      }
    }

    private async void Initialize()
    {
      _speechRecognizer.Constraints.Clear();
      _speechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(Keywords));
      var result = await _speechRecognizer.CompileConstraintsAsync();
      if (result.Status != SpeechRecognitionResultStatus.Success)
      {
        throw new Exception("Could not compile constraints.");
      }
    }

    public async void StartAsync()
    {
      if (_speechRecognizer.State == SpeechRecognizerState.Idle)
      {
        await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
        IsListening = true;
      }
      else
      {
        throw new Exception("SpeechListener already started.");
      }
    }

    public async void StopAsync()
    {
      if (_speechRecognizer.State != SpeechRecognizerState.Idle)
      {
        await _speechRecognizer.ContinuousRecognitionSession.StopAsync();
        IsListening = false;
      }
      else
      {
        throw new Exception("SpeechListener already stopped.");
      }
    }

    public void Dispose()
    {
      _speechRecognizer.Dispose();
    }
  }
}