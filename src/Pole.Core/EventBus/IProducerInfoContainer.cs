﻿using System;
using System.Threading.Tasks;

namespace Pole.Core.EventBus
{
    public interface IProducerInfoContainer
    {
        string GetTargetName(string typeName);
    }
}
