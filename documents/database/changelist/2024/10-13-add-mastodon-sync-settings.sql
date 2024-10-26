USE HappyNotes;
DROP TABLE IF EXISTS MastodonApplication;
CREATE TABLE IF NOT EXISTS MastodonApplication
(
    Id            INT AUTO_INCREMENT PRIMARY KEY,
    InstanceUrl   VARCHAR(255) NOT NULL,
    ApplicationId INT          NOT NULL,
    ClientId      VARCHAR(255) NOT NULL,
    ClientSecret  VARCHAR(255) NOT NULL,
    MaxTootChars  INT          NOT NULL DEFAULT '500',
    Name          VARCHAR(255) NOT NULL,
    Website       VARCHAR(255),
    RedirectUri   VARCHAR(255) NOT NULL,
    Scopes        VARCHAR(255) NOT NULL,
    CreatedAt     BIGINT       NOT NULL,
    UpdatedAt     BIGINT       NOT NULL,
    UNIQUE KEY (InstanceUrl)
);

DROP TABLE IF EXISTS MastodonUserAccount;
CREATE TABLE IF NOT EXISTS MastodonUserAccount
(
    Id             BIGINT AUTO_INCREMENT PRIMARY KEY,
    UserId         BIGINT       NOT NULL,
    InstanceUrl    VARCHAR(255) NOT NULL,
    AccessToken    VARCHAR(255) NOT NULL,
    TokenType      VARCHAR(50)  NOT NULL,
    Scope          VARCHAR(255) NOT NULL,
    Status         INT          NOT NULL COMMENT 'Reference MastodonUserAccountStatus enum for details',
    CreatedAt      BIGINT       NOT NULL,
    UNIQUE KEY (InstanceUrl, UserId),
    INDEX (UserId)
);

CREATE TABLE IF NOT EXISTS MastodonSyncStatusValues
(
    Id     INT PRIMARY KEY,
    Status VARCHAR(20) NOT NULL -- can be 'Pending', 'Synced', or 'Failed'
);

DROP TABLE IF EXISTS MastodonSyncStatus;
CREATE TABLE IF NOT EXISTS MastodonSyncStatus
(
    Id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    InstanceUrl     VARCHAR(255) NOT NULL,
    NoteId          BIGINT       NOT NULL,
    UserId          BIGINT       NOT NULL,
    ApplicationId   INT          NOT NULL,
    TootId          VARCHAR(255),
    SyncStatus      INT          NOT NULL,
    LastSyncAttempt BIGINT,
    ErrorMessage    VARCHAR(1024),
    CreatedAt       BIGINT       NOT NULL,
    UpdatedAt       BIGINT       NOT NULL,
    FOREIGN KEY (SyncStatus) REFERENCES MastodonSyncStatusValues (Id),
    INDEX (NoteId),
    UNIQUE KEY (InstanceUrl, NoteId)
);

-- Check if the 'MastodonTootIds' field exists
SELECT COUNT(*)
INTO @exists
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'HappyNotes'
  AND TABLE_NAME = 'Note'
  AND COLUMN_NAME = 'MastodonTootIds';

-- If the 'MastodonTootIds' field does not exist, add it
SET @sql = IF(@exists = 0,
              'ALTER TABLE `HappyNotes`.`Note` ADD `MastodonTootIds` VARCHAR(512) NULL COMMENT ''Comma-separated ApplicationId:TootId list'' AFTER `TelegramMessageIds`;',
              'SELECT ''Column MastodonTootIds already exists.'' AS Result');

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
