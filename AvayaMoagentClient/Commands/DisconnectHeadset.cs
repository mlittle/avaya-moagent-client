namespace AvayaMoagentClient.Commands
{
  public class DisconnectHeadset : Message
  {
    private const string COMMAND = "AGTDisconnHeadset";

    public DisconnectHeadset()
      : this(false)
    { }

    public DisconnectHeadset(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
