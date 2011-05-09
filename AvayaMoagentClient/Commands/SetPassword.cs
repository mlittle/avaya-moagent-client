namespace AvayaMoagentClient.Commands
{
  public class SetPassword : Message
  {
    private const string COMMAND = "AGTSetPassword";

    public SetPassword(string userId, string presentPassword, string newPassword)
      : this(userId, presentPassword, newPassword, false)
    { }

    public SetPassword(string userId, string presentPassword, string newPassword, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, userId, presentPassword, newPassword)
    { }
  }
}
