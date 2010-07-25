namespace AvayaMoagentClient.Commands
{
  public static class CommandCache
  {
    public static ConnectHeadset ConnectHeadset = new ConnectHeadset(true);
    public static ListJobs ListAllJobs = new ListJobs(ListJobs.JobListingType.All, true);
    public static ListState ListState = new ListState(true);
    public static DisconnectHeadset DisconnectHeadset = new DisconnectHeadset(true);
    public static Logoff LogOff = new Logoff(true);
  }
}
