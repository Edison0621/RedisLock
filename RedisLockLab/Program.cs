using System;
using System.Threading;
using RedisLock;
using StackExchange.Redis;

namespace RedisLockLab
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            IDatabase database = RedisHelper.ConnectionRedisMultiplexer.GetDatabase(5);
            RedisLockHelper redisLock = new RedisLockHelper(database);

            int j = 0;

            ////测试1
            //for (int i = 0; i < 200; i++)
            //{
            //    ThreadPool.QueueUserWorkItem(s => Console.WriteLine(++j));//连库测试更直观
            //}

            //测试2
            for (int i = 0; i < 20; i++)
            {
                ThreadPool.QueueUserWorkItem(s =>
                {
                    var result = redisLock.LockAsync("Lock:Object", TimeSpan.FromMilliseconds(500)).GetAwaiter().GetResult();
                    if (result.Item1)
                    {
                        Console.WriteLine(++j);
                    }
                });
            }

            //threads.ForEach(p => p.Start());

            Console.Read();
        }
    }
}