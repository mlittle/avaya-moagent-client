namespace AvayaMoagentClient.Commands
{
  public class ManagedCall : Message
  {
    private const string COMMAND = "AGTManagedCall";

    public ManagedCall()
      : this(false)
    { }

    public ManagedCall(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
