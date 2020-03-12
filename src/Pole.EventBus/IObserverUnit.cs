﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pole.EventBus
{
    public interface IObserverUnit<PrimaryKey> : IGrainID
    {
        Func<List<byte[]>, Task> GetBatchEventHandler();
    }
}
