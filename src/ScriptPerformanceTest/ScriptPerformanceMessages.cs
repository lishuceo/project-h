namespace GameEntry.ScriptPerformanceTest;

/// <summary>
/// 脚本性能测试请求消息
/// </summary>
public class ScriptPerformanceTestRequest
{
    /// <summary>
    /// 测试类型
    /// </summary>
    public string TestType { get; set; } = string.Empty;
    
    /// <summary>
    /// 测试次数
    /// </summary>
    public int TestCount { get; set; }
    
    /// <summary>
    /// 客户端发送时间
    /// </summary>
    public DateTime ClientSentTime { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 唯一测试ID
    /// </summary>
    public Guid TestId { get; set; } = Guid.NewGuid();
    
    public override string ToString()
    {
        return $"ScriptPerformanceTestRequest({TestType}): Count={TestCount}, TestId={TestId}";
    }
}

/// <summary>
/// 脚本性能测试结果消息
/// </summary>
public class ScriptPerformanceTestResult
{
    /// <summary>
    /// 测试类型
    /// </summary>
    public string TestType { get; set; } = string.Empty;
    
    /// <summary>
    /// 测试次数
    /// </summary>
    public int TestCount { get; set; }
    
    /// <summary>
    /// 服务端执行耗时（毫秒）
    /// </summary>
    public long ServerElapsedMs { get; set; }
    
    /// <summary>
    /// 客户端发送时间
    /// </summary>
    public DateTime ClientSentTime { get; set; }
    
    /// <summary>
    /// 服务端接收时间
    /// </summary>
    public DateTime ServerReceivedTime { get; set; }
    
    /// <summary>
    /// 服务端完成时间
    /// </summary>
    public DateTime ServerCompletedTime { get; set; }
    
    /// <summary>
    /// 唯一测试ID
    /// </summary>
    public Guid TestId { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// 错误信息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 额外的测试信息
    /// </summary>
    public Dictionary<string, object> AdditionalInfo { get; set; } = new();
    
    public override string ToString()
    {
        return $"ScriptPerformanceTestResult({TestType}): {TestCount} iterations in {ServerElapsedMs}ms, Success={Success}";
    }
}


