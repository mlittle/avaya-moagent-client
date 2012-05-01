namespace AvayaMoagentClient.Commands
{
  public class ReleaseLine : Message
  {
    private const string COMMAND = "AGTReleaseLine";

    public ReleaseLine()
      : this(false)
    { }

    public ReleaseLine(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
