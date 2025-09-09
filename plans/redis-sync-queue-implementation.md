# Redis同步队列基础设施 - 实施计划

## 设计理念
从第一阶段开始就建立完善的架构设计和抽象接口，然后在功能范围上渐进实施。每个阶段都是生产就绪的，避免临时方案和重构债务。

## Phase 1: 完整基础设施 + Telegram支持 (核心阶段)

### 架构设计
- **完整接口抽象**: ISyncQueueService + ISyncHandler
- **生产级Redis队列**: 主队列 + 延迟队列 + 死信队列 + 处理中队列  
- **通用后台处理器**: 支持多handler注册的并发处理服务
- **完整配置系统**: 支持每个服务独立的重试策略配置
- **监控API**: 队列统计、任务管理、故障恢复功能

### 数据结构设计
```csharp
public class SyncTask<T>
{
    public string Id { get; set; }
    public string Service { get; set; }
    public string Action { get; set; }
    public long EntityId { get; set; }
    public long UserId { get; set; }
    public T Payload { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}

public class SyncResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShouldRetry { get; set; }
    public TimeSpan? CustomRetryDelay { get; set; }
}
```

### 核心接口
```csharp
public interface ISyncQueueService
{
    Task EnqueueAsync<T>(string service, SyncTask<T> task);
    Task<SyncTask<T>?> DequeueAsync<T>(string service, CancellationToken cancellationToken);
    Task ScheduleRetryAsync<T>(string service, SyncTask<T> task, TimeSpan delay);
    Task MoveToFailedAsync<T>(string service, SyncTask<T> task, string error);
    Task<QueueStats> GetStatsAsync(string service);
    Task RetryFailedTasksAsync(string service);
}

public interface ISyncHandler
{
    string ServiceName { get; }
    Task<SyncResult> ProcessAsync(SyncTask task, CancellationToken cancellationToken);
    TimeSpan CalculateRetryDelay(int attemptCount);
    int MaxRetryAttempts { get; }
}
```

### Redis队列设计
```
队列命名规范:
- 主队列: sync:{service}:queue
- 延迟队列: sync:{service}:delayed (ZSET)
- 死信队列: sync:{service}:failed
- 处理中: sync:{service}:processing

例如:
- sync:telegram:queue
- sync:telegram:delayed
- sync:telegram:failed
- sync:telegram:processing
```

### 配置系统
```json
{
  "SyncQueue": {
    "Redis": {
      "ConnectionString": "localhost:6379",
      "Database": 1,
      "KeyPrefix": "happynotes:sync:"
    },
    "Processing": {
      "MaxConcurrentTasks": 10,
      "ProcessingTimeout": "00:05:00",
      "PollingInterval": "00:00:30"
    },
    "Handlers": {
      "telegram": {
        "maxRetries": 3,
        "baseDelaySeconds": 60,
        "backoffMultiplier": 2.0,
        "maxDelayMinutes": 60
      },
      "mastodon": {
        "maxRetries": 5,
        "baseDelaySeconds": 30,
        "backoffMultiplier": 1.5,
        "maxDelayMinutes": 30
      }
    }
  }
}
```

### 实现范围 (Phase 1)
- RedisSyncQueueService: 完整的队列操作实现
- SyncQueueProcessor: 通用的多服务并发处理器
- TelegramSyncHandler: Telegram同步的具体实现  
- SyncQueueAdminController: 管理和监控API
- 完整的配置、日志、错误处理

### 集成改造
- 修改TelegramSyncNoteService使用队列
- 保持现有API完全向后兼容
- 失败时自动入队，成功时跳过队列

### 监控API
```csharp
[ApiController]
[Route("api/admin/sync-queue")]
public class SyncQueueAdminController : ControllerBase
{
    [HttpGet("stats/{service}")]
    public Task<QueueStats> GetStats(string service);
    
    [HttpPost("{service}/retry-failed")]  
    public Task RetryFailed(string service);
    
    [HttpPost("{service}/pause")]
    public Task PauseProcessing(string service);
    
    [HttpDelete("{service}/clear")]
    public Task ClearQueue(string service);
}
```

## Phase 2: Mastodon支持
### 范围
- 实现MastodonSyncHandler
- 修改MastodonSyncNoteService集成队列  
- 验证多服务并发处理
- 配置Mastodon特定重试策略

### 架构影响
- 零架构变更，纯粹添加新handler
- 复用所有现有基础设施

## Phase 3: Manticore搜索支持
### 范围  
- 实现ManticoreSyncHandler
- 可能优化批处理能力（搜索索引适合批量操作）
- 集成现有搜索索引逻辑

## Phase 4: 高级特性
### 范围
- 批量处理优化
- 优先级队列支持
- 任务去重机制  
- 队列暂停/恢复功能
- 高级监控和告警

## 技术收益
1. **高可用**: API临时不可用时任务不丢失
2. **性能**: 异步处理，不阻塞用户操作  
3. **统一**: 所有同步服务使用相同基础设施
4. **可扩展**: 新增同步服务只需实现handler
5. **可运维**: 完整的监控、管理、故障恢复能力

## 实施策略  
- 第一阶段建立完整的生产级基础设施
- 后续阶段纯粹是功能范围扩展
- 每个阶段都可以独立部署和回滚
- 现有功能始终保持向后兼容

## 状态跟踪
- [x] 计划制定
- [x] Phase 1: 基础设施 + Telegram支持 ✅ **PRODUCTION READY**
  - [x] 核心接口定义
  - [x] Redis队列服务实现
  - [x] 后台处理器实现
  - [x] Telegram Handler实现
  - [x] 集成改造 (基础版本)
  - [x] 监控API
  - [x] 测试验证
  - [x] Redis服务器配置
  - [x] **Critical Bug修复**: 原子化dequeue操作 (Lua脚本)
  - [x] **Critical Bug修复**: Handler生命周期管理 (每任务scope)
  - [x] **代码质量**: JsonSerializerOptions统一配置
  - [x] **CI/CD升级**: GitHub Actions + Redis容器 + 分层测试
  - [x] **部署配置**: Staging/Production Redis配置完成
- [ ] Phase 2: Mastodon支持
- [ ] Phase 3: Manticore支持  
- [ ] Phase 4: 高级特性

## 创建时间
2025-01-15

## 更新记录
- 2025-01-15: 初始计划创建
- 2025-01-15: Phase 1 基本完成
  - ✅ 核心基础设施已实现并测试通过
  - ✅ Redis服务器连接配置完成 (seq.shukebeta.eu.org)
  - ✅ 基本Telegram同步支持（CREATE操作 + queue fallback）
  - ✅ 监控和管理API完成
- 2025-09-09: Phase 1 **生产就绪** 🎉
  - ✅ **严重Bug修复**: 原子化dequeue (消除数据丢失风险)
  - ✅ **严重Bug修复**: Handler生命周期 (消除内存泄漏)
  - ✅ **代码质量提升**: 统一JSON序列化配置
  - ✅ **CI/CD完善**: Redis集成测试 + GitHub Actions优化
  - ✅ **部署就绪**: Staging (6780) / Production (6781) 配置
  - 📝 **下一步**: 部署到staging验证 → Phase 2 (Mastodon)