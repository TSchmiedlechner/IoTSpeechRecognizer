using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace IO.Speech
{
  internal class ContinuousSpeechListener : SpeechListener
  {
    public ContinuousSpeechListener(Language language, string catchword) : base(language, new List<string> {catchword})
    {
      SpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += SpeechRecognizer_ResultGenerated;
    }

    public override async Task StartAsync()
    {
      if (SpeechRecognizer.State != SpeechRecognizerState.Idle)
        throw new Exception("SpeechListener already started.");

      await SpeechRecognizer.ContinuousRecognitionSession.StartAsync();
    }

    public override async Task StopAsync()
    {
      if (SpeechRecognizer.State == SpeechRecognizerState.Idle)
        throw new Exception("SpeechListener already stopped.");

      await SpeechRecognizer.ContinuousRecognitionSession.StopAsync();
    }

    private void SpeechRecognizer_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
    {
      if (args.Result.Constraint != null)
        OnSpeechReceived(sender, new SpeechEventArgs(args.Result.Text));
    }
  }
}