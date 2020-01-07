﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Pole.Domain;
using Pole.Domain.UnitOfWork;
using Pole.EntityframeworkCore.MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pole.EntityframeworkCore
{
    public class DbContextBase : DbContext, IUnitOfWork
    {
        private readonly IMediator _mediator;
        public DbContextBase(DbContextOptions options, IMediator mediator) : base(options)
        {
            _mediator = mediator;
        }

        public async Task<CompleteResult> CompeleteAsync(CancellationToken cancellationToken = default)
        {
            var result = CompleteResult.SuccessResult;

            var eventHnadlersResult = await _mediator.DispatchDomainEventsAsync(this);
            if (eventHnadlersResult.Status != 1)
            {
                return new CompleteResult(eventHnadlersResult);
            }
            var dbResult = await base.SaveChangesAsync(cancellationToken);

            return result;
        }
    }
}