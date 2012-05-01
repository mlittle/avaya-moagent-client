namespace AvayaMoagentClient.Commands
{
  public class ReadyNextItem : Message
  {
    private const string COMMAND = "AGTReadyNextItem";

    public ReadyNextItem()
      : this(false)
    { }

    public ReadyNextItem(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
