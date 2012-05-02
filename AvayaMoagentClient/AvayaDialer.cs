//Copyright (c) 2010 - 2011, Matthew J Little and contributors.
//All rights reserved.
//
//Redistribution and use in source and binary forms, with or without modification, are permitted
//provided that the following conditions are met:
//
//  Redistributions of source code must retain the above copyright notice, this list of conditions
//  and the following disclaimer.
//
//  Redistributions in binary form must reproduce the above copyright notice, this list of
//  conditions and the following disclaimer in the documentation and/or other materials provided
//  with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR 
//IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
//DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
//WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
//ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
    private bool _useSsl;

    public event MoagentClient.MessageSentHandler MessageSent;
    public event MoagentClient.MessageReceivedHandler MessageReceived;
    public event MoagentClient.DisconnectedHandler Disconnected;

    public AvayaDialer(string host, int port, bool useSsl)
    {
      _host = host;
      _port = port;
      _useSsl = useSsl;
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

    void _client_ConnectComplete(object sender, EventArgs e)
    {
      //do something?
    }

    private void _client_Disconnected(object sender, EventArgs e)
    {
      if (Disconnected != null)
        Disconnected(this, e);
    }

    public bool Connected
    {
      get { return _client.Connected; }
    }

    public void Connect()
    {
      _client = new MoagentClient(_host, _port, _useSsl);
      _client.ConnectComplete += _client_ConnectComplete;
      _client.MessageSent += _client_MessageSent;
      _client.MessageReceived += _client_MessageReceived;
      _client.Disconnected += _client_Disconnected;
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

    public void SetPassword(string userId, string presentPassword, string newPassword)
    {
      _client.Send(new SetPassword(userId, presentPassword, newPassword));
    }

    public void AvailableWork()
    {
      _client.Send(CommandCache.AvailableWork);
    }

    public void ReadyNextItem()
    {
      _client.Send(CommandCache.ReadyNextItem);
    }

    public void FinishedItem(string completionCode)
    {
      _client.Send(new FinishedItem(completionCode));
    }

    public void HangupCall()
    {
      _client.Send(CommandCache.HangupCall);
    }

    public void ReleaseLine()
    {
      _client.Send(CommandCache.ReleaseLine);
    }

    public void NoFurtherWork()
    {
      _client.Send(CommandCache.NoFurtherWork);
    }

    public void DetachJob()
    {
      _client.Send(CommandCache.DetachJob);
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
      _client.Disconnected -= _client_Disconnected;
      _client = null;
    }

    public void FreeHeadset()
    {
      _client.Send(CommandCache.FreeHeadset);
    }

    public void TransferCall()
    {
      _client.Send(CommandCache.TransferCall);
    }

    public void TransferCall(string transferNumber)
    {
      _client.Send(new TransferCall(transferNumber));
    }

    public void ManagedCall()
    {
      _client.Send(CommandCache.ManagedCall);
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
