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

using System.Windows;
using AvayaMoagentClient;
using AvayaMoagentClient.Commands;
using AvayaMoagentClient.Enums;
using AvayaTestClient.ViewModels;

namespace AvayaTestClient
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainViewModel Vm { get; set; } 

    public MainWindow()
    {
      InitializeComponent();
      Vm = new MainViewModel();
      DataContext = Vm;
      Vm.UIAction = ((uiAction) => Dispatcher.BeginInvoke(uiAction));
    }

    private void button1_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.Connect();
    }

    private void button2_Click(object sender, RoutedEventArgs e)
    {
      //Vm.Avaya.Login("m9057", "mlitt");
      Vm.Avaya.Login("m9057", "mlitt001");
      //Vm.Avaya.Login("m9999", "mlitt001");
    }

    private void ReserveHeadset_Click(object sender, RoutedEventArgs e)
    {
      //Vm.Avaya.ReserveHeadset("9199");
      Vm.Avaya.ReserveHeadset("56901");
    }

    private void ConnHeadset_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.ConnectHeadset();
    }

    private void ListState_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.ListState();
    }

    private void ListJobs_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.ListJobs();
      //Vm.Avaya.ListActiveJobs();
    }

    private void DisconnectHeadset_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.DisconnectHeadset();
    }

    private void Logoff_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.Logoff();
    }

    private void Disconnect_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.Disconnect();
    }

    private void btnFreeHeadset_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.SendCommand(new Message("AGTFreeHeadset", Message.MessageType.Command));
    }

    private void btnSetWorkClass_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.SetWorkClass(WorkClass.Outbound);
    }

    private void btnAttachJob_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.AttachJob("test34");
    }

    private void btnSetFields_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.SetNotifyKeyField(FieldListType.Outbound, "ACTID");
      Vm.Avaya.SetDataField(FieldListType.Outbound, "ACTID");
      Vm.Avaya.SetDataField(FieldListType.Outbound, "PHONE1");
    }

    private void btnGoAvailable_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.AvailableWork();
    }

    private void btnReadyNextItem_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.ReadyNextItem();
    }

    private void btnFinishedItem_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.FinishedItem("20");
    }

    private void btnNoFurtherWork_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.NoFurtherWork();
    }

    private void btnDetachJob_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.DetachJob();
    }

    private void btnRelease_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.ReleaseLine();
    }

    private void btnSetPassword_Click(object sender, RoutedEventArgs e)
    {
      //Vm.Avaya.SetPassword("m9057", "mlitt001", "Kpk1ig2o");
      //Vm.Avaya.SetPassword("m9057", "Kpk1ig2o", "mlitt001");
      Vm.Avaya.SetPassword("m9999", "temp", "mlitt001");
    }
  }
}
