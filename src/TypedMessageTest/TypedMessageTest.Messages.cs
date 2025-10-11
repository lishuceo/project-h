using System.Text.Json.Serialization;

namespace GameEntry.TypedMessageTest;

/// <summary>
/// 基础测试消息
/// </summary>
public class TestMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string TestType { get; set; } = "General";
    
    public override string ToString()
    {
        return $"TestMessage({TestType}): {Content}";
    }
}

/// <summary>
/// 性能测试消息
/// </summary>
public class PerformanceTestMessage
{
    public int MessageId { get; set; }
    public string Payload { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
    
    public override string ToString()
    {
        return $"PerformanceTestMessage({MessageId}): {Payload}";
    }
}

/// <summary>
/// 错误测试消息
/// </summary>
public class ErrorTestMessage
{
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorContent { get; set; } = string.Empty;
    public bool ShouldFail { get; set; } = false;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return $"ErrorTestMessage({ErrorType}): {ErrorContent}";
    }
}

/// <summary>
/// 网络测试消息
/// </summary>
public class NetworkTestMessage
{
    public string NetworkCondition { get; set; } = string.Empty;
    public int PacketSize { get; set; } = 0;
    public bool SimulateLatency { get; set; } = false;
    public TimeSpan ExpectedDelay { get; set; } = TimeSpan.Zero;
    public DateTime SentTime { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return $"NetworkTestMessage({NetworkCondition}): PacketSize={PacketSize}B";
    }
}

/// <summary>
/// 互动测试消息（用于客户端-服务器交互测试）
/// </summary>
public class InteractiveTestMessage
{
    public string Command { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string ResponseRequired { get; set; } = string.Empty;
    public Guid ConversationId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return $"InteractiveTestMessage({Command}): ConversationId={ConversationId}";
    }
}

/// <summary>
/// 批量测试消息
/// </summary>
public class BatchTestMessage
{
    public int BatchId { get; set; }
    public int MessageIndex { get; set; }
    public int BatchSize { get; set; }
    public string BatchType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime BatchStartTime { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return $"BatchTestMessage({BatchType}): Batch={BatchId}, Index={MessageIndex}/{BatchSize}";
    }
}

/// <summary>
/// 客户端状态消息
/// </summary>
public class ClientStateMessage
{
    public string ClientId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public Dictionary<string, object> StateData { get; set; } = new();
    public DateTime StateTime { get; set; } = DateTime.UtcNow;
    
    public override string ToString()
    {
        return $"ClientStateMessage({ClientId}): {State}";
    }
}

/// <summary>
/// 服务器广播消息
/// </summary>
public class ServerBroadcastMessage
{
    public string BroadcastType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime BroadcastTime { get; set; } = DateTime.UtcNow;
    public string[] TargetPlayers { get; set; } = Array.Empty<string>();
    
    public override string ToString()
    {
        return $"ServerBroadcastMessage({BroadcastType}): {Message}";
    }
}

/// <summary>
/// 压力测试消息
/// </summary>
public class StressTestMessage
{
    public int ThreadId { get; set; }
    public int MessageNumber { get; set; }
    public string LargePayload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; } = TimeSpan.Zero;
    
    public StressTestMessage()
    {
        // 生成较大的负载用于压力测试
        LargePayload = string.Join("", Enumerable.Repeat("StressTest", 100));
    }
    
    public override string ToString()
    {
        return $"StressTestMessage(Thread={ThreadId}, Msg={MessageNumber}): PayloadSize={LargePayload.Length}";
    }
}

/// <summary>
/// 延迟测试消息
/// </summary>
public class LatencyTestMessage
{
    public Guid TestId { get; set; } = Guid.NewGuid();
    public DateTime ClientSentTime { get; set; } = DateTime.UtcNow;
    public DateTime ServerReceivedTime { get; set; }
    public DateTime ServerSentTime { get; set; }
    public DateTime ClientReceivedTime { get; set; }
    public string Direction { get; set; } = "ClientToServer"; // or "ServerToClient"
    
    [JsonIgnore]
    public TimeSpan RoundTripTime => ClientReceivedTime - ClientSentTime;
    
    [JsonIgnore]
    public TimeSpan OneWayLatency => Direction == "ClientToServer" 
        ? ServerReceivedTime - ClientSentTime 
        : ClientReceivedTime - ServerSentTime;
    
    public override string ToString()
    {
        return $"LatencyTestMessage({Direction}): TestId={TestId}, RTT={RoundTripTime.TotalMilliseconds:F2}ms";
    }
}

/// <summary>
/// 序列化测试消息（包含各种数据类型）
/// </summary>
public class SerializationTestMessage
{
    public bool BoolValue { get; set; } = true;
    public int IntValue { get; set; } = 42;
    public long LongValue { get; set; } = 1234567890L;
    public float FloatValue { get; set; } = 3.14f;
    public double DoubleValue { get; set; } = 2.71828;
    public string StringValue { get; set; } = "Hello, TypedMessage!";
    public DateTime DateTimeValue { get; set; } = DateTime.UtcNow;
    public Guid GuidValue { get; set; } = Guid.NewGuid();
    public int[] IntArray { get; set; } = { 1, 2, 3, 4, 5 };
    public List<string> StringList { get; set; } = new() { "A", "B", "C" };
    public Dictionary<string, int> StringIntDict { get; set; } = new() { { "One", 1 }, { "Two", 2 } };
    public NestedTestData? NestedData { get; set; } = new();
    
    public class NestedTestData
    {
        public string Name { get; set; } = "Nested";
        public int Value { get; set; } = 100;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    
    public override string ToString()
    {
        return $"SerializationTestMessage: String={StringValue}, Int={IntValue}, Nested={NestedData?.Name}";
    }
} 