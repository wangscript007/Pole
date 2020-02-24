﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pole.Core.Utils.Abstraction
{
    public interface IEventTypeFinder
    {
        Type FindType(string code);
        string GetCode(Type type);
    }
}