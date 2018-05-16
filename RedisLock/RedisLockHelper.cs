using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using StackExchange.Redis;

namespace RedisLock
{
    public class RedisLockHelper
    {
        /// <summary>
        ///     The unlock script
        /// </summary>
        private static readonly string UnlockScript = @"
            if redis.call(""get"",KEYS[1]) == ARGV[1] then
                return redis.call(""del"",KEYS[1])
            else
                return 0
            end";

        private readonly IDatabase database;
        private readonly int maxRetryCount;
        private readonly int retryEveryMilliseconds;
        private readonly RetryPolicy retryPolicy;

        /// <summary>
        ///     构建redis锁对象,
        /// </summary>
        /// <param name="database">Redis Database实例</param>
        /// <param name="maxRetryCount">最大重试次数</param>
        /// <param name="retryEveryMilliseconds">每次获取锁的时间间隔(毫秒)</param>
        public RedisLockHelper(IDatabase database, int maxRetryCount = 6, int retryEveryMilliseconds = 500)
        {
            this.database = database;
            this.maxRetryCount = maxRetryCount;
            this.retryEveryMilliseconds = retryEveryMilliseconds;

            FixedInterval fixedInterval = new FixedInterval(maxRetryCount, TimeSpan.FromMilliseconds(retryEveryMilliseconds));
            this.retryPolicy = new RetryPolicy(new RedisTransientErrorDetectionStrategy(), fixedInterval);
        }

        /// <summary>
        ///     防并发执行
        /// </summary>
        /// <param name="func">需要防并发执行的方法</param>
        /// <param name="redisKey">Redis键</param>
        /// <param name="expireTime">过期时间</param>
        /// <returns>Task.</returns>
        public async Task AvoidConcurrency(Func<LockObject> func, RedisKey redisKey, TimeSpan expireTime)
        {
            if ((await this.LockAsync(redisKey, expireTime)).Item1)
            {
                func();
            }
        }

        /// <summary>
        ///     锁操作
        /// </summary>
        /// <param name="redisKey">Redis键</param>
        /// <param name="expireTime">过期时间，增强对超时操作的可操作性</param>
        /// <returns>System.Threading.Tasks.Task&lt;System.Tuple&lt;System.Boolean, Jinyinmao.OperativeCenter.Lib.RedisLock.LockObject&gt;&gt;.</returns>
        public async Task<Tuple<bool, LockObject>> LockAsync(RedisKey redisKey, TimeSpan expireTime)
        {
            int i = 0;
            while (true)
            {
                LockObject lockObject = null;
                try
                {
                    lockObject = new LockObject(redisKey, DateTime.Now.ToString(CultureInfo.InvariantCulture), DateTime.UtcNow);

                    if (!await this.SetLockAsync(redisKey, lockObject, expireTime))
                    {
                        await Task.Delay(this.retryEveryMilliseconds);

                        if (await this.IfLockDeadAsync(redisKey, expireTime))
                        {
                            await this.ReleaseLockAsync(redisKey);
                        }

                        if (i > this.maxRetryCount)
                        {
                            await this.ReleaseLockAsync(redisKey);
                            return Tuple.Create(true, lockObject);
                        }

                        i++;
                        continue;
                    }
                }
                catch (Exception)
                {
                    await this.ReleaseLockAsync(redisKey);

                    return Tuple.Create(false, lockObject);
                }

                return Tuple.Create(true, lockObject);
            }
        }

        public async Task<bool> RetryAsync(Func<Task<bool>> action)
        {
            return await this.retryPolicy.ExecuteAction(action);
        }

        private async Task<bool> IfLockDeadAsync(string key, TimeSpan expireTime)
        {
            RedisValue redisValue = await this.database.StringGetAsync(key);
            if (!redisValue.HasValue)
            {
                return false;
            }

            LockObject lockObject = redisValue.ToString().FromJson<LockObject>();
            return lockObject.CreateTime.Add(-expireTime) > DateTime.UtcNow;
        }

        private async Task ReleaseLockAsync(string key)
        {
            await this.database.ScriptEvaluateAsync(UnlockScript, new RedisKey[] { key });
        }

        private async Task<bool> SetLockAsync(string key, LockObject redisValue, TimeSpan expireTime)
        {
            bool succeeded;
            try
            {
                succeeded = await this.database.StringSetAsync(key, redisValue.ToJson(), expireTime, When.NotExists);
            }
            catch (Exception)
            {
                succeeded = false;
            }

            return succeeded;
        }
    }
}