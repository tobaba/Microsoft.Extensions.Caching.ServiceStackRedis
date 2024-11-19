namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    /// <summary>
    /// 描 述：缓存库分配
    /// </summary>
    public static class RedisCacheId
    {
        #region 0号库
        public static long Default => 0;

        #endregion

        #region 1号库
        public static long Admin => 1;

        #endregion
    }
}