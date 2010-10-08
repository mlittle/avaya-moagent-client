namespace AvayaMoagentClient.Commands
{
  public class FreeHeadset : Message
  {
    private const string COMMAND = "AGTFreeHeadset";

    public FreeHeadset()
      : this(false)
    { }

    public FreeHeadset(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
