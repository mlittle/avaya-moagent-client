using System;
namespace AvayaMoagentClient.Commands
{
  public class AttachJob : Message
  {
    private const string COMMAND = "AGTAttachJob";

    public AttachJob(string jobname)
      : this(jobname, false)
    { }

    public AttachJob(string jobname, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, jobname)
    { }
  }
}
