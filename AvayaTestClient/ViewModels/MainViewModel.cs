using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AvayaMoagentClient;

namespace AvayaTestClient.ViewModels
{
  public class MainViewModel : INotifyPropertyChanged
  {
    public AvayaDialer Avaya { get; protected set; }

    public ObservableCollection<Message> Messages { get; set; }
    public Action<Action> UIAction { get; set; }

    public MainViewModel()
    {
      //Avaya = new AvayaDialer("192.168.8.12", 22700);
      Avaya = new AvayaDialer("192.168.80.79", 22700);
      Avaya.MessageReceived += _avaya_MessageReceived;
      Messages = new ObservableCollection<Message>();

      UIAction = ((uiAction) => uiAction());
    }

    void _avaya_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
      UIAction(() => Messages.Insert(0,e.Message));

      switch (e.Message.Type)
      {
        case Message.MessageType.Command:
          break;
        case Message.MessageType.Pending:
          break;
        case Message.MessageType.Data:
          break;
        case Message.MessageType.Response:
          switch (e.Message.Command.Trim())
          {
            case "AGTLogon":
              Avaya.ReserveHeadset("1");
              break;
            case "AGTReserveHeadset":
              Avaya.ConnectHeadset();
              break;
            case "AGTConnHeadset":
              Avaya.ListState();
              break;
            //case "AGTListState":
            //  Avaya.DisconnectHeadset();
            //  break;
            //case "AGTDisconnHeadset":
            //  Avaya.SendCommand(new Message("AGTFreeHeadset", Message.MessageType.Command));
            //  break;
            //case "AGTFreeHeadset":
            //  Avaya.Logoff();
            //  break;
            //case "AGTLogoff":
            //  Avaya.Disconnect();
            //  break;
           }

          break;
        case Message.MessageType.Busy:
          break;
        case Message.MessageType.Notification:
          switch (e.Message.Command.Trim())
          {
            case "AGTSTART":
              Avaya.Login("m9057","9057tmp");
              break;
          }
          break;
        case Message.MessageType.Undefined:
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
