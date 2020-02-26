﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pole.Core.Exceptions
{
    public class AddEventToEventBufferException: Exception
    {
        public AddEventToEventBufferException() : base("Errors when add event to the event buffer ")
        {

        }
    }
}
