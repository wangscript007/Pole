﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Pole.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pole.EntityframeworkCore.MediatR
{
    public static class MediatorExtension
    {
        public static async Task<DomainHandleResult> DispatchDomainEventsAsync(this IMediator mediator, DbContext ctx)
        {
            var result =  DomainHandleResult.SuccessResult;

            var domainEntities = ctx.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.DomainEvents)
                .ToList();

            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            foreach(var domainEvent in domainEvents)
            {
                var currentDomainHandleResult = await mediator.Send(domainEvent);
                if (currentDomainHandleResult.Status != 1)
                {
                    result = currentDomainHandleResult;
                    break;
                }
            }
            return result;
        }
    }
}