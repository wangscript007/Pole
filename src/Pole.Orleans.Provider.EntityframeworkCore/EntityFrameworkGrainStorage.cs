﻿using Microsoft.EntityFrameworkCore;
using Orleans;
using Orleans.Runtime;
using Orleans.Storage;
using Pole.Core.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pole.Orleans.Provider.EntityframeworkCore
{
    public class EntityFrameworkGrainStorage<TContext> : IGrainStorage
           where TContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITypeResolver _typeResolver;
        private readonly IEntityTypeResolver _entityTypeResolver;

        private readonly ConcurrentDictionary<string, IGrainStorage> _storage
            = new ConcurrentDictionary<string, IGrainStorage>();

        public EntityFrameworkGrainStorage(
            IServiceProvider serviceProvider,
            ITypeResolver typeResolver,
            IEntityTypeResolver entityTypeResolver)
        {
            _serviceProvider = serviceProvider;
            _entityTypeResolver = entityTypeResolver;
            _typeResolver = typeResolver;
        }

        public Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!typeof(Entity).IsAssignableFrom(grainState.Type)) return Task.CompletedTask;

            if (!_storage.TryGetValue(grainType, out IGrainStorage storage))
                storage = CreateStorage(grainType, grainState);

            return storage.ReadStateAsync(grainType, grainReference, grainState);
        }

        public Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!_storage.TryGetValue(grainType, out IGrainStorage storage))
                storage = CreateStorage(grainType, grainState);

            return storage.WriteStateAsync(grainType, grainReference, grainState);
        }

        public Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
        {
            if (!_storage.TryGetValue(grainType, out IGrainStorage storage))
                storage = CreateStorage(grainType, grainState);

            return storage.ClearStateAsync(grainType, grainReference, grainState);
        }

        private IGrainStorage CreateStorage(
            string grainType
            , IGrainState grainState)
        {
            Type grainImplType = _typeResolver.ResolveType(grainType);
            Type stateType = _entityTypeResolver.ResolveStateType(grainType, grainState);
            Type entityType = _entityTypeResolver.ResolveEntityType(grainType, grainState);

            Type storageType = typeof(GrainStorage<,,,>)
                .MakeGenericType(typeof(TContext),
                    grainImplType, stateType, entityType);

            IGrainStorage storage;

            try
            {
                storage = (IGrainStorage)Activator.CreateInstance(storageType, grainType, _serviceProvider);
            }
            catch (Exception e)
            {
                if (e.InnerException == null)
                    throw;
                // Unwrap target invocation exception

                throw e.InnerException;
            }


            _storage.TryAdd(grainType, storage);
            return storage;
        }
    }
}
