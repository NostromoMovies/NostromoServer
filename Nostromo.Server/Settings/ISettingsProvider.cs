using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Settings
{
    public interface ISettingsProvider
    {
        IServerSettings GetSettings(bool copy = false);
        void SaveSettings(IServerSettings settings);
        void SaveSettings();
        void DebugSettingsToLog();
    }
}
