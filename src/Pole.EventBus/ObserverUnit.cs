﻿using Microsoft.Extensions.Logging;
using Orleans;
using Pole.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Pole.EventBus.Event;
using Orleans.Concurrency;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Pole.EventBus.EventHandler;
using Pole.Core.Utils.Abstraction;

namespace Pole.EventBus
{
    public class ObserverUnit<PrimaryKey> : IObserverUnit<PrimaryKey>
    {
        readonly IServiceProvider serviceProvider;
        readonly ISerializer serializer;
        readonly IEventTypeFinder typeFinder;
        Func<List<byte[]>, Task> batchEventHandler;
        protected ILogger Logger { get; private set; }
        public Type EventHandlerType { get; }

        public ObserverUnit(IServiceProvider serviceProvider, Type eventHandlerType)
        {
            this.serviceProvider = serviceProvider;
            serializer = serviceProvider.GetService<ISerializer>();
            typeFinder = serviceProvider.GetService<IEventTypeFinder>();
            Logger = serviceProvider.GetService<ILogger<ObserverUnit<PrimaryKey>>>();
            EventHandlerType = eventHandlerType;
        }
        public static ObserverUnit<PrimaryKey> From<Grain>(IServiceProvider serviceProvider) where Grain : Orleans.Grain
        {
            return new ObserverUnit<PrimaryKey>(serviceProvider, typeof(Grain));
        }

        public Func<List<byte[]>, Task> GetBatchEventHandler()
        {
            return batchEventHandler;
        }

        public void Observer()
        {
            if (!typeof(IPoleEventHandler).IsAssignableFrom(EventHandlerType))
                throw new NotSupportedException($"{EventHandlerType.FullName} must inheritance from PoleEventHandler");
            batchEventHandler = BatchEventHandler;
            //内部函数
            Task BatchEventHandler(List<byte[]> list)
            {
                var transports = list.Select(bytes =>
                {
                    var (success, transport) = EventBytesTransport.FromBytes(bytes);
                    if (!success)
                    {
                        if (Logger.IsEnabled(LogLevel.Error))
                            Logger.LogError($" EventId:{nameof(EventBytesTransport.EventId)} is not a event");
                    }
                    return (success, transport);
                }).Where(o => o.success)
                  .Select(o => (o.transport))
                  .ToList();
                // 批量处理的时候 grain Id 取第一个 event的id
                using (var scope = serviceProvider.CreateScope())
                {
                    var eventHandlerInstance = scope.ServiceProvider.GetRequiredService(EventHandlerType);
                    var serializer = scope.ServiceProvider.GetRequiredService<ISerializer>() as ISerializer;
                    var eventTypeFinder = scope.ServiceProvider.GetRequiredService<IEventTypeFinder>() as IEventTypeFinder;
                    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>() as ILoggerFactory;
                    var logger = loggerFactory.CreateLogger(EventHandlerType);
                    return GetObserver(EventHandlerType)(eventHandlerInstance, transports, serializer, eventTypeFinder, logger, EventHandlerType);
                }
            }
        }
        static readonly ConcurrentDictionary<Type, Func<object, List<EventBytesTransport>, ISerializer, IEventTypeFinder, ILogger, Type, Task>> _observerGeneratorDict = new ConcurrentDictionary<Type, Func<object, List<EventBytesTransport>, ISerializer, IEventTypeFinder, ILogger, Type, Task>>();
        private Func<object, List<EventBytesTransport>, ISerializer, IEventTypeFinder, ILogger, Type, Task> GetObserver(Type ObserverType)
        {
            var func = _observerGeneratorDict.GetOrAdd(ObserverType, key =>
            {
                var eventHandlerObjParams = Expression.Parameter(typeof(object), "observerType");

                var eventHandlerParams = Expression.Convert(eventHandlerObjParams, ObserverType);
                var eventBytesTransportParams = Expression.Parameter(typeof(List<EventBytesTransport>), "observerType");
                var serializerParams = Expression.Parameter(typeof(ISerializer), "serializer");
                var eventTypeFinderParams = Expression.Parameter(typeof(IEventTypeFinder), "eventTypeFinder");
                var loggerParams = Expression.Parameter(typeof(ILogger), "logger");
                var eventHandlerTypeParams = Expression.Parameter(typeof(Type), "eventHandlerType");
                var method = typeof(IPoleEventHandler).GetMethod("Invoke");
                var body = Expression.Call(eventHandlerParams, method, eventBytesTransportParams, serializerParams, serializerParams, eventTypeFinderParams, loggerParams, eventHandlerTypeParams);
                return Expression.Lambda<Func<object, List<EventBytesTransport>, ISerializer, IEventTypeFinder, ILogger, Type, Task>>(body, eventHandlerObjParams, eventBytesTransportParams, serializerParams, eventTypeFinderParams, loggerParams, eventHandlerTypeParams).Compile();
            });
            return func;
        }
    }
    public static class ClusterClientExtensions
    {
        public static TGrainInterface GetGrain<TGrainInterface>(IClusterClient client, string primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithStringKey
        {
            return client.GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }
    }
}
