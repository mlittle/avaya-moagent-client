using AvayaMoagentClient.Enums;

namespace AvayaMoagentClient.Commands
{
  public class SetDataField : Message
  {
    private const string COMMAND = "AGTSetDataField";

    public SetDataField(FieldListType type, string fieldName)
      : this(type, fieldName, false)
    { }

    public SetDataField(FieldListType type, string fieldName, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, ((char)type).ToString(), fieldName)
    { }
  }
}
