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
