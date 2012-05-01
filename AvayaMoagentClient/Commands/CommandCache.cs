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

namespace AvayaMoagentClient.Commands
{
  public static class CommandCache
  {
    public static AvailableWork AvailableWork = new AvailableWork(true);
    public static ReadyNextItem ReadyNextItem = new ReadyNextItem(true);
    public static HangupCall HangupCall = new HangupCall(true);
    public static ReleaseLine ReleaseLine = new ReleaseLine(true);
    public static NoFurtherWork NoFurtherWork = new NoFurtherWork(true);
    public static DetachJob DetachJob = new DetachJob(true);
    public static FreeHeadset FreeHeadset = new FreeHeadset(true);
    public static ConnectHeadset ConnectHeadset = new ConnectHeadset(true);
    public static TransferCall TransferCall = new TransferCall(true);
    public static ManagedCall ManagedCall = new ManagedCall(true);
    public static ListJobs ListAllJobs = new ListJobs(ListJobs.JobListingType.All, true);
    public static ListState ListState = new ListState(true);
    public static DisconnectHeadset DisconnectHeadset = new DisconnectHeadset(true);
    public static Logoff LogOff = new Logoff(true);
  }
}
