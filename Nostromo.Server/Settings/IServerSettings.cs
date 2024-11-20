﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostromo.Server.Settings
{
    public interface IServerSettings
    {
        string ImagesPath { get; set; }

        ushort ServerPort { get; set; }

        bool FirstRun { get; set; }


    }
}