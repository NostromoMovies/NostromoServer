using System.ComponentModel;

namespace Nostromo.Server.Utilities
{
    public interface INotifyPropertyChangedExt : INotifyPropertyChanged
    {
        void NotifyPropertyChanged(string propname);
    }
}
