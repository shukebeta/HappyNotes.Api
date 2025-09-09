# Redis同步队列基础设施 - Phase 1完成交接文档

## 背景
用户原本遇到Telegram同步因IPv6网络问题导致的100%失败率。我们冤枉了Polly重试机制（实际是IPv4/IPv6连接问题）。用户提出需要一个Redis队列基础设施来应对未来类似的API临时不可用问题。

## 项目目标
构建一个通用的Redis同步队列系统，支持Telegram、Mastodon、Manticore等多种同步服务的异步处理和重试机制。采用渐进式实施，从完善架构开始但功能逐步扩展。

## 已完成的成果 (Phase 1)

### ✅ 核心架构实现
位置：`src/HappyNotes.Services/SyncQueue/`

**核心接口和模型**：
- `ISyncQueueService` - 通用队列操作接口
- `ISyncHandler` - 同步服务处理器接口
- `SyncTask<T>` - 通用任务数据结构
- `SyncResult` - 处理结果模型
- `QueueStats` - 队列统计模型

**队列服务实现**：
- `RedisSyncQueueService` - 完整的Redis队列实现
  - 主队列 (sync:{service}:queue)
  - 延迟队列 (sync:{service}:delayed) - ZSET用于重试调度
  - 死信队列 (sync:{service}:failed)
  - 处理中队列 (sync:{service}:processing)
- 支持入队、出队、重试调度、失败处理、统计查询

**后台处理器**：
- `SyncQueueProcessor` - 通用BackgroundService
- 支持多服务并发处理
- 信号量控制并发数量
- 完整的错误处理和重试逻辑

### ✅ Telegram集成
- `TelegramSyncHandler` - 处理所有Telegram同步操作
- `TelegramSyncPayload` - Telegram特定的负载数据结构
- 支持CREATE/UPDATE/DELETE操作，智能重试策略
- 区分可重试和不可重试异常

**现有服务改造**：
- `TelegramSyncNoteService` 已改造支持队列fallback
- 策略：先尝试立即同步，失败时自动入队
- 完全向后兼容，现有API无变化

### ✅ 监控和管理
位置：`src/HappyNotes.Api/Controllers/SyncQueueAdminController.cs`

**API端点**：
- `GET /api/admin/sync-queue/stats` - 全局队列统计
- `GET /api/admin/sync-queue/stats/{service}` - 特定服务统计
- `POST /api/admin/sync-queue/{service}/retry-failed` - 重试失败任务
- `DELETE /api/admin/sync-queue/{service}/clear` - 清空队列
- `GET /api/admin/sync-queue/health` - 健康检查

### ✅ 配置系统
**Redis服务器配置**：
- Development: `seq.shukebeta.eu.org:6379`
- Staging: `seq.shukebeta.eu.org:6380`
- Production: `seq.shukebeta.eu.org:6379` (生产用6381，但配置文件中为6379)

**重试策略配置** (appsettings.json)：
```json
"SyncQueue": {
  "Handlers": {
    "telegram": {
      "maxRetries": 3,
      "baseDelaySeconds": 60,
      "backoffMultiplier": 2.0,
      "maxDelayMinutes": 60
    }
  }
}
```

### ✅ 测试验证
- 编译无错误 ✅
- 所有现有测试通过 (109个) ✅
- Redis队列集成测试通过 (6个新测试) ✅
- 向后兼容性完全保证 ✅

## 当前状态

**已部署就绪**：
- 基础设施完整实现并测试通过
- Redis连接配置完成并验证正常
- CREATE操作已支持队列fallback
- 监控API可用

**功能范围**：
- ✅ Telegram CREATE操作 + 队列fallback
- ⚠️ UPDATE/DELETE操作需要进一步完善
- ⚠️ 只有TelegramSyncNoteService.SyncNewNote完全改造，其他方法需要类似改造

## 下一步工作重点

### 1. 完善Telegram队列支持
**优先级：高**
- 完善 `TelegramSyncNoteService.SyncEditNote` 方法的队列支持
- 完善 `TelegramSyncNoteService.SyncDeleteNote` 方法的队列支持
- 处理UPDATE/DELETE操作的消息ID管理复杂度

### 2. Phase 2: 扩展到其他服务
**优先级：中**
- 实现 `MastodonSyncHandler`
- 实现 `ManticoreSyncHandler`
- 为每个服务配置独立的重试策略

### 3. 高级特性
**优先级：低**
- 批量处理支持（特别是搜索索引）
- 优先级队列
- 任务去重机制

## 重要文件位置

**核心实现**：
```
src/HappyNotes.Services/SyncQueue/
├── Models/
│   ├── SyncTask.cs
│   └── TelegramSyncPayload.cs
├── Interfaces/
│   ├── ISyncQueueService.cs
│   └── ISyncHandler.cs
├── Services/
│   ├── RedisSyncQueueService.cs
│   └── SyncQueueProcessor.cs
├── Handlers/
│   └── TelegramSyncHandler.cs
├── Configuration/
│   └── SyncQueueOptions.cs
└── Extensions/
    └── ServiceCollectionExtensions.cs
```

**配置文件**：
- `src/HappyNotes.Api/appsettings.json` - 生产配置
- `src/HappyNotes.Api/appsettings.Development.json` - 开发配置
- `src/HappyNotes.Api/appsettings.Staging.json` - 测试配置

**监控API**：
- `src/HappyNotes.Api/Controllers/SyncQueueAdminController.cs`

**测试**：
- `tests/HappyNotes.Services.Tests/SyncQueue/RedisSyncQueueServiceTests.cs`

**计划文档**：
- `plans/redis-sync-queue-implementation.md`

## 用户期望和约束

1. **渐进式实施** - 用户明确要求从一开始就有完善架构，然后功能范围渐进扩展
2. **生产就绪** - 每个阶段都要能独立部署和回滚
3. **不破坏现有功能** - 完全向后兼容
4. **统一基础设施** - 所有同步服务使用相同的队列基础设施

## 技术细节提醒

1. **Redis连接管理** - 使用 `IConnectionMultiplexer` 单例
2. **任务序列化** - 使用 `System.Text.Json`，注意JsonElement处理
3. **并发控制** - `SemaphoreSlim` 限制并发任务数
4. **错误分类** - 区分可重试和不可重试异常
5. **配置热更新** - 使用 `IOptions<T>` 支持配置变更

## 成功指标

Phase 1已达成：
- ✅ 编译无错误
- ✅ 所有测试通过
- ✅ Redis连接正常
- ✅ 基础CREATE操作队列化成功
- ✅ 监控API可用

下一阶段目标：
- 完善UPDATE/DELETE操作支持
- 部署到staging环境验证
- 观察生产环境队列使用情况

---

**最后提醒**：用户对代码质量要求很高，坚持KISS原则，重视测试和向后兼容性。记住我们的目标是解决API临时不可用问题，而不是优化现有的正常流程。