namespace AvayaMoagentClient.Commands
{
  public class TransferCall : Message
  {
    private const string COMMAND = "AGTTransferCall";

    public TransferCall()
        : this(false)
    { }
    
    public TransferCall(string phoneNumber)
      : this(phoneNumber, false)
    { }

    public TransferCall(bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage)
    { }

    public TransferCall(string phoneNumber, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, phoneNumber)
    { }
  }
}
