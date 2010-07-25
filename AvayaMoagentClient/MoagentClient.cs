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
    // ManualResetEvent instances signal completion.
    private static ManualResetEvent sendDone = new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = new ManualResetEvent(false);

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
      //var ipHostInfo = Dns.GetHostEntry(_server);
      //var ipAddress = ipHostInfo.AddressList[2];
      var ip = IPAddress.Parse(_server);
      var remoteEp = new IPEndPoint(ip, _port);
      _client.BeginConnect(remoteEp, ConnectCallback, _client);
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

    private void GetResponse(Socket client)
    {
      var done = false;

      while (!done)
      {
        receiveDone.Reset();
        Receive(client);
        receiveDone.WaitOne();

        if (lastMsg != null
            && lastMsg.Contents.Count >= 1
            && (lastMsg.Contents[0] == "M00000" ||
                lastMsg.Contents[0] == "AGENT_STARTUP"
                ))
          done = true;
        if (lastMsg != null
            && lastMsg.Contents.Count == 2
            && lastMsg.Contents[1].StartsWith("E"))
          done = true;
      }
    }

    private static Message CreateMessage(string command, List<string> contents)
    {
      return new Message
               {
                 Command = command,
                 Type = Message.MessageType.Command,
                 OrigId = command == "AGTListJobs" ? "404" : "OrigID",
                 ProcessId = command == "AGTListJobs" ? "607" : "PrID",
                 InvokeId = command == "AGTListJobs" ? "L27" : "InID",
                 Contents = contents
               };
    }

    private void Receive(Socket client)
    {
      try
      {
        // Create the state object.
        var state = new StateObject();
        state.WorkSocket = client;

        // Begin receiving the data from the remote device.
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

            // Check for end-of-file tag. If it is not there, read 
            // more data.
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

        // Complete sending the data to the remote device.
        var bytesSent = client.EndSend(ar);

        // Signal that all bytes have been sent.
        sendDone.Set();
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
