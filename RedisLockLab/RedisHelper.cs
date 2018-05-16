// *************************************************************************************************************************
// Project          : RedisLock
// File             : RedisHelper.cs
// Created          : 2018-05-15  10:18
//
// Last Modified By : 马新心(ma.xinxin@jinyinmao.com.cn)
// Last Modified On : 2018-05-15  10:18
// *************************************************************************************************************************

using StackExchange.Redis;

namespace RedisLockLab
{
    internal class RedisHelper
    {
        public static ConnectionMultiplexer connectionMultiplexer;

        /// <summary>
        ///     锁
        /// </summary>
        private static readonly object Locker = new object();

        public static IConnectionMultiplexer ConnectionRedisMultiplexer
        {
            get
            {
                if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                {
                    lock (Locker)
                    {
                        if (connectionMultiplexer == null || !connectionMultiplexer.IsConnected)
                        {
                            ConfigurationOptions options = GetConfigurationOptions("XXXXXXXXXXXX");
                            connectionMultiplexer = ConnectionMultiplexer.Connect(options);
                        }
                    }
                }

                return connectionMultiplexer;
            }
        }

        /// <summary>
        ///     redis初始化
        /// </summary>
        /// <param name="bizRedisConnectionString"></param>
        /// <returns></returns>
        private static ConfigurationOptions GetConfigurationOptions(string bizRedisConnectionString)
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(bizRedisConnectionString, true);
            options.AbortOnConnectFail = false;
            options.AllowAdmin = true;
            options.ConnectRetry = 10;
            options.ConnectTimeout = 20000;
            options.DefaultDatabase = 0;
            options.ResponseTimeout = 20000;
            options.Ssl = false;
            options.SyncTimeout = 20000;
            return options;
        }
    }
}