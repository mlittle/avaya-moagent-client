﻿//Copyright (c) 2010 - 2011, Matthew J Little and contributors.
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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using AvayaMoagentClient;

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
    public bool Disconnect = false;
  }

  public class AvayaPdsServer
  {
    // Thread signal.
    public static ManualResetEvent allDone = new ManualResetEvent(false);
    public static Dictionary<Guid, StateObject> conns = new Dictionary<Guid, StateObject>();

    public AvayaPdsServer()
    {
    }

    public void StartListening()
    {
      // Data buffer for incoming data.
      var bytes = new Byte[1024];
      var localEndPoint = new IPEndPoint(IPAddress.Any, 22700);

      // Create a TCP/IP socket.
      var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
          listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);

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
      var listener = (Socket)ar.AsyncState;
      var handler = listener.EndAccept(ar);

      // Create the state object.
      var state = new StateObject { workSocket = handler };

      conns.Add(state.Id, state);
      var startMsg = new Message
                          {
                            Command = "AGTSTART",
                            Type = Message.MessageType.Notification,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "AGENT_STARTUP" }
                          };
      Send(state, new List<string>() { startMsg.RawMessage });
    }

    public static void ReadCallback(IAsyncResult ar)
    {
      var content = String.Empty;

      try
      {
        // Retrieve the state object and the handler socket
        // from the asynchronous state object.
        var state = (StateObject)ar.AsyncState;
        var handler = state.workSocket;

        // Read data from the client socket. 
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

            Send(state, msgs);
          }
          else
          {
            // Not all data received. Get more.
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
          }
        }
      }
      catch (SocketException e)
      {
        Disconnect((StateObject)ar.AsyncState);
      }
      catch (IOException e)
      {
        //something in the transport leyer has failed, such as the network connection died
        //TODO: log the exception details?
        Disconnect((StateObject)ar.AsyncState);
      }
      catch (ObjectDisposedException e)
      {
        //we've been disconnected
        //TODO: log the exception details?
        Disconnect((StateObject)ar.AsyncState);
      }
      catch (Exception e)
      {
        Debugger.Break();
      }
    }

    private static void Disconnect(StateObject state)
    {
      if (state != null)
      {
        if (conns.ContainsKey(state.Id))
          conns.Remove(state.Id);

        if (state.workSocket != null && state.workSocket.Connected)
        {
          state.workSocket.Close(2);
        }
      }
    }

    private static void Send(StateObject state, List<string> data)
    {
      foreach (var msg in data)
      {
        if (msg.Contains("IPOP"))
        {
          foreach (var conn in conns.Values)
          {
            if (conn.Id != state.Id && (conn.CurrentState == "S70001" || conn.CurrentState == "S70000"))
            {
              _HandleMessage(conn, "IPOP".PadRight(20, ' ').PadRight(55, '0'));
            }
          }
        }
        else if (msg.Contains("POP"))
        {
          foreach (var conn in conns.Values)
          {
            if (conn.Id != state.Id && (conn.CurrentState == "S70001" || conn.CurrentState == "S70000"))
            {
              _HandleMessage(conn, "POP".PadRight(20, ' ').PadRight(55, '0'));
            }
          }
        }
        else if (msg.Contains("MAN"))
        {
          foreach (var conn in conns.Values)
          {
            if (conn.Id != state.Id && (conn.CurrentState == "S70001" || conn.CurrentState == "S70000"))
            {
              _HandleMessage(conn, "MAN".PadRight(20, ' ').PadRight(55, '0'));
            }
          }
        }
        else if (msg.Contains("BTRANS"))
        {
          foreach (var conn in conns.Values)
          {
            if (conn.Id != state.Id && (conn.CurrentState == "S70001" || conn.CurrentState == "S70000"))
            {
              _HandleMessage(conn, "BTRANS".PadRight(20, ' ').PadRight(55, '0'));
            }
          }
        }
        else if (msg.Contains("TRANS"))
        {
          foreach (var conn in conns.Values)
          {
            if (conn.Id != state.Id && (conn.CurrentState == "S70001" || conn.CurrentState == "S70000"))
            {
              _HandleMessage(conn, "TRANS".PadRight(20, ' ').PadRight(55, '0'));
            }
          }
        }
        else
        {
          _HandleMessage(state, msg);
        }
      }

      if (!state.Disconnect)
        state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
      else
      {
        Disconnect(state);
      }
    }

    private static Message CreateMessage(string command, Message.MessageType type, List<string> contents)
    {
      return new Message
                  {
                    Command = command,
                    Type = type,
                    OrigId = "Agent server",
                    ProcessId = "25538",
                    InvokeId = "0",
                    Contents = contents
                  };
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
          //_WriteMessage(handler,
          //    new Message
          //      {
          //        Command = "AGTCallNotify",
          //        Type = Message.MessageType.Notification,
          //        OrigId = "Agent server",
          //        ProcessId = "26621",
          //        InvokeId = "0",
          //        Segments = "5",
          //        Contents =
          //          new List<string>() { "M00001", "Home Phone - 479-273-7762", "OUTBOUND", "CUSTID,71481" }
          //      });
          //_WriteMessage(handler,
          //              new Message
          //                {
          //                  Command = "AGTCallNotify",
          //                  Type = Message.MessageType.Notification,
          //                  OrigId = "Agent server",
          //                  ProcessId = "26621",
          //                  InvokeId = "0",
          //                  Segments = "10", //plus one to the number of fields
          //                  Contents =
          //                    new List<string>()
          //                    {
          //                      "M00001",
          //                      "CUSTID,71481",
          //                      "PHONE1,4792737762",
          //                      "PHONE2,",
          //                      "COAPPSIG,71980",
          //                      "PHONE3,",
          //                      "PHONE4,",
          //                      "PHONE5,",
          //                      "CURPHONE,01"
          //                    }
          //                });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTCallNotify",
                            Type = Message.MessageType.Notification,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents =
  new List<string>() { "M00001", "Home Phone - 479-273-7762", "OUTBOUND", "CUSTID,100" }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTCallNotify",
                            Type = Message.MessageType.Notification,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents =
  new List<string>()
                                {
                                  "M00001",
                                  "CUSTID,100",
                                  "PHONE1,4235555555",
                                  "PHONE2,4235555555",
                                  "COAPPSIG,599",
                                  "Phone3,4235555555",
                                  "Phone4,4235555555",
                                  "Phone5,4235555555",
                                  "CURPHONE,01"
                                }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTCallNotify",
                            Type = Message.MessageType.Notification,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "MAN":
          state.CurrentState = "S70000";
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTPreviewRecord",
                          Type = Message.MessageType.Notification,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents =
new List<string>() { "M00001", "Home Phone - 423-555-5555 (Preview)", "MANAGED", "ACTID,100" }
                        });
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTPreviewRecord",
                          Type = Message.MessageType.Notification,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents =
