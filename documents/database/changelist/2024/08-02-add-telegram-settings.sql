CREATE TABLE IF NOT EXISTS `HappyNotes`.`TelegramSettings`
(
    `Id`             int          NOT NULL AUTO_INCREMENT,
    `UserId`         bigint       NOT NULL,
    `SyncType`       tinyint      NOT NULL,
    `SyncValue`      varchar(32)  NOT NULL DEFAULT '',
    `EncryptedToken` varchar(128) NOT NULL DEFAULT '',
    `ChannelId`      varchar(64)  NOT NULL DEFAULT '' COMMENT 'Telegram channel ID for syncing',
    `TokenRemark`    varchar(64)           DEFAULT NULL,
    `Status`         tinyint      NOT NULL DEFAULT '1' COMMENT 'See TelegramSettingsStatus enum for details',
    `StatusText`     varchar(1024)         DEFAULT NULL COMMENT 'A description/error message for current Status',
    `CreatedAt`      bigint       NOT NULL DEFAULT '0' COMMENT 'A unix timestamp',
    PRIMARY KEY (`Id`),
    UNIQUE KEY `UserId` (`UserId`, `SyncType`, `SyncValue`),
    KEY `SyncType` (`SyncType`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_0900_ai_ci;

DROP TABLE IF EXISTS HappyNotes.DeletedNote;

ALTER TABLE `HappyNotes`.`Note` CHANGE `CreateAt` `CreatedAt` BIGINT NOT NULL DEFAULT '0' COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`NoteTag` CHANGE `CreateAt` `CreatedAt` BIGINT NOT NULL DEFAULT '0' COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`User` CHANGE `CreateAt` `CreatedAt` BIGINT NOT NULL DEFAULT '0' COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`UserSettings` CHANGE `CreateAt` `CreatedAt` BIGINT NOT NULL DEFAULT '0' COMMENT 'A unix timestamp';

ALTER TABLE `HappyNotes`.`Note` CHANGE `DeleteAt` `DeletedAt` BIGINT NULL COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`User` CHANGE `DeleteAt` `DeletedAt` BIGINT NULL COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`UserSettings` CHANGE `DeleteAt` `DeletedAt` BIGINT NULL COMMENT 'A unix timestamp';

ALTER TABLE `HappyNotes`.`Note` CHANGE `UpdateAt` `UpdatedAt` BIGINT NULL COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`User` CHANGE `UpdateAt` `UpdatedAt` BIGINT NULL COMMENT 'A unix timestamp';
ALTER TABLE `HappyNotes`.`UserSettings` CHANGE `UpdateAt` `UpdatedAt` BIGINT NULL COMMENT 'A unix timestamp';
