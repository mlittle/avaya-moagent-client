namespace AvayaMoagentClient.Commands
{
  public class NoFurtherWork : Message
  {
    private const string COMMAND = "AGTNoFurtherWork";

    public NoFurtherWork()
      : this(false)
    { }

    public NoFurtherWork(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}

