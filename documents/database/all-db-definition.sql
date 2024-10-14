-- MySQL dump 10.13  Distrib 8.0.25, for Linux (x86_64)
--
-- Host: 127.0.0.1    Database: HappyNotes
-- ------------------------------------------------------
-- Server version	8.0.32

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `HappyNotes`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `HappyNotes` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `HappyNotes`;

--
-- Table structure for table `Files`
--

DROP TABLE IF EXISTS `Files`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Files` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Md5` char(32) NOT NULL,
  `Path` char(20) NOT NULL,
  `FileExt` char(5) NOT NULL,
  `RefCount` int DEFAULT NULL,
  `CreatedAt` bigint DEFAULT NULL,
  `UpdatedAt` bigint DEFAULT NULL,
  `FileName` char(128) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Md5` (`Md5`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LinkedNote`
--

DROP TABLE IF EXISTS `LinkedNote`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `LinkedNote` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `NoteId` bigint NOT NULL,
  `LinkedNoteId` bigint NOT NULL,
  `CreateAt` bigint NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uniq` (`NoteId`,`LinkedNoteId`),
  KEY `linkedNoteId` (`LinkedNoteId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LongNote`
--

DROP TABLE IF EXISTS `LongNote`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `LongNote` (
  `Id` bigint NOT NULL,
  `Content` mediumtext NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MastodonApplication`
--

DROP TABLE IF EXISTS `MastodonApplication`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `MastodonApplication` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InstanceUrl` varchar(255) NOT NULL,
  `ApplicationId` varchar(255) NOT NULL,
  `ClientId` varchar(255) NOT NULL,
  `ClientSecret` varchar(255) NOT NULL,
  `MaxTootChars` int NOT NULL DEFAULT '500',
  `Name` varchar(255) NOT NULL,
  `Website` varchar(255) DEFAULT NULL,
  `RedirectUri` varchar(255) NOT NULL,
  `Scopes` varchar(255) NOT NULL,
  `CreatedAt` bigint NOT NULL,
  `UpdatedAt` bigint NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `InstanceUrl` (`InstanceUrl`,`ApplicationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MastodonSyncStatus`
--

DROP TABLE IF EXISTS `MastodonSyncStatus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `MastodonSyncStatus` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `NoteId` bigint NOT NULL,
  `UserId` bigint NOT NULL,
  `ApplicationId` int NOT NULL,
  `TootId` varchar(255) DEFAULT NULL,
  `SyncStatus` int NOT NULL,
  `LastSyncAttempt` bigint DEFAULT NULL,
  `ErrorMessage` varchar(1024) DEFAULT NULL,
  `CreatedAt` bigint NOT NULL,
  `UpdatedAt` bigint NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `ApplicationId` (`ApplicationId`,`TootId`),
  KEY `SyncStatus` (`SyncStatus`),
  KEY `NoteId` (`NoteId`),
  CONSTRAINT `MastodonSyncStatus_ibfk_1` FOREIGN KEY (`SyncStatus`) REFERENCES `MastodonSyncStatusValues` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MastodonSyncStatusValues`
--

DROP TABLE IF EXISTS `MastodonSyncStatusValues`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `MastodonSyncStatusValues` (
  `Id` int NOT NULL,
  `Status` varchar(20) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MastodonUserAccount`
--

