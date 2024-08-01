### HappyNotes Telegram Sync Feature Design Document

#### Overview

The Telegram Sync feature for the HappyNotes app allows users to synchronize their notes with a specified Telegram channel. This document outlines the design of the synchronization mechanism, including the `NoteSyncType` enum and the `TelegramSyncSettings` database table.

#### Enum Definition

```csharp
public enum NoteSyncType
{
    Public = 1,  // Sync only public notes (IsPrivate = false)
    Private = 2, // Sync only private notes (IsPrivate = true)
    All = 3,     // Sync all notes
    Tag = 4      // Sync notes with a specific tag (requires SyncValue)
}
```

#### Database Table Definition

The `TelegramSyncSettings` table stores user-specific synchronization settings for the Telegram channel.

```sql
CREATE TABLE `TelegramSyncSettings` (
  `UserId` BIGINT NOT NULL,                          // Unique identifier for the user
  `SyncType` TINYINT NOT NULL,                       // Sync type as defined in NoteSyncType enum
  `SyncValue` VARCHAR(32) NOT NULL DEFAULT '',       // Value associated with the SyncType (e.g., tag text)
  `EncryptedTelegramToken` VARCHAR(128) NOT NULL,    // Encrypted token for Telegram API access
  `TelegramChannelId` VARCHAR(64) DEFAULT NULL,      // ID of the Telegram channel to sync with
  `CreatedAt` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP, // Timestamp when the setting was created
  UNIQUE KEY `UserId` (`UserId`, `SyncType`, `SyncValue`),  // Ensure unique settings per user and sync type/value
  KEY `SyncType` (`SyncType`),                       // Index on SyncType for efficient querying
  KEY `UserId` (`UserId`)                            // Index on UserId for efficient querying
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
```

#### Design Considerations

1. **Sync Type Handling:**
    - **Public:** Sync only notes where `IsPrivate` is `false`.
    - **Private:** Sync only notes where `IsPrivate` is `true`.
    - **All:** Sync all notes regardless of the `IsPrivate` status.
    - **Tag:** Sync notes with a specific tag. The `SyncValue` field holds the tag text.

2. **Data Security:**
    - The `EncryptedTelegramToken` field must be securely handled, ensuring encryption and decryption at the application level.
    - Regular security audits and updates to encryption methods are recommended.

3. **Indexes:**
    - Unique key on `UserId`, `SyncType`, and `SyncValue` ensures no duplicate sync settings for the same user and sync type/value combination.
    - Indexes on `UserId` and `SyncType` improve query performance.

4. **Default Values:**
    - The `SyncValue` defaults to an empty string but must be appropriately populated for `Tag` sync types.

5. **Timestamp:**
    - `CreatedAt` uses the `TIMESTAMP` type for automatic handling of creation times.

#### Error Handling

- Implement robust error handling and logging for all sync operations.
- Validate `TelegramChannelId` to ensure correct format and existence.

#### Documentation and Maintenance

- Provide clear documentation for each field and its usage to assist future developers.
- Maintain the synchronization logic to adapt to any changes in Telegram's API.
