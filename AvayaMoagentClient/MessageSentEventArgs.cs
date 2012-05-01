using System;

namespace AvayaMoagentClient
{
  public class MessageSentEventArgs : EventArgs
  {
    public Message Message { get; set; }
  }
}