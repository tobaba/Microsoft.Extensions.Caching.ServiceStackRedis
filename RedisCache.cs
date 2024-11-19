using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Redis;

namespace Microsoft.Extensions.Caching.ServiceStackRedis;

/// <summary>
/// 描 述：redis操作方法
/// </summary>
public class RedisCache(IRedisClientsManager redisClientCacheManager)
{

    public readonly IRedisClientsManager RedisClientCacheManager = redisClientCacheManager;//IRedisClientCacheManager

    #region -- Item --
    /// <summary>
    /// 设置单体
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">值</param>
    /// <param name="dbId">库Id</param>
    /// <returns></returns>
    public bool Set<T>(string key, T t, long dbId = 0)
    {
         
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.Set(key, t);
    }
    public bool SetValueIfNotExists(string key, string t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.SetValueIfNotExists(key, t);
    }
    /// <summary>
    /// 设置单体
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">值</param>
    /// <param name="timeSpan">保存时间</param>
    /// <param name="dbId">库Id</param>
    /// <returns></returns>
    public bool Set<T>(string key, T t, TimeSpan timeSpan, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.Set(key, t, timeSpan);
    }

    /// <summary>
    /// 设置单体
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">值</param>
    /// <param name="dateTime">过期时间</param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public bool Set<T>(string key, T t, DateTime dateTime, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.Set(key, t, dateTime);
    }
    public T Store<T>(string key, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.Store(t);
    }
    /// <summary>
    /// 获取单体
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public T Get<T>(string key, long dbId = 0) where T : class
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.Get<T>(key);
    }
    public List<string> SearchKeys(string pattern, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.SearchKeys(pattern);
    }
    /// <summary>
    /// 移除单体
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="dbId"></param>
    public bool Remove(string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.Remove(key);
    }
    /// <summary>
    /// 清空所有缓存
    /// </summary>
    public void RemoveAll(long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        redis.FlushDb();
    }

    /// <summary>
    /// 删除指定前缀的key redis-cli keys 1.cn*|xargs redis-cli del 
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="dbId"></param>
    /// <param name="pageSize"></param>
    public void RemoveByPattern(string pattern, int pageSize = 1000, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var listKeys = redis.ScanAllKeys(pattern, pageSize).ToList();

        if (listKeys.Any())
        {
            using var trans = redis.CreateTransaction();
            foreach (var key in listKeys)
            {
                trans.QueueCommand(r => r.Remove(key));
            }

            trans.Commit();
        }
    }
    #endregion

    #region -- List --
    public List<T> List_Get<T>(string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        return redisTypedClient.GetAllItemsFromList(redisTypedClient.Lists[key]);
    }
    /// <summary>
    /// 添加列表
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">值</param>
    /// <param name="dbId">库</param>
    public void List_Add<T>(string key, T t, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        redisTypedClient.AddItemToList(redisTypedClient.Lists[key], t);
    }
    /// <summary>
    /// 移除列表某个值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool List_Remove<T>(string key, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        return redisTypedClient.RemoveItemFromList(redisTypedClient.Lists[key], t) > 0;
    }
    /// <summary>
    /// 移除列表所有值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="dbId">库Id</param>
    public void List_RemoveAll<T>(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        redisTypedClient.Lists[key].RemoveAll();

    }
    /// <summary>
    /// 获取列表数据条数
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public long List_Count(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.GetListCount(key);

    }
    public List<string> GetAllItemsFromList(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.GetAllItemsFromList(key);

    }
    /// <summary>
    /// 获取指定条数列表数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="start">开始编号</param>
    /// <param name="count">条数</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public List<T> List_GetRange<T>(string key, int start, int count, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        var c = redis.As<T>();
        return c.Lists[key].GetRange(start, start + count - 1);

    }
    /// <summary>
    /// 获取列表所有数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="dbId">库数据</param>
    /// <returns></returns>
    public List<T> List_GetList<T>(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        var c = redis.As<T>();
        return c.Lists[key].GetRange(0, c.Lists[key].Count);
    }
    /// <summary>
    /// 获取列表分页数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public List<T> List_GetList<T>(string key, int pageIndex, int pageSize, long dbId = 0)
    {
        int start = pageSize * (pageIndex - 1);
        return List_GetRange<T>(key, start, pageSize, dbId);
    }
    #endregion

    #region -- Set --
    /// <summary>
    /// 添加集合
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">数值</param>
    /// <param name="dbId">库</param>
    public void Set_Add<T>(string key, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        redisTypedClient.Sets[key].Add(t);
    }
    /// <summary>
    /// 集合是否包含指定数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">数值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool Set_Contains<T>(string key, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        return redisTypedClient.Sets[key].Contains(t);
    }
    /// <summary>
    /// 移除集合某个值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="t">数值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool Set_Remove<T>(string key, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var redisTypedClient = redis.As<T>();
        return redisTypedClient.Sets[key].Remove(t);
    }
    #endregion

    #region -- Hash --
    /// <summary>
    /// 判断某个数据是否已经被缓存
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">hashID</param>
    /// <param name="dataKey">键值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool Hash_Exist<T>(string key, string dataKey, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.HashContainsEntry(key, dataKey);

    }

    /// <summary>
    /// 存储数据到hash表
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">hashID</param>
    /// <param name="dataKey">键值</param>
    /// <param name="t">数值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool Hash_Set<T>(string key, string dataKey, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        string value = ServiceStack.Text.JsonSerializer.SerializeToString(t);
        return redis.SetEntryInHash(key, dataKey, value);
    }
    /// <summary>
    /// 移除hash中的某值
    /// </summary>
    /// <param name="key">hashID</param>
    /// <param name="dataKey">键值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool Hash_Remove(string key, string dataKey, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.RemoveEntryFromHash(key, dataKey);
    }
    /// <summary>
    /// 移除整个hash
    /// </summary>
    /// <param name="key">hashID</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool Hash_Remove(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.Remove(key);
    }
    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">hashID</param>
    /// <param name="dataKey">键值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public T Hash_Get<T>(string key, string dataKey, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        string value = redis.GetValueFromHash(key, dataKey);
        return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(value);
    }
    /// <summary>
    /// 获取整个hash的数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">hashID</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public List<T>? Hash_GetAll<T>(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        var list = redis.GetHashValues(key);
        if (list != null && list.Count > 0)
        {
            List<T> result = new List<T>();
            foreach (var item in list)
            {
                var value = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(item);
                result.Add(value);
            }
            return result;
        }
        return null;
    }
    #endregion

    #region -- SortedSet --
    /// <summary>
    ///  添加数据到 SortedSet
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">集合id</param>
    /// <param name="t">数值</param>
    /// <param name="score">排序码</param>
    /// <param name="dbId">库</param>
    public bool SortedSet_Add<T>(string key, T t, double score, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        string value = ServiceStack.Text.JsonSerializer.SerializeToString(t);
        return redis.AddItemToSortedSet(key, value, score);
    }
    /// <summary>
    /// 移除数据从SortedSet
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">集合id</param>
    /// <param name="t">数值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public bool SortedSet_Remove<T>(string key, T t, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        string value = ServiceStack.Text.JsonSerializer.SerializeToString(t);
        return redis.RemoveItemFromSortedSet(key, value);
    }
    /// <summary>
    /// 修剪SortedSet
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="size">保留的条数</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public long SortedSet_Trim(string key, int size, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.RemoveRangeFromSortedSet(key, size, 9999999);
    }
    /// <summary>
    /// 获取SortedSet的长度
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public long SortedSet_Count(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.GetSortedSetCount(key);
    }

    /// <summary>
    /// 获取SortedSet的分页数据
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="key">键值</param>
    /// <param name="pageIndex">页码</param>
    /// <param name="pageSize">每页条数</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public List<T>? SortedSet_GetList<T>(string key, int pageIndex, int pageSize, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        var list = redis.GetRangeFromSortedSet(key, (pageIndex - 1) * pageSize, pageIndex * pageSize - 1);
        if (list != null && list.Count > 0)
        {
            List<T> result = new List<T>();
            foreach (var item in list)
            {
                var data = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(item);
                result.Add(data);
            }
            return result;
        }

        return null;
    }


    /// <summary>
    /// 获取SortedSet的全部数据
    /// </summary>
    /// <typeparam name="T">类</typeparam>
    /// <param name="key">键值</param>
    /// <param name="dbId">库</param>
    /// <returns></returns>
    public List<T>? SortedSet_GetListALL<T>(string key, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        var list = redis.GetRangeFromSortedSet(key, 0, 9999999);
        if (list is { Count: > 0 })
        {
            List<T> result = new List<T>();
            foreach (var item in list)
            {
                var data = ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(item);
                result.Add(data);
            }
            return result;
        }

        return null;
    }
    #endregion

    #region 公用方法
    /// <summary>
    /// 设置缓存过期
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="datetime">过期时间</param>
    /// <param name="dbId">库</param>
    public void SetExpire(string key, DateTime datetime, long dbId = 0)
    {
        using IRedisClient redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        redis.ExpireEntryAt(key, datetime);
    }
    #endregion

    #region 扩展
    /// <summary>
    /// 保存数据DB文件到硬盘
    /// </summary>
    public void Save()
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Save();
    }
    /// <summary>
    /// 异步保存数据DB文件到硬盘
    /// </summary>
    public void SaveAsync()
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.SaveAsync();
    }
    /// <summary>
    /// Redis获取自增长序号(正则) 并设置60秒过期 防止缓存穿透
    /// </summary>
    /// <returns></returns>
    public long IncrId(string pattern, int incr)
    {

        long count;
        using var redis = RedisClientCacheManager.GetClient();
        var result = redis.Get<string>(pattern);
        if (string.IsNullOrEmpty(result))
        {
            count = redis.GetKeysByPattern(pattern).Count() + incr;
        }
        else
        {
            count = Convert.ToInt32(result) + incr;
        }
        redis.Set(pattern, count.ToString(), new TimeSpan(0, 0, 60));

        return count;
    }

    #region ZSet

    #region 添加
    /// <summary>
    /// 添加key/value，默认分数是从1.多*10的9次方以此递增的,自带自增效果
    /// </summary>
    public bool AddItemToSortedSet(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.AddItemToSortedSet(key, value);
    }
    /// <summary>
    /// 添加key/value,并设置value的分数
    /// </summary>
    public bool AddItemToSortedSet(string key, string value, double score)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.AddItemToSortedSet(key, value, score);
    }
    /// <summary>
    /// 为key添加values集合，values集合中每个value的分数设置为score
    /// </summary>
    public bool AddRangeToSortedSet(string key, List<string> values, double score)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.AddRangeToSortedSet(key, values, score);
    }
    /// <summary>
    /// 为key添加values集合，values集合中每个value的分数设置为score
    /// </summary>
    public bool AddRangeToSortedSet(string key, List<string> values, long score)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.AddRangeToSortedSet(key, values, score);
    }
    #endregion
    #region 获取
    /// <summary>
    /// 获取key的所有集合
    /// </summary>
    public List<string> GetAllItemsFromSortedSet(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetAllItemsFromSortedSet(key);
    }
    /// <summary>
    /// 获取key的所有集合，倒叙输出
    /// </summary>
    public List<string> GetAllItemsFromSortedSetDesc(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetAllItemsFromSortedSetDesc(key);
    }
    /// <summary>
    /// 获取可以的说有集合，带分数
    /// </summary>
    public IDictionary<string, double> GetAllWithScoresFromSortedSet(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetAllWithScoresFromSortedSet(key);
    }
    /// <summary>
    /// 获取key为value的下标值
    /// </summary>
    public long GetItemIndexInSortedSet(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetItemIndexInSortedSet(key, value);
    }
    /// <summary>
    /// 倒叙排列获取key为value的下标值
    /// </summary>
    public long GetItemIndexInSortedSetDesc(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetItemIndexInSortedSetDesc(key, value);
    }
    /// <summary>
    /// 获取key为value的分数
    /// </summary>
    public double GetItemScoreInSortedSet(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetItemScoreInSortedSet(key, value);
    }
    /// <summary>
    /// 获取key所有集合的数据总数
    /// </summary>
    public long GetSortedSetCount(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetSortedSetCount(key);
    }
    /// <summary>
    /// key集合数据从分数为fromscore到分数为toscore的数据总数
    /// </summary>
    public long GetSortedSetCount(string key, double fromScore, double toScore)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetSortedSetCount(key, fromScore, toScore);
    }
    /// <summary>
    /// 获取key集合从高分到低分排序数据，分数从fromscore到分数为toscore的数据
    /// </summary>
    public List<string> GetRangeFromSortedSetByHighestScore(string key, double fromscore, double toscore)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeFromSortedSetByHighestScore(key, fromscore, toscore);
    }
    /// <summary>
    /// 获取key集合从低分到高分排序数据，分数从fromscore到分数为toscore的数据
    /// </summary>
    public List<string> GetRangeFromSortedSetByLowestScore(string key, double fromscore, double toscore)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeFromSortedSetByLowestScore(key, fromscore, toscore);
    }
    /// <summary>
    /// 获取key集合从高分到低分排序数据，分数从fromscore到分数为toscore的数据，带分数
    /// </summary>
    public IDictionary<string, double> GetRangeWithScoresFromSortedSetByHighestScore(string key, double fromscore, double toscore)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeWithScoresFromSortedSetByHighestScore(key, fromscore, toscore);
    }
    /// <summary>
    ///  获取key集合从低分到高分排序数据，分数从fromscore到分数为toscore的数据，带分数
    /// </summary>
    public IDictionary<string, double> GetRangeWithScoresFromSortedSetByLowestScore(string key, double fromscore, double toscore)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeWithScoresFromSortedSetByLowestScore(key, fromscore, toscore);
    }
    /// <summary>
    ///  获取key集合数据，下标从fromRank到分数为toRank的数据
    /// </summary>
    public List<string> GetRangeFromSortedSet(string key, int fromRank, int toRank)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeFromSortedSet(key, fromRank, toRank);
    }
    /// <summary>
    /// 获取key集合倒叙排列数据，下标从fromRank到分数为toRank的数据
    /// </summary>
    public List<string> GetRangeFromSortedSetDesc(string key, int fromRank, int toRank)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeFromSortedSetDesc(key, fromRank, toRank);
    }
    /// <summary>
    /// 获取key集合数据，下标从fromRank到分数为toRank的数据，带分数
    /// </summary>
    public IDictionary<string, double> GetRangeWithScoresFromSortedSet(string key, int fromRank, int toRank)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeWithScoresFromSortedSet(key, fromRank, toRank);
    }
    /// <summary>
    ///  获取key集合倒叙排列数据，下标从fromRank到分数为toRank的数据，带分数
    /// </summary>
    public IDictionary<string, double> GetRangeWithScoresFromSortedSetDesc(string key, int fromRank, int toRank)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeWithScoresFromSortedSetDesc(key, fromRank, toRank);
    }
    #endregion
    #region 删除
    /// <summary>
    /// 删除key为value的数据
    /// </summary>
    public bool RemoveItemFromSortedSet(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveItemFromSortedSet(key, value);
    }
    /// <summary>
    /// 删除下标从minRank到maxRank的key集合数据
    /// </summary>
    public long RemoveRangeFromSortedSet(string key, int minRank, int maxRank)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveRangeFromSortedSet(key, minRank, maxRank);
    }
    /// <summary>
    /// 删除分数从fromscore到toscore的key集合数据
    /// </summary>
    public long RemoveRangeFromSortedSetByScore(string key, double fromscore, double toscore)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveRangeFromSortedSetByScore(key, fromscore, toscore);
    }
    /// <summary>
    /// 删除key集合中分数最大的数据
    /// </summary>
    public string PopItemWithHighestScoreFromSortedSet(string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.PopItemWithHighestScoreFromSortedSet(key);
    }
    /// <summary>
    /// 删除key集合中分数最小的数据
    /// </summary>
    public string PopItemWithLowestScoreFromSortedSet(string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.PopItemWithLowestScoreFromSortedSet(key);
    }
    #endregion
    #region 其它
    /// <summary>
    /// 判断key集合中是否存在value数据
    /// </summary>
    public bool SortedSetContainsItem(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.SortedSetContainsItem(key, value);
    }
    /// <summary>
    /// 为key集合值为value的数据，分数加scoreby，返回相加后的分数
    /// </summary>
    public double IncrementItemInSortedSet(string key, string value, double scoreBy)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.IncrementItemInSortedSet(key, value, scoreBy);
    }
    /// <summary>
    /// 获取keys多个集合的交集，并把交集添加的newkey集合中，返回交集数据的总数
    /// </summary>
    public long StoreIntersectFromSortedSets(string newkey, string[] keys)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.StoreIntersectFromSortedSets(newkey, keys);
    }
    /// <summary>
    /// 获取keys多个集合的并集，并把并集数据添加到newkey集合中，返回并集数据的总数
    /// </summary>
    public long StoreUnionFromSortedSets(string newkey, string[] keys)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.StoreUnionFromSortedSets(newkey, keys);
    }
    #endregion

    #endregion

    #region Set

    #region 添加
    /// <summary>
    /// key集合中添加value值
    /// </summary>
    public void AddSet(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddItemToSet(key, value);
    }
    /// <summary>
    /// key集合中添加list集合
    /// </summary>
    public void AddSet(string key, List<string> list)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddRangeToSet(key, list);
    }
    #endregion
    #region 获取
    /// <summary>
    /// 随机获取key集合中的一个值
    /// </summary>
    public string GetRandomItemFromSet(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRandomItemFromSet(key);
    }
    /// <summary>
    /// 获取key集合值的数量
    /// </summary>
    public long GetCountFromSet(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetSetCount(key);
    }
    /// <summary>
    /// 获取所有key集合的值
    /// </summary>
    public HashSet<string> GetAllItemsFromSet(string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetAllItemsFromSet(key);
    }
    #endregion
    #region 删除
    /// <summary>
    /// 随机删除key集合中的一个值
    /// </summary>
    public string PopItemFromSet(string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.PopItemFromSet(key);
    }
    /// <summary>
    /// 删除key集合中的value
    /// </summary>
    public void RemoveItemFromSet(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.RemoveItemFromSet(key, value);
    }
    #endregion
    #region 其它
    /// <summary>
    /// 从fromkey集合中移除值为value的值，并把value添加到tokey集合中
    /// </summary>
    public void MoveBetweenSets(string fromkey, string tokey, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.MoveBetweenSets(fromkey, tokey, value);
    }
    /// <summary>
    /// 返回keys多个集合中的并集，返还hashset
    /// </summary>
    public HashSet<string> GetUnionFromSets(string[] keys)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.GetUnionFromSets(keys);
    }
    /// <summary>
    /// keys多个集合中的并集，放入newkey集合中
    /// </summary>
    public void StoreUnionFromSets(string newkey, string[] keys)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.StoreUnionFromSets(newkey, keys);
    }
    /// <summary>
    /// 把fromkey集合中的数据与keys集合中的数据对比，fromkey集合中不存在keys集合中，则把这些不存在的数据放入newkey集合中
    /// </summary>
    public void StoreDifferencesFromSet(string newkey, string fromkey, string[] keys)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.StoreDifferencesFromSet(newkey, fromkey, keys);
    }
    #endregion

    #endregion

    #region List

    #region 赋值
    /// <summary>
    /// 从左侧向list中添加值
    /// </summary>
    public void LPush(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.PushItemToList(key, value);
    }
    /// <summary>
    /// 从左侧向list中添加值，并设置过期时间
    /// </summary>
    public void LPush(string key, string value, DateTime dt)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.PushItemToList(key, value);
        redis.ExpireEntryAt(key, dt);
    }
    /// <summary>
    /// 从左侧向list中添加值，设置过期时间
    /// </summary>
    public void LPush(string key, string value, TimeSpan sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.PushItemToList(key, value);
        redis.ExpireEntryIn(key, sp);
    }
    /// <summary>
    /// 从左侧向list中添加值
    /// </summary>
    public void RPush(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.PrependItemToList(key, value);
    }
    /// <summary>
    /// 从右侧向list中添加值，并设置过期时间
    /// </summary>    
    public void RPush(string key, string value, DateTime dt)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.PrependItemToList(key, value);
        redis.ExpireEntryAt(key, dt);
    }
    /// <summary>
    /// 从右侧向list中添加值，并设置过期时间
    /// </summary>        
    public void RPush(string key, string value, TimeSpan sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.PrependItemToList(key, value);
        redis.ExpireEntryIn(key, sp);
    }
    /// <summary>
    /// 添加key/value
    /// </summary>     
    public void AddList(string key, string value, long dbId)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        redis.AddItemToList(key, value);
    }
    /// <summary>
    /// 添加key/value ,并设置过期时间
    /// </summary>  
    public void AddList(string key, string value, DateTime dt)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddItemToList(key, value);
        redis.ExpireEntryAt(key, dt);
    }
    /// <summary>
    /// 添加key/value。并添加过期时间
    /// </summary>  
    public void AddList(string key, string value, TimeSpan sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddItemToList(key, value);
        redis.ExpireEntryIn(key, sp);
    }
    /// <summary>
    /// 为key添加多个值
    /// </summary>  
    public void AddList(string key, List<string> values)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddRangeToList(key, values);
    }
    /// <summary>
    /// 为key添加多个值，并设置过期时间
    /// </summary>  
    public void AddList(string key, List<string> values, DateTime dt)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddRangeToList(key, values);
        redis.ExpireEntryAt(key, dt);
    }
    /// <summary>
    /// 为key添加多个值，并设置过期时间
    /// </summary>  
    public void AddList(string key, List<string> values, TimeSpan sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.AddRangeToList(key, values);
        redis.ExpireEntryIn(key, sp);
    }
    #endregion
    #region 获取值
    /// <summary>
    /// 获取list中key包含的数据数量
    /// </summary>  
    public long CountList(string key, long dbId)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.GetListCount(key);
    }
    /// <summary>
    /// 获取key包含的所有数据集合
    /// </summary>  
    public List<string> GetList(string key, long dbId)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.GetAllItemsFromList(key);
    }
    /// <summary>
    /// 获取key中下标为star到end的值集合
    /// </summary>  
    public List<string> GetList(string key, int star, int end)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetRangeFromList(key, star, end);
    }
    #endregion
    #region 阻塞命令
    /// <summary>
    ///  阻塞命令：从list中keys的尾部移除一个值，并返回移除的值，阻塞时间为sp
    /// </summary>  
    public string BlockingPopItemFromList(string key, TimeSpan? sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.BlockingPopItemFromList(key, sp);
    }

    /// <summary>
    ///  阻塞命令：从list中keys的尾部移除一个值，并返回移除的值，阻塞时间为sp
    /// </summary>  
    public ItemRef BlockingPopItemFromLists(string[] keys, TimeSpan? sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.BlockingPopItemFromLists(keys, sp);
    }

    /// <summary>
    /// 阻塞命令：从list中keys的尾部移除一个值，并返回移除的值，阻塞时间为sp
    /// </summary>  
    public ItemRef BlockingDequeueItemFromLists(string[] keys, TimeSpan? sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.BlockingDequeueItemFromLists(keys, sp);
    }
    /// <summary>
    /// 阻塞命令：从list中key的头部移除一个值，并返回移除的值，阻塞时间为sp
    /// </summary>  
    public string BlockingRemoveStartFromList(string keys, TimeSpan? sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.BlockingRemoveStartFromList(keys, sp);
    }
    /// <summary>
    /// 阻塞命令：从list中key的头部移除一个值，并返回移除的值，阻塞时间为sp
    /// </summary>  
    public ItemRef BlockingRemoveStartFromLists(string[] keys, TimeSpan? sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.BlockingRemoveStartFromLists(keys, sp);
    }
    /// <summary>
    /// 阻塞命令：从list中一个fromkey的尾部移除一个值，添加到另外一个tokey的头部，并返回移除的值，阻塞时间为sp
    /// </summary>  
    public string BlockingPopAndPushItemBetweenLists(string fromkey, string tokey, TimeSpan? sp)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.BlockingPopAndPushItemBetweenLists(fromkey, tokey, sp);
    }
    #endregion
    #region 删除
    /// <summary>
    /// 从尾部移除数据，返回移除的数据
    /// </summary>  
    public string PopItemFromList(string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.PopItemFromList(key);
    }
    /// <summary>
    /// 移除list中，key/value,与参数相同的值，并返回移除的数量
    /// </summary>  
    public long RemoveItemFromList(string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveItemFromList(key, value);
    }
    /// <summary>
    /// 从list的尾部移除一个数据，返回移除的数据
    /// </summary>  
    public string RemoveEndFromList(string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveEndFromList(key);
    }
    /// <summary>
    /// 从list的头部移除一个数据，返回移除的值
    /// </summary>  
    public string RemoveStartFromList(string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveStartFromList(key);
    }
    #endregion
    #region 其它
    /// <summary>
    /// 从一个list的尾部移除一个数据，添加到另外一个list的头部，并返回移动的值
    /// </summary>  
    public string PopAndPushItemBetweenLists(string fromKey, string toKey)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.PopAndPushItemBetweenLists(fromKey, toKey);
    }
    #endregion

    #endregion

    #region Hash

    #region 添加
    /// <summary>
    /// 向hashid集合中添加key/value
    /// </summary>       
    public bool SetEntryInHash(string hashid, string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.SetEntryInHash(hashid, key, value);
    }
    /// <summary>
    /// 如果hashid集合中存在key/value则不添加返回false，如果不存在在添加key/value,返回true
    /// </summary>
    public bool SetEntryInHashIfNotExists(string hashid, string key, string value)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.SetEntryInHashIfNotExists(hashid, key, value);
    }
    /// <summary>
    /// 存储对象T t到hash集合中
    /// </summary>
    public void StoreAsHash<T>(T t)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.StoreAsHash(t);
    }

    public void SetRangeInHash(string hashId, IEnumerable<KeyValuePair<string, string>> keyValuePairs)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.SetRangeInHash(hashId, keyValuePairs);
    }
    #endregion
    #region 获取
    /// <summary>
    /// 获取对象T中ID为id的数据。
    /// </summary>
    public T GetFromHash<T>(object id)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetFromHash<T>(id);
    }
    /// <summary>
    /// 获取所有hashid数据集的key/value数据集合
    /// </summary>
    public Dictionary<string, string> GetAllEntriesFromHash(string hashid)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetAllEntriesFromHash(hashid);
    }
    /// <summary>
    /// 获取hashid数据集中的数据总数
    /// </summary>
    public long GetHashCount(string hashid)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetHashCount(hashid);
    }
    /// <summary>
    /// 获取hashid数据集中所有key的集合
    /// </summary>
    public List<string> GetHashKeys(string hashid)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetHashKeys(hashid);
    }
    /// <summary>
    /// 获取hashid数据集中的所有value集合
    /// </summary>
    public List<string> GetHashValues(string hashid)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.GetHashValues(hashid);
    }
    /// <summary>
    /// 获取hashid数据集中，key的value数据
    /// </summary>
    public string GetValueFromHash(string hashid, string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db=dbId;
        return redis.GetValueFromHash(hashid, key);
    }

    /// <summary>
    /// 获取hashid数据集中，多个keys的value集合
    /// </summary>
    /// <param name="hashid"></param>
    /// <param name="keys"></param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public List<string> GetValuesFromHash(string hashid, string[] keys, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.GetValuesFromHash(hashid, keys);
    }
    #endregion
    #region 删除
    #endregion
    /// <summary>
    /// 删除hashid数据集中的key数据
    /// </summary>
    public bool RemoveEntryFromHash(string hashid, string key)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.RemoveEntryFromHash(hashid, key);
    }
    #region 其它
    /// <summary>
    /// 判断hashid数据集中是否存在key的数据
    /// </summary>
    public bool HashContainsEntry(string hashid, string key)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        return redis.HashContainsEntry(hashid, key);
    }
    /// <summary>
    /// 给hashid数据集key的value加countby，返回相加后的数据
    /// </summary>
    public double IncrementValueInHash(string hashid, string key, double countBy)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.IncrementValueInHash(hashid, key, countBy);
    }
    #endregion

    #endregion

    #endregion

    #region 队列操作

    /// <summary>
    /// 往队列中写入(队首)数据
    /// 将一个元素存入指定ListId的ListT的头部
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="dbId"></param>
    public void EnqueueItemOnList(string key, string value, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        redis.EnqueueItemOnList(key, value);
    }

    /// <summary>
    /// 从队列中获取(队尾)最先进队列的数据，先进先出原则
    /// 将指定ListId的ListT末尾的那个元素出列，返回出列元素
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public string DequeueItemFromList(string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.DequeueItemFromList(key);
    }
    /// <summary>
    /// 将指定ListId的ListT队尾的那个元素出列，区别是：会阻塞该ListT，支持超时时间，返回出列元素
    /// 它是命令 LPOP 的阻塞版本，这是因为当给定列表内没有任何元素可供弹出的时候， 连接将被 BLPOP 命令阻塞。
    /// 原指令：BLPOP key [key ...] timeout
    /// 时间复杂度：O(1)
    /// 参考:http://www.redis.cn/commands/blpop.html
    /// </summary>  
    public string BlockingDequeueItemFromList(string key, TimeSpan? sp, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.BlockingDequeueItemFromList(key, sp);
    }

    #endregion

    #region 辅助方法
    /// <summary>
    /// 获取值的长度
    /// </summary>
    public long GetLength(string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetReadOnlyClient();
        redis.Db = dbId;
        return redis.GetStringCount(key);
    }
    /// <summary>
    /// 自增1，返回自增后的值 保存的是10 调用后，+1 返回11
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public long Incr(string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var result = redis.IncrementValue(key);
        redis.ExpireEntryIn(key, TimeSpan.FromHours(10));
        return result;
    }

    /// <summary>
    /// 自增count，返回自增后的值 自定义自增的步长值
    /// </summary>
    public long IncrBy(string key, int count, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.IncrementValueBy(key, count);
    }
    /// <summary>
    /// 自减1，返回自减后的值，Redis操作是单线程操作；不会出现超卖的情况
    /// </summary>
    public long Decr(string key, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        var result = redis.DecrementValue(key);
        redis.ExpireEntryIn(key, TimeSpan.FromHours(10));
        return result;
    }

    /// <summary>
    /// 自减count ，返回自减后的值
    /// </summary>
    /// <param name="key"></param>
    /// <param name="count"></param>
    /// <param name="dbId"></param>
    /// <returns></returns>
    public long DecrBy(string key, int count, long dbId = 0)
    {
        using var redis = RedisClientCacheManager.GetClient();
        redis.Db = dbId;
        return redis.DecrementValueBy(key, count);
    }
    #endregion

    #region  PublishMessage
    /// <summary>
    /// 发送消息给频道
    /// </summary>
    /// <param name="channel">订阅的频道</param>
    /// <param name="msg">消息内容</param>
    /// <returns></returns>
    public long PublishMessage(string channel, string msg)
    {
        using var redis = RedisClientCacheManager.GetClient();
        return redis.PublishMessage(channel, msg);
    }
    

    #endregion
}