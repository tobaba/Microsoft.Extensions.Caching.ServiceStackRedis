namespace Microsoft.Extensions.Caching.ServiceStackRedis
{
    /// <summary>
    /// Claims名称
    /// </summary>
    public static class ClaimsName
    {
        /// <summary>
        /// 租户编号
        /// </summary>
        public const string TenantId = "td";

        /// <summary>
        /// 是否允许重复登录
        /// </summary>
        public const string IsRepeatLogin  = "rl";
        /// <summary>
        /// 账户编号
        /// </summary>
        public const string AccountId = "id";
            
    }
}
