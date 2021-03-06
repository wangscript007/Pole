﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pole.EventBus.Abstraction
{
    public interface IObserverUnit<PrimaryKey> : IGrainID
    {
        Func<byte[], Task> GetEventHandler();
    }
}