DROP TABLE IF EXISTS `MastodonUserAccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `MastodonUserAccount` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint NOT NULL,
  `ApplicationId` int NOT NULL,
  `MastodonUserId` varchar(255) NOT NULL,
  `InstanceUrl` varchar(255) NOT NULL,
  `Username` varchar(255) NOT NULL,
  `DisplayName` varchar(255) DEFAULT NULL,
  `AvatarUrl` varchar(255) DEFAULT NULL,
  `AccessToken` varchar(255) NOT NULL,
  `RefreshToken` varchar(255) DEFAULT NULL,
  `TokenType` varchar(50) NOT NULL,
  `Scope` varchar(255) NOT NULL,
  `Status` int NOT NULL COMMENT 'Reference MastodonUserAccountStatus enum for details',
  `StatusText` varchar(1024) DEFAULT NULL,
  `ExpiresAt` bigint DEFAULT NULL,
  `CreatedAt` bigint NOT NULL,
  `UpdatedAt` bigint DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserId` (`UserId`,`ApplicationId`),
  KEY `UserId_2` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Note`
--

DROP TABLE IF EXISTS `Note`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Note` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint NOT NULL DEFAULT '0',
  `Content` varchar(1024) NOT NULL,
  `Tags` varchar(512) DEFAULT NULL,
  `TelegramMessageIds` varchar(512) DEFAULT NULL COMMENT 'Common separated telegram MessageId list',
  `MastodonTootIds` varchar(512) DEFAULT NULL COMMENT 'Comma-separated ApplicationId:TootId list',
  `FavoriteCount` int NOT NULL DEFAULT '0',
  `IsLong` tinyint NOT NULL DEFAULT '0',
  `IsPrivate` tinyint NOT NULL DEFAULT '1',
  `IsMarkdown` tinyint NOT NULL DEFAULT '0' COMMENT 'indicate content field is in markdown format or not',
  `CreatedAt` bigint NOT NULL DEFAULT '0' COMMENT 'A unix timestamp',
  `UpdatedAt` bigint DEFAULT NULL COMMENT 'A unix timestamp',
  `DeletedAt` bigint DEFAULT NULL COMMENT 'A unix timestamp',
  PRIMARY KEY (`Id`),
  KEY `idx_FavoriteCount` (`FavoriteCount`),
  KEY `idx_CreateAt` (`CreatedAt`),
  KEY `idx_DeleteAt` (`DeletedAt`),
  KEY `idx_UserId_DeleteAt` (`UserId`,`DeletedAt`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NoteTag`
--

DROP TABLE IF EXISTS `NoteTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `NoteTag` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `UserId` bigint NOT NULL COMMENT '=Note.UserId',
  `NoteId` bigint unsigned NOT NULL,
  `Tag` varchar(32) NOT NULL COMMENT 'Note tag, put #tag1 tag2 tag3 in note content',
  `CreatedAt` bigint NOT NULL DEFAULT '0' COMMENT 'A unix timestamp',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `NoteId` (`NoteId`,`Tag`),
  KEY `idx_TagName` (`Tag`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SequelizeMeta`
--

DROP TABLE IF EXISTS `SequelizeMeta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `SequelizeMeta` (
  `name` varchar(255) COLLATE utf8mb3_unicode_ci NOT NULL,
  PRIMARY KEY (`name`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `TelegramSettings`
--

DROP TABLE IF EXISTS `TelegramSettings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `TelegramSettings` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` bigint NOT NULL,
  `SyncType` tinyint NOT NULL,
  `SyncValue` varchar(32) NOT NULL DEFAULT '',
  `EncryptedToken` varchar(128) NOT NULL DEFAULT '' COMMENT 'Telegram channel ID for syncing',
  `ChannelId` varchar(64) NOT NULL DEFAULT '' COMMENT 'Telegram channel ID for syncing',
  `ChannelName` varchar(64) NOT NULL DEFAULT '',
  `TokenRemark` varchar(64) DEFAULT NULL,
  `Status` tinyint NOT NULL DEFAULT '1' COMMENT 'See TelegramSettingsStatus enum for details',
  `LastError` varchar(1024) DEFAULT NULL,
  `CreatedAt` bigint NOT NULL DEFAULT '0' COMMENT 'A unix timestamp',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserId` (`UserId`,`SyncType`,`SyncValue`),
  KEY `SyncType` (`SyncType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `User`
--

DROP TABLE IF EXISTS `User`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `User` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Username` varchar(20) NOT NULL,
  `Email` varchar(128) NOT NULL DEFAULT '',
  `EmailVerified` tinyint NOT NULL DEFAULT '0',
  `Gravatar` varchar(512) DEFAULT NULL,
  `Password` varchar(64) NOT NULL,
  `Salt` varchar(64) NOT NULL DEFAULT '',
  `CreatedAt` bigint NOT NULL DEFAULT '0' COMMENT 'A unix timestamp',
  `UpdatedAt` bigint DEFAULT NULL COMMENT 'A unix timestamp',
  `DeletedAt` bigint DEFAULT NULL COMMENT 'A unix timestamp',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserName` (`Username`),
  UNIQUE KEY `Email` (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `UserSettings`
--

DROP TABLE IF EXISTS `UserSettings`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `UserSettings` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint DEFAULT NULL,
  `SettingName` varchar(255) DEFAULT NULL,
  `SettingValue` varchar(4096) DEFAULT NULL,
  `CreatedAt` bigint NOT NULL DEFAULT '0' COMMENT 'A unix timestamp',
  `UpdatedAt` bigint DEFAULT NULL COMMENT 'A unix timestamp',
  `DeletedAt` bigint DEFAULT NULL COMMENT 'A unix timestamp',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SettingName` (`UserId`,`SettingName`),
  KEY `UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

