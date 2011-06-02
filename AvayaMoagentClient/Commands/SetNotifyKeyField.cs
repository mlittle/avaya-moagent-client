using AvayaMoagentClient.Enums;

namespace AvayaMoagentClient.Commands
{
  public class SetNotifyKeyField : Message
  {
    private const string COMMAND = "AGTSetNotifyKeyField";

    public SetNotifyKeyField(FieldListType type, string fieldName)
      : this(type, fieldName, false)
    { }

    public SetNotifyKeyField(FieldListType type, string fieldName, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, ((char)type).ToString(), fieldName)
    { }
  }
}
