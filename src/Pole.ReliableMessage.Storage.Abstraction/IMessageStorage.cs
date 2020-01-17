using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Pole.ReliableMessage.Storage.Abstraction
{
    public interface IMessageStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<bool> Add(Message message);

        Task<long> Delete(Expression<Func<Message, bool>> filter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageStatus"></param>
        /// <param name="endRetryTime"></param>
        /// <returns></returns>
        Task<List<Message>> GetMany(Expression<Func<Message, bool>> filter, int count);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageStatus"></param>
        /// <param name="endRetryTime"></param>
        /// <returns></returns>
        Task<Message> GetOne(Expression<Func<Message, bool>> filter);

        /// <summary>
        /// 批量更新
        /// 更新这几个值   MessageStatusId ,  RetryTimes  LastRetryUTCTime, NextRetryUTCTime 
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        Task<bool> Save(IEnumerable<Message> messages);

        /// <summary>
        /// 检查 消息的状态,如果不是指定状态则返回true,并且更新状态到指定状态 ,如果已经是指定状态返回false
        /// </summary>
        /// <param name="id"> </param>
        /// <param name="messageStatus"></param>
        /// <returns></returns>
        Task<bool> CheckAndUpdateStatus(Expression<Func<Message, bool>> filter, MessageStatus messageStatus);

        Task<bool> UpdateStatus(Expression<Func<Message, bool>> filter, MessageStatus messageStatus);
    }
}
