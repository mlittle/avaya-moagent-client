namespace AvayaMoagentClient.Commands
{
  public static class CommandCache
  {
    public static AvailableWork AvailableWork = new AvailableWork(true);
    public static ReadyNextItem ReadyNextItem = new ReadyNextItem(true);
    public static HangupCall HangupCall = new HangupCall(true);
    public static ReleaseLine ReleaseLine = new ReleaseLine(true);
    public static NoFurtherWork NoFurtherWork = new NoFurtherWork(true);
    public static DetachJob DetachJob = new DetachJob(true);
    public static FreeHeadset FreeHeadset = new FreeHeadset(true);
    public static ConnectHeadset ConnectHeadset = new ConnectHeadset(true);
    public static TransferCall TransferCall = new TransferCall(true);
    public static ManagedCall ManagedCall = new ManagedCall(true);
    public static ListJobs ListAllJobs = new ListJobs(ListJobs.JobListingType.All, true);
    public static ListState ListState = new ListState(true);
    public static DisconnectHeadset DisconnectHeadset = new DisconnectHeadset(true);
    public static Logoff LogOff = new Logoff(true);
  }
}
