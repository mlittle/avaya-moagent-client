namespace AvayaMoagentClient.Commands
{
  public class HangupCall : Message
  {
    private const string COMMAND = "AGTHangupCall";

    public HangupCall()
      : this(false)
    { }

    public HangupCall(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }
  }
}
