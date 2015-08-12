using System;
using System.Collections.Generic;

namespace IoTSpeechRecognizer.Speech
{
  public interface ISpeechListener : IDisposable
  {
    event EventHandler<SpeechEventArgs> SpeechReceived;
    
    IEnumerable<string> Keywords { get; set; }
    bool IsListening { get; set; }

    void StartAsync();
    void StopAsync();
  }
}