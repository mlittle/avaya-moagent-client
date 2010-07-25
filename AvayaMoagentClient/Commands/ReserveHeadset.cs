namespace AvayaMoagentClient.Commands
{
  public class ReserveHeadset : Message
  {
    private const string COMMAND = "AGTReserveHeadset";

    public ReserveHeadset(string extension)
      : this(extension, false)
    { }

    public ReserveHeadset(string extension, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, extension)
    { }
  }
}
