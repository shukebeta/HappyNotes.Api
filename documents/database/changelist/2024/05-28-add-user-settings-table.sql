CREATE TABLE `HappyNotes`.`UserSettings`
(
    `Id`           bigint NOT NULL AUTO_INCREMENT,
    `UserId`       bigint        DEFAULT NULL,
    `SettingName`  varchar(255)  DEFAULT NULL,
    `SettingValue` varchar(4096) DEFAULT NULL,
    `CreateAt`     bigint NOT NULL,
    `UpdateAt`     bigint        DEFAULT NULL,
    `DeleteAt`     bigint        DEFAULT NULL,
    PRIMARY KEY (`Id`),
    UNIQUE KEY `SettingName` (`UserId`, `SettingName`),
    KEY `UserId` (`UserId`)
) ENGINE = InnoDB
  DEFAULT CHARSET = utf8mb4
  COLLATE = utf8mb4_0900_ai_ci;
