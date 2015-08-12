using System;
using System.Collections.Generic;

namespace IoTSpeechRecognizer.Speech
{
  class CommandSpeechListener : ISpeechListener
  {
    public event EventHandler<SpeechEventArgs> SpeechReceived;

    public IEnumerable<string> Keywords { get; set; }
    public bool IsListening { get; set; }

    public void StartAsync()
    {
      throw new NotImplementedException();
    }

    public void StopAsync()
    {
      throw new NotImplementedException();
    }

    public void Dispose()
    {
      throw new NotImplementedException();
    }
  }
}