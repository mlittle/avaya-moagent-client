namespace AvayaMoagentClient.Commands
{
  public class DetachJob : Message
  {
    private const string COMMAND = "AGTDetachJob";

    public DetachJob()
      : this(false)
    { }

    public DetachJob(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}

