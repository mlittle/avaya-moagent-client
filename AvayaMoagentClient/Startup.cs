using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvayaMoagentClient
{
  class Startup
  {
    static void Main(string[] args)
    {
      var s = new MoagentClient();

      s.StartClient();
      Console.ReadKey(true);
    }
  }
}
