namespace AvayaMoagentClient.Commands
{
  public class ListState : Message
  {
    private const string COMMAND = "AGTListState";

    public ListState()
      : this(false)
    { }

    public ListState(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
