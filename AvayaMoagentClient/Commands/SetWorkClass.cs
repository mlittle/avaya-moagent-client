namespace AvayaMoagentClient.Commands
{
  public class SetWorkClass : Message
  {
    private const string COMMAND = "AGTSetWorkClass";

    public enum WorkClass
    {
      Inbound = 'I',
      Outbound = 'O',
      Blend = 'B',
      PersonToPerson = 'P',
      Managed = 'M'
    }

    public SetWorkClass(WorkClass workClass)
      : this(workClass, false)
    { }

    public SetWorkClass(WorkClass workClass, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, ((char)workClass).ToString())
    { }
  }
}
