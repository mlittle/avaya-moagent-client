namespace AvayaMoagentClient.Commands
{
  public class Logoff : Message
  {
    private const string COMMAND = "AGTLogoff";

    public Logoff()
      : this(false)
    { }

    public Logoff(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
