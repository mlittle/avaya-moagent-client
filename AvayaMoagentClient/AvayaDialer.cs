using AvayaMoagentClient.Commands;

namespace AvayaMoagentClient
{
  public class AvayaDialer
  {
    private MoagentClient _client;
    private string _host;
    private int _port;

    public event MoagentClient.MessageReceivedHandler MessageReceived;

    public AvayaDialer(string host, int port)
    {
      _host = host;
      _port = port;
    }

    void _client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
      if (MessageReceived != null)
        MessageReceived(this, e);
    }

    void _client_ConnectComplete(object sender, System.EventArgs e)
    {
      //do something?
    }

    public void Connect()
    {
      _client = new MoagentClient(_host, _port);
      _client.ConnectComplete += _client_ConnectComplete;
      _client.MessageReceived += _client_MessageReceived;
      _client.StartConnectAsync();
    }

    public void Login(string username, string password)
    {
     _client.Send(new Logon(username, password)); 
    }

    public void ReserveHeadset(string extension)
    {
      _client.Send(new ReserveHeadset(extension));
    }

    public void ConnectHeadset()
    {
      _client.Send(CommandCache.ConnectHeadset);
    }

    public void ListState()
    {
      _client.Send(CommandCache.ListState);
    }

    public void ListJobs()
    {
      _client.Send(CommandCache.ListAllJobs);
    }

    public void ListActiveJobs()
    {
      _client.Send(new ListJobs(Commands.ListJobs.JobListingType.All, Commands.ListJobs.JobStatus.Active));
    }

    public void DisconnectHeadset()
    {
      _client.Send(CommandCache.DisconnectHeadset);
    }

    public void Logoff()
    {
      _client.Send(CommandCache.LogOff);
    }

    public void Disconnect()
    {
      _client.Disconnect();
      _client.ConnectComplete -= _client_ConnectComplete;
      _client.MessageReceived -= _client_MessageReceived;
      _client = null;
    }

    public void SendCommand(Message command)
    {
      _client.Send(command);
    }
  }
}
