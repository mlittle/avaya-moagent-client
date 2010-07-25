namespace AvayaMoagentClient.Commands
{
  public class ConnectHeadset : Message
  {
    private const string COMMAND = "AGTConnHeadset";

    public ConnectHeadset()
        : this(false)
    {}

    public ConnectHeadset(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    {}
  }
}
