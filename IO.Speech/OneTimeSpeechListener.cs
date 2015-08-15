using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace IO.Speech
{
  internal class OneTimeSpeechListener : SpeechListener
  {
    public OneTimeSpeechListener(Language language, IEnumerable<string> keywords) : base(language, keywords) { }

    public override async Task StartAsync()
    {
      if (SpeechRecognizer.State != SpeechRecognizerState.Idle)
        throw new Exception("SpeechListener already started.");

      var speechRecognitionResult = await SpeechRecognizer.RecognizeAsync();
      SpeechRecognizer_ResultGenerated(this, speechRecognitionResult);
    }

    public override async Task StopAsync()
    {
      if (SpeechRecognizer.State == SpeechRecognizerState.Idle)
        throw new Exception("SpeechListener already stopped.");

      await SpeechRecognizer.StopRecognitionAsync();
    }

    private void SpeechRecognizer_ResultGenerated(object sender, SpeechRecognitionResult result)
    {
      if (result.Constraint != null)
        OnSpeechReceived(sender, new SpeechEventArgs(result.Text));
    }
  }
}