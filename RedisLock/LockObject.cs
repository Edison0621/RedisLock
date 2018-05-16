using System;
using StackExchange.Redis;

namespace RedisLock
{
    /// <summary>
    ///     Class LockObject.
    /// </summary>
    public class LockObject
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LockObject" /> class.
        /// </summary>
        /// <param name="redisKey">The redis key.</param>
        /// <param name="redisValue">The redis value.</param>
        /// <param name="createTime">The expire time.</param>
        public LockObject(RedisKey redisKey, RedisValue redisValue, DateTime createTime)
        {
            this.RedisKey = redisKey;
            this.RedisValue = redisValue;
            this.CreateTime = createTime;
        }

        /// <summary>
        ///     Gets the expire time.
        /// </summary>
        /// <value>The expire time.</value>
        public DateTime CreateTime { get; }

        /// <summary>
        ///     Gets the redis key.
        /// </summary>
        /// <value>The redis key.</value>
        public RedisKey RedisKey { get; }

        /// <summary>
        ///     Gets the redis value.
        /// </summary>
        /// <value>The redis value.</value>
        public RedisValue RedisValue { get; }
    }
}