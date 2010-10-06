namespace AvayaMoagentClient.Commands
{
  public class SetDataField : Message
  {
    private const string COMMAND = "AGTSetDataField";

    public enum ListType
    {
      Inbound = 'I',
      Outbound = 'O'
    }

    public SetDataField(ListType type, string fieldName)
      : this(type, fieldName, false)
    { }

    public SetDataField(ListType type, string fieldName, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, ((char)type).ToString(), fieldName)
    { }
  }
}
