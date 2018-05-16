using System;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using StackExchange.Redis;

namespace RedisLock
{
    public class RedisTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        #region ITransientErrorDetectionStrategy Members

        /// <summary>
        ///     Custom Redis Transient Error Detenction Strategy must have been implemented to satisfy Redis exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public bool IsTransient(Exception ex)
        {
            if (ex == null) return false;

            if (ex is TimeoutException) return true;

            if (ex is RedisServerException) return true;

            if (ex is RedisException) return true;

            return ex.InnerException != null && this.IsTransient(ex.InnerException);
        }

        #endregion ITransientErrorDetectionStrategy Members
    }
}