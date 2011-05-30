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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AvayaMoagentClient
{
  // State object for receiving data from remote device.
  public class StateObject
  {
    public Socket WorkSocket { get; set; }
    public const int BufferSize = 256;
    public byte[] buffer = new byte[BufferSize];
    public StringBuilder sb = new StringBuilder();
  }

  public class MoagentClient
  {

    private int _invokeIdSequence = 1;
    private static Message lastMsg;
    private Socket _client;
    private string _server;
    private int _port;

    public delegate void MessageReceivedHandler(object sender, MessageReceivedEventArgs e);

    public event EventHandler ConnectComplete;
    public event MessageReceivedHandler MessageReceived;

    public MoagentClient(string host, int port)
    {
      _server = host;
      _port = port;

      _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    public void StartClient(string host, string port)
    {
      // Connect to a remote device.
      try
      {
        // Connect to the remote endpoint.
        //client.BeginConnect(remoteEp, ConnectCallback, client);
        //connectDone.WaitOne();
        //GetResponse(client);
        //Console.WriteLine("Connected!");
        //Console.ReadKey(true);

        //// Send test data to the remote device.
        //Send(client, new Logon("m9057", "mlitt"));
        //sendDone.WaitOne();
        //GetResponse(client);
        //Console.ReadKey(true);

        //Send(client, new ReserveHeadset("1590"));
        //sendDone.WaitOne();
        //GetResponse(client);
        //Console.ReadKey(true);

        //Send(client, CommandCache.ConnectHeadset);
        //sendDone.WaitOne();
        //GetResponse(client);
        //Console.ReadKey(true);

        //Send(client, CommandCache.ListState);
        //sendDone.WaitOne();
        //GetResponse(client);
        //Console.ReadKey(true);

        //Send(client, CommandCache.ListAllJobs);
        //sendDone.WaitOne();
        //GetResponse(client);
        //Console.ReadKey(true);

        //Send(client, CommandCache.DisconnectHeadset);
        //sendDone.WaitOne();
        //GetResponse(client);
        //Console.ReadKey(true);

        //Send(client, CommandCache.LogOff);
        //sendDone.WaitOne();
        //GetResponse(client);

        //// Release the socket.
        //client.Shutdown(SocketShutdown.Both);
        //client.Close();

      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }

    public void StartConnectAsync()
    {
      var ip = IPAddress.Parse(_server);
      var remoteEp = new IPEndPoint(ip, _port);
      _client.BeginConnect(remoteEp, ConnectCallback, _client);
      //_client.BeginConnect("mlittle.acttoday.com", 22700, ConnectCallback, _client);
    }

    private void ConnectCallback(IAsyncResult ar)
    {
      var client = (Socket)ar.AsyncState;

      client.EndConnect(ar);

      Receive(client);

      if (ConnectComplete != null)
        ConnectComplete(this, EventArgs.Empty);
    }

    public void Disconnect()
    {
      _client.Shutdown(SocketShutdown.Both);
      _client.Close();
    }

    //private void GetResponse(Socket client)
    //{
    //  var done = false;

    //  while (!done)
    //  {
    //    receiveDone.Reset();
    //    Receive(client);
    //    receiveDone.WaitOne();

    //    if (lastMsg != null
    //        && lastMsg.Contents.Count >= 1
    //        && (lastMsg.Contents[0] == "M00000" ||
    //            lastMsg.Contents[0] == "AGENT_STARTUP"
    //            ))
    //      done = true;
    //    if (lastMsg != null
    //        && lastMsg.Contents.Count == 2
    //        && lastMsg.Contents[1].StartsWith("E"))
    //      done = true;
    //  }
    //}

    private void Receive(Socket client)
    {
      try
      {
        var state = new StateObject();
        state.WorkSocket = client;

        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
      var content = String.Empty;

      try
      {
        // Retrieve the state object and the client socket 
        // from the asynchronous state object.
        var state = (StateObject)ar.AsyncState;
        var handler = state.WorkSocket;

        if (handler.Connected)
        {
          // Read data from the remote device.
          int bytesRead = handler.EndReceive(ar);

          if (bytesRead > 0)
          {
            // There  might be more data, so store the data received so far.
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

            // Check for end-of-file tag. If it is not there, read more data.
            content = state.sb.ToString();
            if (content.IndexOf((char)3) > -1)
            {
              var msgs = new List<string>();
              var msg = new StringBuilder();

              foreach (var ch in content)
              {
                if (ch != (char)3)
                  msg.Append(ch);
                else
                {
                  msg.Append(ch);
                  msgs.Add(msg.ToString());
                  msg.Length = 0;
                }
              }

              state.sb.Length = 0;
              if (msg.ToString().IndexOf((char)3) > -1)
                state.sb.Append(msg.ToString());

              _LogMessages(msgs);
            }
          }

          handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }
      }
      catch (Exception e)
      {
        Debugger.Break();
      }
    }

    private void _LogMessages(List<string> msgs)
    {
      foreach (var msg in msgs)
      {
        lastMsg = Message.ParseMessage(msg);
        if (MessageReceived != null)
          MessageReceived(this, new MessageReceivedEventArgs() { Message = lastMsg });
        //Console.WriteLine("Client {0} {1} " + lastMsg.BuildMessage(),
        //    lastMsg.Type == Message.MessageType.Command ? ">" : "<",
        //    DateTime.Now.ToShortTimeString());
      }
    }

    public void Send(Message data)
    {
      data.InvokeId = (_invokeIdSequence++).ToString();
      Send(data.RawMessage);
    }

    private void Send(string data)
    {
      // Convert the string data to byte data using ASCII encoding.
      _LogMessages(new List<string>() { data });

      byte[] byteData = Encoding.ASCII.GetBytes(data);

      // Begin sending the data to the remote device.
      _client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), _client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
      try
      {
        // Retrieve the socket from the state object.
        var client = (Socket)ar.AsyncState;

        //TODO: Tell somebody?
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
