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
      Avaya = new AvayaDialer("192.168.8.12", 22700);
      Avaya.MessageReceived += _avaya_MessageReceived;
      Messages = new ObservableCollection<Message>();

      UIAction = ((uiAction) => uiAction());
    }

    void _avaya_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
      UIAction(() => Messages.Insert(0,e.Message));
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
