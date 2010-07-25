namespace AvayaMoagentClient.Commands
{
  public class ListJobs : Message
  {
    private const string COMMAND = "AGTListJobs";

    public enum JobListingType
    {
      All = 'A',
      Inbound = 'I',
      Blend = 'B',
      Managed = 'M',
      Outbound = 'O'
    }

    public ListJobs(JobListingType type)
      : this(type, false)
    { }

    public ListJobs(JobListingType type, bool cacheRawMessage)
      : base(COMMAND, MessageType.Command, cacheRawMessage, ((char)type).ToString())
    { }
  }
}
