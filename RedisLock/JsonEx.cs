// *************************************************************************************************************************
// Project          : RedisLock
// File             : JsonEx.cs
// Created          : 2018-05-14  16:29
//
// Last Modified By : 马新心(jstsmaxx@163.com)
// Last Modified On : 2018-05-14  16:29
// *************************************************************************************************************************

using System;
using Newtonsoft.Json;

namespace RedisLock
{
    public static class JsonEx
    {
        public static T FromJson<T>(this string str) where T : class
        {
            if (str == null) throw new Exception("对象值不能为空");

            return JsonConvert.DeserializeObject<T>(str);
        }

        public static string ToJson(this object obj)
        {
            if (obj == null) throw new Exception("对象值不能为空");

            return JsonConvert.SerializeObject(obj);
        }
    }
}