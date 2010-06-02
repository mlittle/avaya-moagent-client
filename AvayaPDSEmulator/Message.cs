using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvayaPDSEmulator
{
  public class Message
  {
    private const char _RECORD_SEPERATOR = (char) 30;
    private const char _END_OF_LINE = (char) 3;

    public string Command { get; set; }
    public string Type { get; set; }
    public string OrigId { get; set; }
    public string ProcessId { get; set; }
    public string InvokeId { get; set; }
    public string Segments { get; set; }
    public bool IsError { get; set; }
    public List<string> Contents { get; set; }

    public Message()
    {
      IsError = false;
    }

    public string BuildMessage()
    {
      var ret = new StringBuilder();

      ret.Append(Command.PadRight(20, ' '));
      ret.Append(Type.PadRight(1, ' '));
      ret.Append(OrigId.PadRight(20, ' '));
      ret.Append(ProcessId.PadRight(6, ' '));
      ret.Append(InvokeId.PadRight(4, ' '));
      ret.Append(Segments.PadRight(4, ' '));
      ret.Append(_RECORD_SEPERATOR);
      
      if (IsError)
        ret.Append("1");
      else
        ret.Append("0");

      foreach (var content in Contents)
      {
        ret.Append(_RECORD_SEPERATOR);
        ret.Append(content);  
      }
      
      ret.Append(_END_OF_LINE);

      return ret.ToString();
    }

    public static Message ParseMessage(string raw)
    {
      var ret = new Message();

      //raw = raw.Trim();

      ret.Command = raw.Substring(0, 20);
      ret.Type = raw.Substring(20, 1);
      ret.OrigId = raw.Substring(21, 20);
      ret.ProcessId = raw.Substring(41, 6);
      ret.InvokeId = raw.Substring(47, 4);
      ret.Segments = raw.Substring(51, 4);
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
