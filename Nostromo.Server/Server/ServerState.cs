using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using NLog;
using Nostromo.Server.Utilities;

namespace Nostromo.Server.Server
{
    public class ServerState : INotifyPropertyChangedExt
    {
        public static ServerState Instance { get; set; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void NotifyPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        private bool serverOnline = false;

        public bool ServerOnline
        {
            get => serverOnline;
            set => this.SetField(() => serverOnline, value);
        }
    }
}
