﻿using System;
using System.Threading.Tasks;

namespace Pole.Sagas.Core.Abstraction
{
    public interface IEventSender
    {
        Task SagaStarted(string sagaId, string serviceName, DateTime addTime);
        Task SagaEnded(string sagaId, DateTime ExpiresAt);
        Task ActivityExecuting(string activityId, string sagaId, byte[] parameterData, int order, DateTime addTime);
        Task ActivityRetried(string activityId, string status, int retries, ActivityRetryType retryType);
        Task ActivityExecuteAborted(string activityId);
        Task ActivityCompensateAborted(string activityId, string sagaId, string errors);
        Task ActivityExecuted(string activityId,byte[] resultData);
        Task ActivityCompensated(string activityId);
        Task ActivityExecuteOvertime(string activityId);
        Task ActivityRevoked(string activityId);
        Task ActivityCompensating(string activityId);
    }
}
