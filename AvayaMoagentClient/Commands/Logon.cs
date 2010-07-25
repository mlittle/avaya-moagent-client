namespace AvayaMoagentClient.Commands
{
  public class Logon : Message
  {
    private const string COMMAND = "AGTLogon";

    public Logon(string username, string password) 
      : this(username, password, false)
    { }

    public Logon(string username, string password, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, username, password)
    { }
  }
}
