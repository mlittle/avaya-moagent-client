using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AvayaPDSEmulator
{
  class Startup
  {
    static void Main(string[] args)
    {
      var s = new AvayaPdsServer();

      s.StartListening();
    }
  }
}
