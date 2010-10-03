using System.Windows;
using AvayaMoagentClient;
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
      Vm.Avaya.Login("m9057", "mlitt");
    }

    private void ReserveHeadset_Click(object sender, RoutedEventArgs e)
    {
      Vm.Avaya.ReserveHeadset("1590");
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
  }
}
