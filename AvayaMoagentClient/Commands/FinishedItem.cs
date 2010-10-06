namespace AvayaMoagentClient.Commands
{
  public class FinishedItem : Message
  {
    private const string COMMAND = "AGTFinishedItem";

    public FinishedItem(string completionCode)
      : this(completionCode, false)
    { }

    public FinishedItem(string completionCode, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, completionCode)
    { }
  }
}
