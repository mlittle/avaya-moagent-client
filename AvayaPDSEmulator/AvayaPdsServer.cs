using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace AvayaPDSEmulator
{
  // State object for reading client data asynchronously
  public class StateObject
  {
    public Guid Id = Guid.NewGuid();
    public Socket workSocket = null;
    public const int BufferSize = 1024;
    public byte[] buffer = new byte[BufferSize];
    public StringBuilder sb = new StringBuilder();
    public string CurrentState = "S70004";
    public string CurrentJob = string.Empty;
    public bool LeaveJob = false;
  }

  public class AvayaPdsServer
  {
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    public static List<StateObject> conns = new List<StateObject>();

    public AvayaPdsServer()
    {
    }

    public void StartListening()
    {
      // Data buffer for incoming data.
      byte[] bytes = new Byte[1024];

      IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
      IPAddress ipAddress = ipHostInfo.AddressList[0];
      IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 22700);

      // Create a TCP/IP socket.
      Socket listener = new Socket(AddressFamily.InterNetwork,
          SocketType.Stream, ProtocolType.Tcp);

      // Bind the socket to the local endpoint and listen for incoming connections.
      try
      {
        listener.Bind(localEndPoint);
        listener.Listen(100);

        while (true)
        {
          // Set the event to nonsignaled state.
          allDone.Reset();

          // Start an asynchronous socket to listen for connections.
          Console.WriteLine("Waiting for a connection...");
          listener.BeginAccept(
              new AsyncCallback(AcceptCallback),
              listener);

          // Wait until a connection is made before continuing.
          allDone.WaitOne();
        }

      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }

      Console.WriteLine("\nPress ENTER to continue...");
      Console.Read();
    }

    public static void AcceptCallback(IAsyncResult ar)
    {
      // Signal the main thread to continue.
      allDone.Set();

      // Get the socket that handles the client request.
      Socket listener = (Socket)ar.AsyncState;
      Socket handler = listener.EndAccept(ar);

      // Create the state object.
      StateObject state = new StateObject();
      state.workSocket = handler;

      conns.Add(state);
      handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
    }

    public static void ReadCallback(IAsyncResult ar)
    {
      String content = String.Empty;

      // Retrieve the state object and the handler socket
      // from the asynchronous state object.
      StateObject state = (StateObject)ar.AsyncState;
      Socket handler = state.workSocket;

      // Read data from the client socket. 
      int bytesRead = handler.EndReceive(ar);

      if (bytesRead > 0)
      {
        // There  might be more data, so store the data received so far.
        state.sb.Append(Encoding.ASCII.GetString(
            state.buffer, 0, bytesRead));

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
              msg.Clear();
            }
          }
          
          state.sb.Clear();
          if (msg.ToString().IndexOf((char)3) > -1)
            state.sb.Append(msg.ToString());

          Send(state, msgs);
        }
        else
        {
          // Not all data received. Get more.
          handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
          new AsyncCallback(ReadCallback), state);
        }
      }
    }

    private static void Send(StateObject state, List<string> data)
    {
      foreach (var msg in data)
      {
        if (msg.Contains("POP"))
        {
          foreach (var conn in conns)
          {
            if (conn.Id != state.Id && conn.CurrentState == "S70001")
            {
              _HandleMessage(conn, "POP".PadRight(20,' ').PadRight(55,'0'));
            }
          }
        }
        else
        {
          _HandleMessage(state, msg);
        }
      }

      state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
        new AsyncCallback(ReadCallback), state);
    }

    private static void _HandleMessage(StateObject state, string data)
    {
      var handler = state.workSocket;
      Console.WriteLine("Rcvd ({0}):" + data, DateTime.Now.ToString());

      var m = Message.ParseMessage(data);

      switch (m.Command.Trim())
      {
        case "POP":
          state.CurrentState = "S70000";
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTCallNotify",
                            Type = "N",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "5",
                            Contents =
                              new List<string>() {"M00001", "Home Phone - 4235551234", "OUTBOUND", "CUSTID,1234"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTCallNotify",
                            Type = "N",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "9",
                            Contents =
                              new List<string>()
                                {
                                  "M00001",
                                  "CUSTID,1234",
                                  "PHONE1,4235551234",
                                  "PHONE2,8654441234",
                                  "COAPPSIG,345678",
                                  "PHONE3,8885551234",
                                  "PHONE4,",
                                  "CURPHONE,01"
                                }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTCallNotify",
                            Type = "N",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTLogon":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSTART",
                            Type = "N",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"AGENT_STARTUP"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTLogon",
                            Type = "P",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTLogon",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTReserveHeadset":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReserveHeadset",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReserveHeadset",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTConnHeadset":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTConnHeadset",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTConnHeadset",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTListState":
          string content;

          // If on a job, add the job name to the message
          if (state.CurrentState != "S70004")
            content = state.CurrentState + "," + state.CurrentJob;
          else
            content = state.CurrentState;

          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTListState",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {content}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTListState",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTSetWorkClass":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSetWorkClass",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTAttachJob":
          state.CurrentState = "S70003";
          state.CurrentJob = m.Contents[0];
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTAttachJob",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTSetNotifyKeyField":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSetNotifyKeyField",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTSetDataField":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSetDataField",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTAvailWork":
          state.CurrentState = "S70002";
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTAvailWork",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTAvailWork",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTReleaseLine":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReleaseLine",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReleaseLine",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTReadyNextItem":
          if (state.CurrentState == "S70004")
          {
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTReadyNextItem",
                              Type = "R",
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Segments = "2",
                              IsError = true,
                              Contents = new List<string>() {"E28885"}
                            });
          }
          else
          {
            state.CurrentState = "S70001";
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTReadyNextItem",
                              Type = "R",
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Segments = "2",
                              Contents = new List<string>() {"M00000"}
                            });
          }
          break;
        case "AGTFinishedItem":
          state.CurrentState = "S70002";
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTFinishedItem",
                            Type = "P",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTFinishedItem",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          if (state.LeaveJob)
          {
            state.CurrentState = "S70003";
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTNoFurtherWork",
                              Type = "R",
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Segments = "2",
                              Contents = new List<string>() {"M00000"}
                            });
          }
          break;
        case "AGTNoFurtherWork":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTNoFurtherWork",
                            Type = "P",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          if (state.CurrentState != "S70000")
          {
            state.CurrentState = "S70003";
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTNoFurtherWork",
                              Type = "R",
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Segments = "2",
                              Contents = new List<string>() {"M00000"}
                            });
          }
          else
          {
            state.LeaveJob = true;
          }
          break;
        case "AGTDetachJob":
          state.CurrentState = "S70004";
          state.CurrentJob = string.Empty;
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTDetachJob",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          state.LeaveJob = false;
          break;
        case "AGTDisconnHeadset":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTDisconnHeadset",
                            Type = "D",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"S28833"}
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTDisconnHeadset",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTListJobs":
          _WriteMessage(handler,
                        new Message()
                          {
                            Command = "AGTListJobs",
                            Type = "D",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "4",
                            Contents = new List<string>() {"M00001", "O,JOB1,A", "O,JOB2,A"}
                          });
          _WriteMessage(handler,
                        new Message()
                          {
                            Command = "AGTListJobs",
                            Type = "R",
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Segments = "2",
                            Contents = new List<string>() {"M00000"}
                          });
          break;
        case "AGTLogoff":
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTLogoff",
                          Type = "R",
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Segments = "2",
                          Contents = new List<string>() { "M00000" }
                        });
          break;
      } 
    }

    private static void _WriteMessage(Socket sock, Message msg)
    {
      var writer = new StreamWriter(new NetworkStream(sock, true));
      var rawMsg = msg.BuildMessage();

      Console.WriteLine("Sent ({0}):" + rawMsg, DateTime.Now.ToString());
      writer.Write(rawMsg);
      writer.Flush();
      Thread.Sleep(50);
    }

    private static void SendCallback(IAsyncResult ar)
    {
      try
      {
        // Retrieve the socket from the state object.
        Socket handler = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        int bytesSent = handler.EndSend(ar);
        Console.WriteLine("Sent {0} bytes to client.", bytesSent);

        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }
    }
  }
}
