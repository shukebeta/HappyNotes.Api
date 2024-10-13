# Database Design Document: Mastodon Sync Feature for HappyNotes

## 1. Overview

This document outlines the database design for integrating Mastodon synchronization capabilities into the HappyNotes application. The design allows for efficient management of Mastodon applications, user accounts, and note synchronization statuses across multiple Mastodon instances.

## 2. Table Structures

### 2.1 MastodonApplications

Stores information about Mastodon instances and their associated application credentials.

```sql
CREATE TABLE IF NOT EXISTS MastodonApplications (
    Id            INT AUTO_INCREMENT PRIMARY KEY,
    InstanceUrl   VARCHAR(255) NOT NULL,
    ApplicationId VARCHAR(255) NOT NULL,
    ClientId      VARCHAR(255) NOT NULL,
    ClientSecret  VARCHAR(255) NOT NULL,
    MaxTootChars  INT          NOT NULL DEFAULT '500',
    Name          VARCHAR(255) NOT NULL,
    Website       VARCHAR(255),
    RedirectUri   VARCHAR(255) NOT NULL,
    Scopes        VARCHAR(255) NOT NULL,
    CreatedAt     BIGINT       NOT NULL,
    UpdatedAt     BIGINT       NOT NULL,
    UNIQUE KEY (InstanceUrl, ApplicationId)
);
```

### 2.2 MastodonUserAccounts

Manages user-specific Mastodon account information and authentication tokens.

```sql
CREATE TABLE IF NOT EXISTS MastodonUserAccounts (
    Id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    UserId         BIGINT       NOT NULL,
    ApplicationId  INT          NOT NULL,
    MastodonUserId VARCHAR(255) NOT NULL,
    Username       VARCHAR(255) NOT NULL,
    DisplayName    VARCHAR(255),
    AvatarUrl      VARCHAR(255),
    AccessToken    VARCHAR(255) NOT NULL,
    RefreshToken   VARCHAR(255),
    TokenType      VARCHAR(50)  NOT NULL,
    Scope          VARCHAR(255) NOT NULL,
    ExpiresAt      BIGINT,
    CreatedAt      BIGINT       NOT NULL,
    UpdatedAt      BIGINT       NOT NULL,
    UNIQUE KEY (UserId, ApplicationId),
    INDEX (UserId)
);
```

### 2.3 MastodonSyncStatus

Tracks the synchronization status of individual notes to Mastodon.

```sql
CREATE TABLE IF NOT EXISTS MastodonSyncStatus (
    Id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    NoteId          BIGINT NOT NULL,
    UserId          BIGINT NOT NULL,
    ApplicationId   INT    NOT NULL,
    TootId          VARCHAR(255),
    SyncStatus      INT    NOT NULL DEFAULT '0',
    LastSyncAttempt BIGINT,
    ErrorMessage    VARCHAR(1024),
    CreatedAt       BIGINT NOT NULL,
    UpdatedAt       BIGINT NOT NULL,
    FOREIGN KEY (SyncStatus) REFERENCES MastodonSyncStatusValues (Id),
    INDEX(NoteId),
    UNIQUE KEY (ApplicationId, TootId)
);
```

### 2.4 MastodonSyncStatusValues

Defines the possible synchronization status types.

```sql
CREATE TABLE IF NOT EXISTS MastodonSyncStatusValues (
    Id     INT PRIMARY KEY,
    Status VARCHAR(20) NOT NULL -- e.g., 'Pending', 'Synced', 'Failed'
);
```

### 2.5 Note Table Modification

The existing Note table is modified to include Mastodon-specific information:

```sql
ALTER TABLE `HappyNotes`.`Note` 
ADD `MastodonTootIds` VARCHAR(512) NULL 
COMMENT 'Comma-separated ApplicationId:TootId list';
```

## 3. Key Design Decisions

1. **Use of INT for ApplicationId**: Assuming a limited number of Mastodon instances, INT is used for ApplicationId to save space.
2. **Avoiding ENUM**: A separate table (MastodonSyncStatusValues) is used instead of ENUM for flexibility in managing status types.
3. **Indexing**: Strategic indexes are placed on frequently queried fields to optimize performance.
4. **Timestamp Format**: BIGINT is used for timestamp fields, allowing for Unix timestamp storage.
5. **Soft Deletion**: The design supports soft deletion through the existing DeletedAt field in the User table.

## 4. Relationships

- MastodonUserAccounts.UserId references the main User table.
- MastodonUserAccounts.ApplicationId references MastodonApplications.Id.
- MastodonSyncStatus.UserId references the main User table.
- MastodonSyncStatus.ApplicationId references MastodonApplications.Id.
- MastodonSyncStatus.SyncStatus references MastodonSyncStatusValues.Id.
- Note.MastodonTootIds indirectly references MastodonSyncStatus entries.

## 5. Scalability Considerations

- The design avoids using foreign keys in tables expected to have a large number of records to maintain performance at scale.
- Indexes are strategically placed to support common query patterns.
- The use of VARCHAR(512) for MastodonTootIds in the Note table may need monitoring if notes are frequently synced to many instances.

## 6. Security Considerations

- Sensitive data (e.g., ClientSecret, AccessToken) should be encrypted before storage.
- Access to the MastodonApplications and MastodonUserAccounts tables should be strictly controlled.

## 7. Future Enhancements

- Consider implementing a queuing system for managing sync tasks to handle rate limiting.
- Monitor the size of the MastodonTootIds field in the Note table and consider alternative storage methods if it grows too large.
- Implement a system for periodic refresh of access tokens based on ExpiresAt timestamps.

This design provides a robust foundation for integrating Mastodon synchronization into the HappyNotes application while maintaining flexibility for future enhancements and scalability.
