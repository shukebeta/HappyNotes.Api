# HappyNotes Bigram 索引迁移步骤

## 前置准备

1. **创建新分支**
```bash
git checkout -b feature/bigram-search
```

2. **备份现有数据**（可选，用于对比）
```sql
-- 连接到 Manticore
mysql -h127.0.0.1 -P9306

-- 备份旧表结构和数据（可选）
CREATE TABLE noteindex_backup LIKE noteindex;
INSERT INTO noteindex_backup SELECT * FROM noteindex;
```

## 迁移步骤

3. **重建 Manticore 容器**
```bash
# 停止并删除容器
cd docker
docker-compose down
docker volume rm docker_manticore_data  # 清空数据

# 重新启动（会使用新的 create_table.sql）
docker-compose up -d
```

4. **验证新表结构**
```sql
mysql -h127.0.0.1 -P9306
DESCRIBE noteindex;
```

5. **重新同步所有数据**
```bash
# 运行数据同步程序重新导入所有笔记
# 具体命令取决于你的数据同步实现
```

## 验证步骤

6. **运行验证脚本**
```bash
mysql -h127.0.0.1 -P9306 < scripts/verify_migration.sql
```

7. **运行单元测试**
```bash
dotnet test
```

8. **手动测试中文搜索**
- 测试单字符查询（应返回空）
- 测试双字符中文查询（应有结果）
- 测试标签搜索功能
- 对比新旧搜索结果差异

## 回滚方案

如果效果不理想：
```bash
git checkout master
# 重新部署原版本
```