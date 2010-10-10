using System;
using AvayaMoagentClient.Commands;
using AvayaMoagentClient.Enums;

namespace AvayaMoagentClient
{
  public class AvayaDialer
  {
    private MoagentClient _client;
    private string _host;
    private int _port;

    public event MoagentClient.MessageSentHandler MessageSent;
    public event MoagentClient.MessageReceivedHandler MessageReceived;

    public AvayaDialer(string host, int port)
    {
      _host = host;
      _port = port;
    }

    private void _client_MessageSent(object sender, MessageSentEventArgs e)
    {
      if (MessageSent != null)
        MessageSent(this, e);
    }

    private void _client_MessageReceived(object sender, MessageReceivedEventArgs e)
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
      _client.MessageSent += _client_MessageSent;
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

    public void AttachJob(string jobname)
    {
      _client.Send(new AttachJob(jobname));
    }

    public void SetWorkClass(WorkClass workClass)
    {
      _client.Send(new SetWorkClass(workClass));
    }

    public void SetNotifyKeyField(FieldListType type, string fieldName)
    {
      _client.Send(new SetNotifyKeyField(type, fieldName)); 
    }

    public void SetDataField(FieldListType type, string fieldName)
    {
      _client.Send(new SetDataField(type, fieldName));
    }

    public void AvailableWork()
    {
      _client.Send(new AvailableWork());
    }

    public void ReadyNextItem()
    {
      _client.Send(new ReadyNextItem());
    }

    public void FinishedItem(string completionCode)
    {
      _client.Send(new FinishedItem(completionCode));
    }

    public void HangupCall()
    {
      _client.Send(new HangupCall());
    }

    public void ReleaseLine()
    {
      _client.Send(new ReleaseLine());
    }

    public void NoFurtherWork()
    {
      _client.Send(new NoFurtherWork());
    }

    public void DetachJob()
    {
      _client.Send(new DetachJob());
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
      _client.MessageSent -= _client_MessageSent;
      _client.ConnectComplete -= _client_ConnectComplete;
      _client.MessageReceived -= _client_MessageReceived;
      _client = null;
    }

    public void FreeHeadset()
    {
      _client.Send(new FreeHeadset());
    }

    public void TransferCall(string transferNumber)
    {
      _client.Send(new TransferCall(transferNumber));
    }

    public void ManagedCall()
    {
      throw new NotImplementedException();
    }

    public void ManualCall()
    {
      throw new NotImplementedException();
    }

    public void DialDigit(string digit)
    {
      throw new NotImplementedException();
    }

    public void SetCallback(string callbackDate, string callbackTime, string phoneIndex, string recallName, 
        string recallNumber)
    {
      throw new NotImplementedException();
    }

    public void SendCommand(Message command)
    {
      _client.Send(command);
    }
  }
}