new List<string>()
                                {
                                  "M00001",
                                  "ACTID,700",
                                  "VKEY,3456789012",
                                  "LOC,149",
                                  "BNAMEF,f",
                                  "BMIDNME,m",
                                  "BNMELST,l",
                                  "ADNUM,",
                                  "ADDNAME,",
                                  "ADCITY,",
                                  "ADST,",
                                  "ADDZIP,",
                                  "PHONE1,4235555555",
                                  "STRATEGY_ID,"
                                }
                        });
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTPreviewRecord",
                          Type = Message.MessageType.Notification,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "M00000" }
                        });

          Thread.Sleep(5000);
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTManagedCall",
                          Type = Message.MessageType.Pending,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "S28833" }
                        });
          Thread.Sleep(7000);
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTManagedCall",
                          Type = Message.MessageType.Data,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "M00001","(CONNECT)" }
                        });
          _WriteMessage(handler,
              new Message
              {
                Command = "AGTManagedCall",
                Type = Message.MessageType.Response,
                OrigId = "Agent server",
                ProcessId = "26621",
                InvokeId = "0",
                Contents = new List<string>() { "M00000" }
              });
          break;
        case "IPOP":
          state.CurrentState = "S70000";
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTCallNotify",
                          Type = Message.MessageType.Notification,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents =
  new List<string>() { "M00001", "INBOUND CALL * 11-20 SECS. WAITING", "INBOUND" }
                        });
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTCallNotify",
                          Type = Message.MessageType.Notification,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "M00000" }
                        });
          break;
        case "TRANS":
          _WriteMessage(handler,
            new Message
            {
              Command = "AGTJobTransLink",
              Type = Message.MessageType.Notification,
              OrigId = "Agent server",
              ProcessId = "26621",
              InvokeId = "0",
              Contents = new List<string>() { "M00001", "GEO_HM2" }
            });
          _WriteMessage(handler,
            new Message
            {
              Command = "AGTJobTransLink",
              Type = Message.MessageType.Notification,
              OrigId = "Agent server",
              ProcessId = "26621",
              InvokeId = "0",
              Contents = new List<string>() { "M00000" }
            });
          break;
        case "BTRANS":
          _WriteMessage(handler,
            new Message
            {
              Command = "AGTJobTransLink",
              Type = Message.MessageType.Notification,
              OrigId = "Agent server",
              ProcessId = "26621",
              InvokeId = "0",
              Contents = new List<string>() { "M00001", "GEO_HM3" }
            });
          _WriteMessage(handler,
            new Message
            {
              Command = "AGTJobTransLink",
              Type = Message.MessageType.Notification,
              OrigId = "Agent server",
              ProcessId = "26621",
              InvokeId = "0",
              Contents = new List<string>() { "M00000" }
            });
          break;
        case "AGTSTART":
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTSTART",
                          Type = Message.MessageType.Notification,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "AGENT_STARTUP" }
                        });
          break;
        case "AGTLogon":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTLogon",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTLogon",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTReserveHeadset":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReserveHeadset",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          Thread.Sleep(500);
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReserveHeadset",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTConnHeadset":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTConnHeadset",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          Thread.Sleep(500);
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTConnHeadset",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
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
                            Type = Message.MessageType.Data,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { content }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTListState",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTSetWorkClass":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSetWorkClass",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTAttachJob":
          state.CurrentState = "S70003";
          state.CurrentJob = m.Contents[0];
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTAttachJob",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTSetNotifyKeyField":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSetNotifyKeyField",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTSetDataField":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTSetDataField",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTAvailWork":
          state.CurrentState = "S70002";
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTAvailWork",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTAvailWork",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTTransferCall":
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTTransferCall",
                          Type = Message.MessageType.Response,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "M00000" }
                        });
          break;
        case "AGTReleaseLine":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReleaseLine",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTReleaseLine",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTReadyNextItem":
          if (state.CurrentState == "S70004")
          {
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTReadyNextItem",
                              Type = Message.MessageType.Response,
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              //IsError = true,
                              Contents = new List<string>() { "E28885" }
                            });
          }
          else
          {
            state.CurrentState = "S70001";
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTReadyNextItem",
                              Type = Message.MessageType.Response,
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Contents = new List<string>() { "M00000" }
                            });
          }
          break;
        case "AGTFinishedItem":
          state.CurrentState = "S70002";
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTFinishedItem",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTFinishedItem",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          if (state.LeaveJob)
          {
            state.CurrentState = "S70003";
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTNoFurtherWork",
                              Type = Message.MessageType.Response,
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Contents = new List<string>() { "M00000" }
                            });
          }
          break;
        case "AGTNoFurtherWork":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTNoFurtherWork",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          if (state.CurrentState != "S70000")
          {
            state.CurrentState = "S70003";
            _WriteMessage(handler,
                          new Message
                            {
                              Command = "AGTNoFurtherWork",
                              Type = Message.MessageType.Response,
                              OrigId = "Agent server",
                              ProcessId = "26621",
                              InvokeId = "0",
                              Contents = new List<string>() { "M00000" }
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

          //Thread.Sleep(3000);

          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTDetachJob",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          state.LeaveJob = false;
          break;
        case "AGTDisconnHeadset":
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTDisconnHeadset",
                            Type = Message.MessageType.Pending,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "S28833" }
                          });
          _WriteMessage(handler,
                        new Message
                          {
                            Command = "AGTDisconnHeadset",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTFreeHeadset":
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTFreeHeadset",
                          Type = Message.MessageType.Response,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "M00000" }
                        });
          break;
        case "AGTListJobs":
          _WriteMessage(handler,
                        new Message()
                        {
                          Command = "AGTListJobs",
                          Type = Message.MessageType.Data,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          //Contents = new List<string>() { "M00001", "O,30DHOP1,I", "O,30DHOP2,I", "O,30HOHiP1,I", "O,30HOHiP2,I", "O,5BIHOP1,I", "O,5BIHOP2,I", "B,ACT_blend,I", "O,ACT_outbnd,I", "O,ALW_C1T3SL,I", "O,ALW_C7S1SL,A", "O,ATTE_C1S1,I", "O,ATTE_C1S2,I", "O,ATTE_C1S3,I", "O,ATTE_C1S5,I", "O,ATTE_C1SP,I", "O,ATTE_C1W1,I", "O,AutoTest,I", "B,BLENDCOPY,I", "B,BlendTst,I", "B,GE_JCALLP5,I", "I,InbClosed,I", "O,Matttest,I", "O,NS_OB,I", "O,SX_MSPP1,I", "O,SX_MSPP2,I", "O,SX_MSPWCP1,I", "O,SX_MSPWCP2,I", "O,SX_Mod1,I", "O,SX_Mod1_2,I", "O,SX_ModSkp,I", "I,SallieINB,I", "B,SallieLO,A", "B,SallieSLM,I", "O,Sallie_AM,I", "B,Sallie_Dev,I", "O,SaxCol2LN1,I", "M,SaxCol2LN2,I", "O,SaxColLPS1,I", "O,SaxColLPS2,I", "M,SaxColLST1,I", "O,SaxColLST2,I", "O,SaxColMSPW,I", "O,SaxColPEP1,I", "O,SaxColPEP2,I", "M,SaxColPR31,I", "M,SaxColPR32,I", "M,SaxColPR61,I", "M,SaxColPR62,I", "O,SaxCol_121,I", "O,SaxCol_122,I", "O,SaxCol_31,I", "O,SaxCol_32,I", "O,SaxCol_61,I", "O,SaxCol_62,I", "O,SaxCol_91,I", "O,SaxCol_92,I", "O,SaxCol_FC1,I", "O,SaxCol_FC2,I", "M,SaxCol_L31,I", "M,SaxCol_L32,I", "O,SaxCol_L61,I", "O,SaxCol_L62,I", "M,SaxDev,I", "M,SaxonArm,I", "M,SaxonEsc,I", "O,SaxonII_1,I", "O,SaxonII_2,I", "O,SxonII_Dev,I", "O,UVRE_C7S1,I", "O,UVRSE_C7S1,I", "O,UVRSW_C7S1,A", "O,UVRW_C7S1,A", "O,Uvrs_Dev,I", "B,blend,I", "I,inbnd1,I", "M,managed,I", "O,outbnd,I" }
                          Contents = new List<string>() { "M00001", "O,GEO_HM1,A", "O,GEO_HM2,A", "O,GEO_HM3,I" }
                          //Contents = new List<string>() { "M00001", "O,SallieLO,A", "O,JOB2,A" }
                        });
          //_WriteMessage(handler,
          //    new Message()
          //    {
          //      Command = "AGTListJobs",
          //      Type = "D",
          //      OrigId = "Agent server",
          //      ProcessId = "26621",
          //      InvokeId = "0",
          //      Segments = "3",
          //      Contents = new List<string>() { "M00001", "O,SallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieLOSallieL,A" }
          //    });
          _WriteMessage(handler,
                        new Message()
                          {
                            Command = "AGTListJobs",
                            Type = Message.MessageType.Response,
                            OrigId = "Agent server",
                            ProcessId = "26621",
                            InvokeId = "0",
                            Contents = new List<string>() { "M00000" }
                          });
          break;
        case "AGTLogoff":
          _WriteMessage(handler,
                        new Message
                        {
                          Command = "AGTLogoff",
                          Type = Message.MessageType.Response,
                          OrigId = "Agent server",
                          ProcessId = "26621",
                          InvokeId = "0",
                          Contents = new List<string>() { "M00000" }
                        });
          state.Disconnect = true;
          break;
      }
    }

    private static void _WriteMessage(Socket sock, Message msg)
    {
      var writer = new StreamWriter(new NetworkStream(sock, true));
      var rawMsg = msg.RawMessage;

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
        var handler = (Socket)ar.AsyncState;

        // Complete sending the data to the remote device.
        var bytesSent = handler.EndSend(ar);
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
