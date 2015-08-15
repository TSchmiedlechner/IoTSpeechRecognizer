using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;

namespace IO.Speech
{
  internal abstract class SpeechListener : IDisposable
  {
    private IEnumerable<string> _keywords;

    protected readonly SpeechRecognizer SpeechRecognizer;

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

    protected SpeechListener(Language language, IEnumerable<string> keywords)
    {
      SpeechRecognizer = new SpeechRecognizer(language);
      Keywords = keywords;
    }
    
    protected void OnSpeechReceived(object sender, SpeechEventArgs args)
    {
        var handler = SpeechReceived;
        handler?.Invoke(sender, args);
    }

    public abstract Task StartAsync();
    public abstract Task StopAsync();

    
    private async void Initialize()
    {
      SpeechRecognizer.Constraints.Clear();
      SpeechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(Keywords));

      var result = await SpeechRecognizer.CompileConstraintsAsync();
      if (result.Status != SpeechRecognitionResultStatus.Success)
      {
        throw new Exception("Could not compile constraints.");
      }
    }

    public void Dispose()
    {
      SpeechRecognizer.Dispose();
    }
  }
}