namespace AvayaMoagentClient.Commands
{
  public class SetNotifyKeyField : Message
  {
    private const string COMMAND = "AGTSetNotifyKeyField";

    public enum ListType
    {
      Inbound = 'I',
      Outbound = 'O'
    }

    public SetNotifyKeyField(ListType type, string fieldName)
      : this(type, fieldName, false)
    { }

    public SetNotifyKeyField(ListType type, string fieldName, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, ((char)type).ToString(), fieldName)
    { }
  }
}
