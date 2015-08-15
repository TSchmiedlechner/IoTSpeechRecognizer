using System;

namespace IO.Speech
{
  public class SpeechEventArgs : EventArgs
  {
    public string Text { get; set; }

    public SpeechEventArgs(string text)
    {
      Text = text;
    }
  }
}