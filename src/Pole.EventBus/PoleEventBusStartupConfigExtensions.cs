﻿using Microsoft.Extensions.DependencyInjection;
using Pole.Core;
using Pole.Core.Processor;
using Pole.EventBus.Processor;
using Pole.EventBus.Processor.Server;
using Pole.EventBus.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pole.EventBus
{
    public static class PoleEventBusStartupConfigExtensions
    {
        public static void AddEventBus(
           this StartupConfig startupOption)
        {
            startupOption.Services.AddSingleton<IEventBuffer, EventBuffer>();
            startupOption.Services.AddScoped<IBus, Bus>();
            startupOption.Services.AddSingleton<IObserverUnitContainer, ObserverUnitContainer>();
            startupOption.Services.AddSingleton<IProcessor, PendingMessageRetryProcessor>();
            startupOption.Services.AddSingleton<IProcessor, ExpiredEventsCollectorProcessor>();
            startupOption.Services.AddHostedService<BackgroundServiceBasedProcessorServer>();
            startupOption.Services.AddScoped<IUnitOfWork, Pole.EventBus.UnitOfWork.UnitOfWork>();
            startupOption.Services.AddSingleton<IEventTypeFinder, EventTypeFinder>();

            Startup.Register(async serviceProvider =>
            {

            });
        }
    }
}