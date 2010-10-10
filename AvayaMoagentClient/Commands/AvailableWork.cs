using System;
namespace AvayaMoagentClient.Commands
{
  public class AvailableWork : Message
  {
    private const string COMMAND = "AGTAvailWork";

       public AvailableWork()
      : this(false)
    { }

    public AvailableWork(bool cacheRawMessage)
      : base(COMMAND, Message.MessageType.Command, cacheRawMessage)
    { }
  }
}
