using AvayaMoagentClient.Commands;

namespace AvayaMoagentClient
{
  public class AvayaDialer
  {
    private MoagentClient _client;

    public event MoagentClient.MessageReceivedHandler MessageReceived;

    public AvayaDialer(string host, int port)
    {
      _client = new MoagentClient(host, port);
      _client.ConnectComplete += _client_ConnectComplete;
      _client.MessageReceived += _client_MessageReceived;
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
    }
  }
}
