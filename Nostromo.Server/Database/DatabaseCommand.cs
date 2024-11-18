using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Database;
public class DatabaseCommand
{
    public string Command { get; }

    public DatabaseCommand(string command)
    {
        Command = command;
    }
}

