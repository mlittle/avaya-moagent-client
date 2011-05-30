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
using System.Text;

namespace AvayaMoagentClient
{
  public class Message
  {
    private const char _RECORD_SEPERATOR = (char)30;
    private const char _END_OF_LINE = (char)3;

    private string _command;
    private MessageType _type = MessageType.Undefined;
    private string _origId = "OrigID";
    private string _processId = "PrID";
    private string _invokeId = "InID";
    private string _rawMessage;

    public string Command
    {
      get { return _command; }
      set { _command = value; }
    }

    public MessageType Type
    {
      get { return _type; }
      set { _type = value; }
    }

    public string OrigId
    {
      get { return _origId; }
      set { _origId = value; }
    }

    public string ProcessId
    {
      get { return _processId; }
      set { _processId = value; }
    }

    public string InvokeId
    {
      get { return _invokeId; }
      set { _invokeId = value; }
    }

    public string Segments
    {
      get
      {
        var ret = string.Empty;
        switch (Type)
        {
          case MessageType.Command:
            ret = Contents.Count.ToString();
            break;
          case MessageType.Pending:
          case MessageType.Data:
          case MessageType.Response:
          case MessageType.Busy:
          case MessageType.Notification:
            ret = (Contents.Count + 1).ToString();
            break;
          case MessageType.Undefined:
          default:
            throw new ArgumentOutOfRangeException();
            break;
        }

        return ret;
      }
    }

    public string RawMessage
    {
      get
      {
        //string ret;

        //if (_rawMessage == null)
        //  ret = this.BuildMessage();
        //else
        //  ret = _rawMessage;

        //return ret;
        return this.BuildMessage();
      }
      set
      {
        _rawMessage = value;
      }
    }

    public bool IsError { get; set; }
    public List<string> Contents { get; set; }
    public bool CacheRawMessage { get; protected set; }

    public enum MessageType
    {
      Command = 'C',
      Pending = 'P',
      Data = 'D',
      Response = 'R',
      Busy = 'B',
      Notification = 'N',
      Undefined = 'U'
    }

    public Message()
    {
    }

    public Message(string command, MessageType type, params string[] messageContents) :
      this(command, type, false, messageContents)
    { }

    public Message(string command, MessageType type, bool cacheRawMessage, params string[] messageContents)
    {
      Command = command;
      Type = type;
      Contents = new List<string>();
      Contents.AddRange(messageContents);

      if (cacheRawMessage)
      {
        RawMessage = this.BuildMessage();
      }
    }

    private string BuildMessage()
    {
      var msg = new StringBuilder();

      msg.Append(Command.PadRight(20, ' '));
      msg.Append(((char)Type).ToString().PadRight(1, ' '));
      msg.Append(OrigId.PadRight(20, ' '));
      msg.Append(ProcessId.PadRight(6, ' '));
      msg.Append(InvokeId.PadRight(4, ' '));
      msg.Append(Segments.PadRight(4, ' '));

      //Server has an additional flag that indicates if the message is an Error
      //Client does not add this flag
      if (Type != MessageType.Command)
      {
        msg.Append(_RECORD_SEPERATOR);
        msg.Append(IsError ? "1" : "0");
      }

      foreach (var content in Contents)
      {
        msg.Append(_RECORD_SEPERATOR);
        msg.Append(content);
      }

      msg.Append(_END_OF_LINE);

      return msg.ToString();
    }

    public static Message ParseMessage(string raw)
    {
      var ret = new Message();
      ret.RawMessage = raw;

      ret.Command = raw.Substring(0, 20);
      ret.Type = (MessageType)char.Parse(raw.Substring(20, 1));
      ret.OrigId = raw.Substring(21, 20);
      ret.ProcessId = raw.Substring(41, 6);
      ret.InvokeId = raw.Substring(47, 4);
      ret.Contents = new List<string>();

      foreach (var data in raw.Substring(55).Replace(_END_OF_LINE.ToString(), string.Empty).Split(_RECORD_SEPERATOR))
      {
        if (!string.IsNullOrEmpty(data) && data != "0")
          ret.Contents.Add(data);
      }

      return ret;
    }
  }
}
