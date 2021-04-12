﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerGameFramework.Interfaces.Support
{
    public interface IBackgroundThread
    {
        void Setup(IServerApplication server);
        void Run(Object threadContext);
        void Stop();
    }
}
