using System;
using System.Collections.Generic;
using Windows.Globalization;

namespace IO.Speech
{
  public class CommandSpeechListener : IDisposable
  {
    public event EventHandler<CommandEventArgs> CommandReceived;
    public event EventHandler CatchwordReceived;

    private readonly SpeechListener _oneTimeListener, _continuousListener;

    public CommandSpeechListener(Language language, string catchword, IEnumerable<string> commands)
    {
      _continuousListener = new ContinuousSpeechListener(language, catchword);
      _oneTimeListener = new OneTimeSpeechListener(language, commands);

      _continuousListener.SpeechReceived += _continuousListener_SpeechReceived;
      _oneTimeListener.SpeechReceived += _oneTimeListener_SpeechReceived;
    }

    public async void StartAsync()
    {
      await _continuousListener.StartAsync();
    }

    public async void StopAsync()
    {
      await _continuousListener.StopAsync();
    }

    private async void _continuousListener_SpeechReceived(object sender, SpeechEventArgs args)
    {
      var handler = CatchwordReceived;
      handler?.Invoke(sender, null);
      
      await _continuousListener.StopAsync();
      await _oneTimeListener.StartAsync();
    }

    private void _oneTimeListener_SpeechReceived(object sender, SpeechEventArgs args)
    {
      var handler = CommandReceived;
      handler?.Invoke(sender, new CommandEventArgs {Name = args.Text});

      _continuousListener.StartAsync();
    }
    
    public void Dispose()
    {
      _continuousListener.Dispose();
      _oneTimeListener.Dispose();
    }
  }
}