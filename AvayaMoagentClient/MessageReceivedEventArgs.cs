using System;

namespace AvayaMoagentClient
{
  public class MessageReceivedEventArgs : EventArgs
  {
    public Message Message { get; set; }
  }
}