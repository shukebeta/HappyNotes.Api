-- MySQL dump 10.13  Distrib 5.7.30, for Linux (x86_64)
--
-- Host: 192.168.178.52    Database: IDServer
-- ------------------------------------------------------
-- Server version	8.0.19

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `IDServer`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `IDServer` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `IDServer`;

--
-- Table structure for table `ApiClaims`
--

DROP TABLE IF EXISTS `ApiClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiClaims_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `ApiClaims_ibfk_1` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ApiProperties`
--

DROP TABLE IF EXISTS `ApiProperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiProperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiProperties_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `ApiProperties_ibfk_1` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ApiResources`
--

DROP TABLE IF EXISTS `ApiResources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiResources` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DisplayName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Created` datetime(6) NOT NULL,
  `Updated` datetime(6) DEFAULT NULL,
  `LastAccessed` datetime(6) DEFAULT NULL,
  `NonEditable` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ApiResources_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ApiScopeClaims`
--

DROP TABLE IF EXISTS `ApiScopeClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiScopeClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ApiScopeId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiScopeClaims_ApiScopeId` (`ApiScopeId`),
  CONSTRAINT `ApiScopeClaims_ibfk_1` FOREIGN KEY (`ApiScopeId`) REFERENCES `ApiScopes` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ApiScopes`
--

DROP TABLE IF EXISTS `ApiScopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiScopes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DisplayName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Required` tinyint(1) NOT NULL,
  `Emphasize` tinyint(1) NOT NULL,
  `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_ApiScopes_Name` (`Name`),
  KEY `IX_ApiScopes_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `ApiScopes_ibfk_1` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ApiSecrets`
--

DROP TABLE IF EXISTS `ApiSecrets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ApiSecrets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Expiration` datetime(6) DEFAULT NULL,
  `Type` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Created` datetime(6) NOT NULL,
  `ApiResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ApiSecrets_ApiResourceId` (`ApiResourceId`),
  CONSTRAINT `ApiSecrets_ibfk_1` FOREIGN KEY (`ApiResourceId`) REFERENCES `ApiResources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetRoleClaims`
--

DROP TABLE IF EXISTS `AspNetRoleClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetRoleClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RoleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetRoleClaims_RoleId` (`RoleId`),
  CONSTRAINT `AspNetRoleClaims_ibfk_1` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetRoles`
--

DROP TABLE IF EXISTS `AspNetRoles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetRoles` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `NormalizedName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `RoleNameIndex` (`NormalizedName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetUserClaims`
--

DROP TABLE IF EXISTS `AspNetUserClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetUserClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClaimType` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `ClaimValue` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`Id`),
  KEY `IX_AspNetUserClaims_UserId` (`UserId`),
  CONSTRAINT `AspNetUserClaims_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetUserLogins`
--

DROP TABLE IF EXISTS `AspNetUserLogins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetUserLogins` (
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderKey` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProviderDisplayName` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`LoginProvider`,`ProviderKey`),
  KEY `IX_AspNetUserLogins_UserId` (`UserId`),
  CONSTRAINT `AspNetUserLogins_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetUserRoles`
--

DROP TABLE IF EXISTS `AspNetUserRoles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetUserRoles` (
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RoleId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`UserId`,`RoleId`),
  KEY `IX_AspNetUserRoles_RoleId` (`RoleId`),
  CONSTRAINT `AspNetUserRoles_ibfk_1` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `AspNetUserRoles_ibfk_2` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetUserTokens`
--

DROP TABLE IF EXISTS `AspNetUserTokens`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetUserTokens` (
  `UserId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `LoginProvider` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci,
  PRIMARY KEY (`UserId`,`LoginProvider`,`Name`),
  CONSTRAINT `AspNetUserTokens_ibfk_1` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AspNetUsers`
--

DROP TABLE IF EXISTS `AspNetUsers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AspNetUsers` (
  `Id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主键，Sequential GUID',
  `ReferenceId` varchar(255) DEFAULT NULL COMMENT 'such as a memberId, or a shareDataId...',
  `ReferenceType` int DEFAULT NULL COMMENT '引荐人类型：小程序分享Sku等等',
  `UserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '用户名',
  `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '规范后的大写的用户名',
  `Email` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '邮箱',
  `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '规范后的全大写的邮箱',
  `EmailConfirmed` tinyint(1) NOT NULL DEFAULT '0' COMMENT '邮箱确认状态, 0未确认 1已确认',
  `PasswordHash` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '哈希后的密码',
  `SecurityStamp` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '安全戳，GUID类型，账号发生变更时该字段会自动变更',
  `ConcurrencyStamp` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '一致性标记，GUID类型',
  `PhoneNumber` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '电话号码',
  `PhoneNumberConfirmed` tinyint(1) NOT NULL DEFAULT '0' COMMENT '电话号码确认状态, 0未确认 1已确认',
  `TwoFactorEnabled` tinyint(1) NOT NULL COMMENT '两步验证已启用',
  `LockoutEnd` datetime(6) DEFAULT NULL COMMENT '锁定截止时间',
  `LockoutEnabled` tinyint(1) NOT NULL DEFAULT '0' COMMENT '是否可以锁定',
  `AccessFailedCount` int NOT NULL COMMENT '登录失败次数，用来判断是否需要锁定',
  `CountryCode` int DEFAULT NULL COMMENT '移动电话国家代码',
  `Mobile` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '移动电话号码，不含国家代码',
  `CreateAt` datetime(6) DEFAULT NULL COMMENT '用户注册时间，DATETIME UTC',
  `DeleteTime` datetime DEFAULT NULL,
  `ReferenceSourcePath` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '访问路径:web访问时是URL; 小程序访问时是Path',
  `ReferenceSourceParam` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '访问来源带的参数(可能是处理后的path，可能是post来的数据), json形式存储',
  `SourceEventId` int DEFAULT NULL,
  `WechatMiniProAuthed` tinyint(1) DEFAULT '0' COMMENT '微信是否授权，默认未授权',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UserNameIndex` (`NormalizedUserName`),
  UNIQUE KEY `Mobile_CountryCode` (`Mobile`,`CountryCode`),
  UNIQUE KEY `PhoneNumber` (`PhoneNumber`),
  KEY `EmailIndex` (`NormalizedEmail`),
  KEY `CreateAt` (`CreateAt`),
  KEY `ReferenceUserId` (`ReferenceId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='YangtaoABC 用户表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientClaims`
--

DROP TABLE IF EXISTS `ClientClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientClaims_ClientId` (`ClientId`),
  CONSTRAINT `ClientClaims_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientCorsOrigins`
--

DROP TABLE IF EXISTS `ClientCorsOrigins`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientCorsOrigins` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Origin` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientCorsOrigins_ClientId` (`ClientId`),
  CONSTRAINT `ClientCorsOrigins_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientGrantTypes`
--

DROP TABLE IF EXISTS `ClientGrantTypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientGrantTypes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `GrantType` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientGrantTypes_ClientId` (`ClientId`),
  CONSTRAINT `ClientGrantTypes_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientIdPRestrictions`
--

DROP TABLE IF EXISTS `ClientIdPRestrictions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientIdPRestrictions` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Provider` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientIdPRestrictions_ClientId` (`ClientId`),
  CONSTRAINT `ClientIdPRestrictions_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientPostLogoutRedirectUris`
--

DROP TABLE IF EXISTS `ClientPostLogoutRedirectUris`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientPostLogoutRedirectUris` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `PostLogoutRedirectUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientPostLogoutRedirectUris_ClientId` (`ClientId`),
  CONSTRAINT `ClientPostLogoutRedirectUris_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientProperties`
--

DROP TABLE IF EXISTS `ClientProperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientProperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientProperties_ClientId` (`ClientId`),
  CONSTRAINT `ClientProperties_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientRedirectUris`
--

DROP TABLE IF EXISTS `ClientRedirectUris`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientRedirectUris` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `RedirectUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientRedirectUris_ClientId` (`ClientId`),
  CONSTRAINT `ClientRedirectUris_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientScopes`
--

DROP TABLE IF EXISTS `ClientScopes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientScopes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Scope` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientScopes_ClientId` (`ClientId`),
  CONSTRAINT `ClientScopes_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ClientSecrets`
--

DROP TABLE IF EXISTS `ClientSecrets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ClientSecrets` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Description` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Value` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Expiration` datetime(6) DEFAULT NULL,
  `Type` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Created` datetime(6) NOT NULL,
  `ClientId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_ClientSecrets_ClientId` (`ClientId`),
  CONSTRAINT `ClientSecrets_ibfk_1` FOREIGN KEY (`ClientId`) REFERENCES `Clients` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Clients`
--

DROP TABLE IF EXISTS `Clients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Clients` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `ClientId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProtocolType` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RequireClientSecret` tinyint(1) NOT NULL,
  `ClientName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClientUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `LogoUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `RequireConsent` tinyint(1) NOT NULL,
  `AllowRememberConsent` tinyint(1) NOT NULL,
  `AlwaysIncludeUserClaimsInIdToken` tinyint(1) NOT NULL,
  `RequirePkce` tinyint(1) NOT NULL,
  `AllowPlainTextPkce` tinyint(1) NOT NULL,
  `AllowAccessTokensViaBrowser` tinyint(1) NOT NULL,
  `FrontChannelLogoutUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `FrontChannelLogoutSessionRequired` tinyint(1) NOT NULL,
  `BackChannelLogoutUri` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `BackChannelLogoutSessionRequired` tinyint(1) NOT NULL,
  `AllowOfflineAccess` tinyint(1) NOT NULL,
  `IdentityTokenLifetime` int NOT NULL,
  `AccessTokenLifetime` int NOT NULL,
  `AuthorizationCodeLifetime` int NOT NULL,
  `ConsentLifetime` int DEFAULT NULL,
  `AbsoluteRefreshTokenLifetime` int NOT NULL,
  `SlidingRefreshTokenLifetime` int NOT NULL,
  `RefreshTokenUsage` int NOT NULL,
  `UpdateAccessTokenClaimsOnRefresh` tinyint(1) NOT NULL,
  `RefreshTokenExpiration` int NOT NULL,
  `AccessTokenType` int NOT NULL,
  `EnableLocalLogin` tinyint(1) NOT NULL,
  `IncludeJwtId` tinyint(1) NOT NULL,
  `AlwaysSendClientClaims` tinyint(1) NOT NULL,
  `ClientClaimsPrefix` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `PairWiseSubjectSalt` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Created` datetime(6) NOT NULL,
  `Updated` datetime(6) DEFAULT NULL,
  `LastAccessed` datetime(6) DEFAULT NULL,
  `UserSsoLifetime` int DEFAULT NULL,
  `UserCodeType` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `DeviceCodeLifetime` int NOT NULL,
  `NonEditable` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Clients_ClientId` (`ClientId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DeviceCodes`
--

DROP TABLE IF EXISTS `DeviceCodes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DeviceCodes` (
  `UserCode` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DeviceCode` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SubjectId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClientId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreationTime` datetime(6) NOT NULL,
  `Expiration` datetime(6) NOT NULL,
  `Data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`UserCode`),
  UNIQUE KEY `IX_DeviceCodes_DeviceCode` (`DeviceCode`),
  KEY `IX_DeviceCodes_Expiration` (`Expiration`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `IdentityClaims`
--

DROP TABLE IF EXISTS `IdentityClaims`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `IdentityClaims` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Type` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IdentityResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_IdentityClaims_IdentityResourceId` (`IdentityResourceId`),
  CONSTRAINT `IdentityClaims_ibfk_1` FOREIGN KEY (`IdentityResourceId`) REFERENCES `IdentityResources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `IdentityProperties`
--

DROP TABLE IF EXISTS `IdentityProperties`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `IdentityProperties` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Key` varchar(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Value` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `IdentityResourceId` int NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_IdentityProperties_IdentityResourceId` (`IdentityResourceId`),
  CONSTRAINT `IdentityProperties_ibfk_1` FOREIGN KEY (`IdentityResourceId`) REFERENCES `IdentityResources` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `IdentityResources`
--

DROP TABLE IF EXISTS `IdentityResources`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `IdentityResources` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Enabled` tinyint(1) NOT NULL,
  `Name` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `DisplayName` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Required` tinyint(1) NOT NULL,
  `Emphasize` tinyint(1) NOT NULL,
  `ShowInDiscoveryDocument` tinyint(1) NOT NULL,
  `Created` datetime(6) NOT NULL,
  `Updated` datetime(6) DEFAULT NULL,
  `NonEditable` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_IdentityResources_Name` (`Name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MemberBaseInfo`
--

DROP TABLE IF EXISTS `MemberBaseInfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MemberBaseInfo` (
  `MemberId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '主键，Sequential GUID',
  `TimezoneName` varchar(64) DEFAULT NULL,
  `Nickname` varchar(50) NOT NULL DEFAULT '' COMMENT '会员昵称',
  `ProfilePic` varchar(256) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '头像URI',
  `RealName` varchar(255) DEFAULT NULL COMMENT '真实姓名',
  `IdCardNumber` varchar(255) DEFAULT NULL COMMENT '身份证号',
  `IdCardNumberVerified` tinyint DEFAULT NULL COMMENT '是否已通过实名验证',
  `Birthday` datetime(6) DEFAULT NULL COMMENT '生日',
  `Gender` int DEFAULT NULL COMMENT '性别 1 男 2 女 3 保密',
  `Country` varchar(50) DEFAULT NULL COMMENT 'Member’s Country',
  `Province` varchar(50) DEFAULT NULL COMMENT 'Member’s Province',
  `City` varchar(50) DEFAULT NULL COMMENT 'Member’s City',
  `LoginIp` varchar(128) DEFAULT NULL COMMENT '登录IP, 例如 192.168.178.178',
  `Longitude` decimal(11,8) DEFAULT NULL COMMENT '经度',
  `Latitude` decimal(10,8) DEFAULT NULL COMMENT '纬度',
  `LoginTime` datetime DEFAULT NULL,
  PRIMARY KEY (`MemberId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='杨桃会员Base信息表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Numbers`
--

DROP TABLE IF EXISTS `Numbers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Numbers` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrderNumber` int DEFAULT NULL,
  `UserNumber` int DEFAULT NULL,
  `SkuNumber` int DEFAULT NULL,
  `CreateDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PersistedGrants`
--

DROP TABLE IF EXISTS `PersistedGrants`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PersistedGrants` (
  `Key` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Type` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `SubjectId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClientId` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `CreationTime` datetime(6) NOT NULL,
  `Expiration` datetime(6) DEFAULT NULL,
  `Data` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Key`),
  KEY `IX_PersistedGrants_Expiration` (`Expiration`),
  KEY `IX_PersistedGrants_SubjectId_ClientId_Type` (`SubjectId`,`ClientId`,`Type`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SourceChannel`
--

DROP TABLE IF EXISTS `SourceChannel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SourceChannel` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Platform` varchar(255) NOT NULL COMMENT 'Platform Enum',
  `ChannelName` varchar(255) NOT NULL,
  `ChannelNo` varchar(255) DEFAULT NULL COMMENT 'Channal''s number on the source platform',
  `ChannelKey` varchar(255) DEFAULT NULL COMMENT 'Channel''s key name on the source of platform',
  `CreateTime` datetime DEFAULT NULL,
  `Creator` bigint DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SourceEvent`
--

DROP TABLE IF EXISTS `SourceEvent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SourceEvent` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `EventName` varchar(255) NOT NULL,
  `EventKey` varchar(255) DEFAULT NULL,
  `EventValue` varchar(255) DEFAULT NULL,
  `Platform` int DEFAULT NULL,
  `ChannelId` int DEFAULT NULL,
  `ChannelName` varchar(255) DEFAULT NULL,
  `CreateTime` datetime DEFAULT NULL,
  `Creator` bigint DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ThirdLogin`
--

DROP TABLE IF EXISTS `ThirdLogin`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ThirdLogin` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(255) NOT NULL DEFAULT '',
  `ThirdUserId` varchar(128) NOT NULL DEFAULT '',
  `WeChatUnionId` varchar(128) NOT NULL DEFAULT '' COMMENT '开放平台唯一ID（unionid），目前只有徾信登录会用到',
  `AccessToken` varchar(768) NOT NULL DEFAULT '',
  `Scope` varchar(128) DEFAULT NULL COMMENT 'accessToken 对应的scope',
  `RefreshToken` varchar(768) DEFAULT NULL,
  `SessionKey` varchar(255) DEFAULT NULL COMMENT '仅微信小程序会用到',
  `LoginProvider` int NOT NULL DEFAULT '0' COMMENT '一个枚举, 记录用户来源',
  `AccessTokenExpire` int unsigned NOT NULL DEFAULT '0' COMMENT 'access token过期时间',
  `RefreshTokenExpire` int unsigned NOT NULL DEFAULT '0' COMMENT 'refresh token过期时间',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `ThirdUserId` (`ThirdUserId`,`LoginProvider`),
  UNIQUE KEY `MemberId` (`MemberId`,`LoginProvider`),
  KEY `LoginProvider` (`LoginProvider`),
  KEY `WeChatUnionId` (`WeChatUnionId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `__EFMigrationsHistory`
--

DROP TABLE IF EXISTS `__EFMigrationsHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `__EFMigrationsHistory` (
  `MigrationId` varchar(95) NOT NULL,
  `ProductVersion` varchar(32) NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `thirdLoginBackup`
--

DROP TABLE IF EXISTS `thirdLoginBackup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `thirdLoginBackup` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(255) NOT NULL DEFAULT '',
  `ThirdUserId` varchar(128) NOT NULL DEFAULT '',
  `WeChatUnionId` varchar(128) NOT NULL DEFAULT '' COMMENT '开放平台唯一ID（unionid），目前只有徾信登录会用到',
  `AccessToken` varchar(768) NOT NULL DEFAULT '',
  `Scope` varchar(128) DEFAULT NULL COMMENT 'accessToken 对应的scope',
  `RefreshToken` varchar(768) DEFAULT NULL,
  `SessionKey` varchar(255) DEFAULT NULL COMMENT '仅微信小程序会用到',
  `LoginProvider` int NOT NULL DEFAULT '0' COMMENT '一个枚举, 记录用户来源',
  `AccessTokenExpire` int unsigned NOT NULL DEFAULT '0' COMMENT 'access token过期时间',
  `RefreshTokenExpire` int unsigned NOT NULL DEFAULT '0' COMMENT 'refresh token过期时间',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MemberId` (`MemberId`,`LoginProvider`),
  KEY `LoginProvider` (`LoginProvider`),
  KEY `ThirdUserId` (`ThirdUserId`),
  KEY `WeChatUnionId` (`WeChatUnionId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Current Database: `YangtaoStandard`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `YangtaoStandard` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `YangtaoStandard`;

--
-- Table structure for table `Banners`
--

DROP TABLE IF EXISTS `Banners`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Banners` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `Name` varchar(32) NOT NULL COMMENT '名称',
  `TitleCn` varchar(128) DEFAULT NULL COMMENT 'Banner展示中文文字',
  `TitleEn` varchar(128) DEFAULT NULL COMMENT 'Banner展示英文文字',
  `TargetUrl` varchar(255) DEFAULT NULL COMMENT '跳转Url 可为空，不跳转',
  `WechatTargetUrl` varchar(255) DEFAULT 'Null' COMMENT 'Target url for wechat mini program',
  `ImgUrl` varchar(255) NOT NULL COMMENT '图片路径',
  `ImgSize` varchar(32) DEFAULT NULL COMMENT '图片大小规格 宽高逗号分隔(width,height):800,600',
  `BannerLocation` varchar(32) DEFAULT NULL COMMENT 'Banner展示位置: Homepage 或 store 或...',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺Id 商铺Banner需记录商铺Id',
  `Status` int DEFAULT '0' COMMENT '状态 0，隐藏;1,显示',
  `Sort` int NOT NULL DEFAULT '100' COMMENT '排序',
  `CreateBy` int DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '更新时间',
  `Channel` int NOT NULL DEFAULT '1' COMMENT 'this field indicate which channel works for ; 1: web; 2: mini program',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Banner表 首页或者其他页的Banner信息';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CMSArticle`
--

DROP TABLE IF EXISTS `CMSArticle`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CMSArticle` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint NOT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `Author` varchar(500) DEFAULT NULL,
  `Content` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '内容',
  `Title` varchar(1024) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Sort` int NOT NULL COMMENT 'This field indicates the order of the list display in home page',
  `ReferenceLink` varchar(1024) DEFAULT NULL COMMENT 'if it is not a self manage article, use this link to reference Other article.',
  `ArticleCategory` int NOT NULL DEFAULT '1' COMMENT 'this field indicate which category this article belongs to ',
  `IsRecommended` bit(1) NOT NULL DEFAULT b'0' COMMENT 'this field indicate whetcher this artical is listed in recommended list',
  `CMSImage` varchar(500) NOT NULL COMMENT 'this field persist the image display on the article list',
  `Brief` varchar(2000) DEFAULT NULL,
  `IsHot` bit(1) NOT NULL DEFAULT b'0',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='文章';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CMSBanner`
--

DROP TABLE IF EXISTS `CMSBanner`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CMSBanner` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `IdFile` bigint DEFAULT NULL COMMENT 'banner图id',
  `Title` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '标题',
  `Type` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '类型',
  `Url` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '点击banner跳转到url',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='文章';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CMSChannel`
--

DROP TABLE IF EXISTS `CMSChannel`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CMSChannel` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Code` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '编码',
  `Name` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '名称',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='文章栏目';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CMSContacts`
--

DROP TABLE IF EXISTS `CMSContacts`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CMSContacts` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Email` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '电子邮箱',
  `Mobile` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '联系电话',
  `Remark` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '备注',
  `UserName` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '邀约人名称',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='邀约信息';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ChinaProvinceCity`
--

DROP TABLE IF EXISTS `ChinaProvinceCity`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ChinaProvinceCity` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT COMMENT '主键，自增Id',
  `Code` bigint NOT NULL COMMENT '行政区划码',
  `Name` varchar(32) DEFAULT NULL COMMENT '名称',
  `Province` varchar(32) DEFAULT NULL COMMENT '省/直辖市',
  `City` varchar(32) DEFAULT NULL COMMENT '市',
  `Area` varchar(32) DEFAULT NULL COMMENT '区',
  `Town` varchar(32) DEFAULT NULL COMMENT '城镇地区',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ChosenBrand`
--

DROP TABLE IF EXISTS `ChosenBrand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ChosenBrand` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `BrandId` int NOT NULL,
  `CustomLogo` varchar(255) DEFAULT NULL,
  `CustomImage` varchar(255) DEFAULT NULL,
  `CustomBanner` varchar(255) DEFAULT NULL,
  `SkuId` varchar(255) DEFAULT NULL,
  `Sort` int NOT NULL DEFAULT '100',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  `UpdateTime` datetime NOT NULL COMMENT 'a utc time',
  `CreateBy` bigint NOT NULL COMMENT 'Admin Id',
  `UpdateBy` bigint NOT NULL COMMENT 'Admin Id',
  `DeleteTime` datetime DEFAULT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `BrandId` (`BrandId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ChosenBrandSkus`
--

DROP TABLE IF EXISTS `ChosenBrandSkus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ChosenBrandSkus` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `BrandId` int NOT NULL,
  `SkuId` varchar(255) NOT NULL,
  `Sort` int NOT NULL DEFAULT '100',
  `IsRecommended` int NOT NULL DEFAULT '0' COMMENT 'for homepage',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  `UpdateTime` datetime NOT NULL COMMENT 'a utc time',
  `CreateBy` bigint NOT NULL COMMENT 'Admin Id',
  `UpdateBy` bigint NOT NULL COMMENT 'Admin Id',
  `DeleteTime` datetime DEFAULT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `BrandId` (`BrandId`,`SkuId`),
  KEY `BrandId_2` (`BrandId`,`IsRecommended`),
  KEY `Sort` (`Sort`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ChosenStore`
--

DROP TABLE IF EXISTS `ChosenStore`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ChosenStore` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `StoreId` bigint NOT NULL,
  `Sort` int NOT NULL DEFAULT '100',
  `IsRecommended` int NOT NULL DEFAULT '0' COMMENT 'for homepage',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  `UpdateTime` datetime NOT NULL COMMENT 'a utc time',
  `CreateBy` bigint NOT NULL COMMENT 'Admin Id',
  `UpdateBy` bigint NOT NULL COMMENT 'Admin Id',
  `DeleteTime` datetime DEFAULT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `StoreId` (`StoreId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ChosenStoreSkus`
--

DROP TABLE IF EXISTS `ChosenStoreSkus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ChosenStoreSkus` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `StoreId` bigint NOT NULL,
  `SkuId` varchar(255) NOT NULL,
  `Sort` int NOT NULL DEFAULT '100',
  `IsRecommended` int NOT NULL DEFAULT '0' COMMENT 'for homepage',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  `UpdateTime` datetime NOT NULL COMMENT 'a utc time',
  `CreateBy` bigint NOT NULL COMMENT 'Admin Id',
  `UpdateBy` bigint NOT NULL COMMENT 'Admin Id',
  `DeleteTime` datetime DEFAULT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `StoreId` (`StoreId`,`SkuId`),
  KEY `StoreId_2` (`StoreId`,`IsRecommended`),
  KEY `Sort` (`Sort`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CityList`
--

DROP TABLE IF EXISTS `CityList`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CityList` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Code` varchar(50) NOT NULL,
  `CountryCode` varchar(10) NOT NULL,
  `CityName` varchar(100) NOT NULL,
  `CityNameCN` varchar(100) NOT NULL,
  `PCode` varchar(50) DEFAULT NULL COMMENT 'a reference to Code to fomat a city tree',
  `CreateBy` bigint DEFAULT NULL COMMENT 'Creator: admin id',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateBy` bigint DEFAULT NULL COMMENT 'Modifier: admin id',
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Code` (`Code`,`CountryCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CollectionPoint`
--

DROP TABLE IF EXISTS `CollectionPoint`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CollectionPoint` (
  `Id` varchar(255) NOT NULL,
  `CPName` varchar(255) NOT NULL COMMENT 'collection Point Name;\n',
  `CPAddress` varchar(255) NOT NULL COMMENT 'collection point address',
  `CPCity` varchar(100) NOT NULL COMMENT 'the city where collection point located in',
  `ContactName` varchar(50) DEFAULT NULL COMMENT 'contact name',
  `ContactPhone` varchar(20) DEFAULT NULL COMMENT 'contact phone NO',
  `Description` varchar(255) DEFAULT NULL COMMENT 'other description about the collection point',
  `CPGroupCode` varchar(20) DEFAULT NULL COMMENT 'collection point group code',
  `CreateBy` bigint NOT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `IsActive` bit(1) NOT NULL DEFAULT b'1' COMMENT 'this collection point is active or not',
  `Price` decimal(8,2) NOT NULL DEFAULT '0.00' COMMENT 'the unit price for each parcel',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CollectionPointOperator`
--

DROP TABLE IF EXISTS `CollectionPointOperator`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CollectionPointOperator` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `CPId` varchar(255) NOT NULL COMMENT 'collection point id',
  `CPGroupKey` varchar(255) DEFAULT NULL COMMENT 'collection point group key',
  `LoginUserName` varchar(255) NOT NULL,
  `LoginPassword` varchar(255) NOT NULL,
  `Salt` varchar(20) NOT NULL,
  `Role` varchar(50) NOT NULL COMMENT 'current only support operator; admin; two roles',
  `IsActive` bit(1) NOT NULL DEFAULT b'1',
  `DoesInitial` bit(1) NOT NULL COMMENT 'This means this user hasn''t reset his password after first created or reset by admin',
  `DisplayName` varchar(100) DEFAULT NULL,
  `Mobile` varchar(50) DEFAULT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Email` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  UNIQUE KEY `UserName_Unique_2` (`LoginUserName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CountryList`
--

DROP TABLE IF EXISTS `CountryList`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CountryList` (
  `Code` varchar(10) NOT NULL COMMENT '国家数字代码',
  `EnCode` varchar(10) NOT NULL COMMENT '国家英文代码',
  `CnShortName` varchar(50) NOT NULL COMMENT '中文简名',
  `CnFullName` varchar(64) NOT NULL DEFAULT '' COMMENT '中文全名',
  `AreaCode` int NOT NULL COMMENT '国家电话代码',
  `EnShortName` varchar(128) NOT NULL COMMENT '英文简名',
  `EnFullName` varchar(128) NOT NULL DEFAULT '' COMMENT '英文全名',
  PRIMARY KEY (`Code`),
  UNIQUE KEY `EnCode` (`EnCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LeftNavigation`
--

DROP TABLE IF EXISTS `LeftNavigation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LeftNavigation` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `HeaderLinkIDs` varchar(500) NOT NULL COMMENT 'Using comma to split two links Id,',
  `BrandLinkIDs` varchar(500) DEFAULT NULL COMMENT 'Using comma to split two links Id,',
  `CategoryLinkIDs` varchar(500) DEFAULT NULL COMMENT 'Using comma to split two links Id,',
  `AdImageLinkIDs` varchar(500) DEFAULT NULL COMMENT 'Using comma to split two links Id,',
  `CreateTime` datetime NOT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `RecommendationLinkIDs` varchar(500) DEFAULT NULL COMMENT 'Recommendation link Ids',
  `Channel` int NOT NULL,
  `Sort` int NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='The navigation items for the de';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LeftSubLink`
--

DROP TABLE IF EXISTS `LeftSubLink`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LeftSubLink` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `ParentLinkID` bigint NOT NULL COMMENT 'The category link ID',
  `ParentCategoryLinkID` bigint NOT NULL,
  `SubLinkIDs` varchar(500) DEFAULT NULL COMMENT 'sub link IDs, split with comma',
  `CreateTime` datetime NOT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MainNavigation`
--

DROP TABLE IF EXISTS `MainNavigation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MainNavigation` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `ParentId` bigint NOT NULL DEFAULT '0' COMMENT '上级ID, 根导航为0',
  `IdPath` varchar(128) NOT NULL COMMENT 'ID路径 从左到右代表层级关系，短横线分隔：0-1-22',
  `TitleCn` varchar(32) NOT NULL COMMENT '中文标题',
  `TitleEn` varchar(32) NOT NULL COMMENT '英文标题',
  `Url` varchar(255) DEFAULT NULL COMMENT '链接',
  `Target` varchar(32) DEFAULT NULL COMMENT '打开方式 _blank或者当前页面',
  `Image` varchar(255) DEFAULT NULL COMMENT '展示图片',
  `FontIcon` varchar(32) DEFAULT NULL COMMENT '字体图标',
  `Sort` int DEFAULT NULL COMMENT '排序 200',
  `Status` int DEFAULT '0' COMMENT '状态 0，隐藏;1,显示',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `CreateBy` int DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `ModifyBy` bigint DEFAULT NULL COMMENT '更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='站点主导航';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MemberExtension`
--

DROP TABLE IF EXISTS `MemberExtension`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MemberExtension` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(100) NOT NULL,
  `MemberKey` varchar(100) NOT NULL,
  `ExtensionValue` varchar(255) NOT NULL,
  `Desc` varchar(255) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `CreateBy` varchar(100) NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` varchar(100) NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ID_UNIQUE` (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='an extension table to mark member related information';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MemberLocationHistory`
--

DROP TABLE IF EXISTS `MemberLocationHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MemberLocationHistory` (
  `Id` bigint unsigned NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(255) NOT NULL COMMENT '用户Id',
  `LoginIp` varchar(128) NOT NULL COMMENT '登录IP, 例如 192.168.178.178',
  `Longitude` decimal(11,8) DEFAULT NULL COMMENT '经度',
  `Latitude` decimal(10,8) NOT NULL COMMENT '纬度',
  `CreateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `MemberId` (`MemberId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MemberShippingAddress`
--

DROP TABLE IF EXISTS `MemberShippingAddress`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MemberShippingAddress` (
  `Id` int unsigned NOT NULL AUTO_INCREMENT COMMENT '主键，自增Id',
  `MemberId` varchar(256) NOT NULL COMMENT '用户Id',
  `Receiver` varchar(50) DEFAULT NULL COMMENT '收件人姓名',
  `Label` varchar(20) DEFAULT NULL COMMENT 'Address label, such as Friend',
  `IsDefault` tinyint NOT NULL DEFAULT '0' COMMENT 'Default address flag',
  `CountryCode` int DEFAULT NULL COMMENT '国家代码',
  `LandLine` varchar(50) DEFAULT NULL COMMENT '固话号码',
  `Mobile` varchar(50) DEFAULT NULL COMMENT '移动电话号码',
  `Province` varchar(100) DEFAULT NULL COMMENT '省/直辖市',
  `City` varchar(100) DEFAULT NULL COMMENT '地级市',
  `Area` varchar(100) DEFAULT NULL COMMENT '区/县',
  `Town` varchar(100) DEFAULT NULL COMMENT '乡镇/街道办事处',
  `Address` varchar(500) DEFAULT NULL COMMENT '具体地址:某路某号',
  `PostCode` varchar(20) DEFAULT NULL COMMENT '邮政编码',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `LastUsedTime` datetime DEFAULT NULL,
  `IdNO` varchar(100) DEFAULT NULL COMMENT 'chinese national identity card NO',
  `StartDate` datetime DEFAULT NULL COMMENT 'chinese national id card expire date start date',
  `EndDate` datetime DEFAULT NULL COMMENT 'chinese national id card expire date end date',
  `IdFrontPhoto` varchar(255) DEFAULT NULL COMMENT 'chinese national id card front photo',
  `IdBackPhoto` varchar(255) DEFAULT NULL COMMENT 'chinese national id card back photo',
  `DeleteTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `LastUsedTime` (`LastUsedTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Message`
--

DROP TABLE IF EXISTS `Message`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Message` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Content` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '消息内容',
  `Receiver` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '接收者',
  `State` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '消息类型,0:初始,1:成功,2:失败',
  `TplCode` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '模板编码',
  `Type` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '消息类型,0:短信,1:邮件',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='历史消息';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MessageSender`
--

DROP TABLE IF EXISTS `MessageSender`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MessageSender` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `ClassName` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '发送类',
  `Name` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '名称',
  `TplCode` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '短信运营商模板编号',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='消息发送者';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MessageTemplate`
--

DROP TABLE IF EXISTS `MessageTemplate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MessageTemplate` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Code` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '编号',
  `Cond` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '发送条件',
  `Content` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '内容',
  `IDMessageSender` bigint NOT NULL COMMENT '发送者id',
  `Title` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '标题',
  `Type` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '消息类型,0:短信,1:邮件',
  PRIMARY KEY (`ID`) USING BTREE,
  KEY `FK942sadqk5x9kbrwil0psyek3n` (`IDMessageSender`) USING BTREE,
  CONSTRAINT `FK942sadqk5x9kbrwil0psyek3n` FOREIGN KEY (`IDMessageSender`) REFERENCES `MessageSender` (`ID`) ON DELETE RESTRICT ON UPDATE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='消息模板';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `RecommendSkus`
--

DROP TABLE IF EXISTS `RecommendSkus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `RecommendSkus` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `SkuId` varchar(255) NOT NULL COMMENT 'Sku编号',
  `RecommendPosition` int NOT NULL COMMENT '推荐位置 0,首页分类推荐;1,首页人气推荐;(先定义成枚举，后续考虑改成配置管理)',
  `Type` int NOT NULL COMMENT '推荐类型 0,平台推荐;1,商家推荐;',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺Id 商铺推荐时需记录商铺Id',
  `Status` int DEFAULT '0' COMMENT '状态 0,停用;1,启用;',
  `CategoryId` int DEFAULT NULL COMMENT 'Sku所属类目',
  `CategoryGroupId` int DEFAULT NULL COMMENT '推荐类目组 如果是首页分类推荐，则对应首页推荐的类目组',
  `Sort` int DEFAULT '100' COMMENT '排序',
  `StoreManagerId` varchar(255) DEFAULT NULL COMMENT '店铺管理员Id，店铺推荐时记录店铺操作人员Id',
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `ModifyTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SkuId` (`SkuId`,`CategoryGroupId`),
  KEY `CategoryGroupId` (`CategoryGroupId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='推荐商品 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `RecommendationTheme`
--

DROP TABLE IF EXISTS `RecommendationTheme`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `RecommendationTheme` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `ThemeName` varchar(100) NOT NULL,
  `ThemeType` varchar(50) NOT NULL,
  `ThemeDescription` varchar(200) DEFAULT NULL,
  `UpdateBy` bigint NOT NULL,
  `CreateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `WebBanner` varchar(500) DEFAULT NULL,
  `MobileBanner` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `ThemeType_UNIQUE` (`ThemeType`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `RecommendationThemeItems`
--

DROP TABLE IF EXISTS `RecommendationThemeItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `RecommendationThemeItems` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `ThemeID` bigint NOT NULL,
  `SkuID` varchar(100) NOT NULL,
  `RecommendToHomePage` bit(1) NOT NULL DEFAULT b'0',
  `UpdateBy` bigint NOT NULL,
  `CreateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `Sort` int NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ScoreTasks`
--

DROP TABLE IF EXISTS `ScoreTasks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ScoreTasks` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `Channel` varchar(20) NOT NULL COMMENT '// indicate which plateform the task assigned to, 1: web; 2: mini program',
  `TaskName` varchar(100) NOT NULL,
  `TaskNameEn` varchar(100) NOT NULL,
  `TaskDesc` varchar(255) DEFAULT NULL,
  `TaskDescEn` varchar(255) DEFAULT NULL,
  `TaskCode` varchar(50) NOT NULL,
  `NavigationUri` varchar(255) DEFAULT NULL,
  `IconUrl` varchar(255) DEFAULT NULL,
  `TaskType` tinyint NOT NULL COMMENT '1: one time task; 2: daily task; 3: long term task',
  `DailyThresthold` int DEFAULT NULL COMMENT 'to indicate how many time need to complete a day',
  `BtnText` varchar(50) NOT NULL,
  `InActiveBtnText` varchar(50) DEFAULT NULL,
  `ScoreItemType` varchar(50) NOT NULL,
  `CreateTime` datetime NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Sort` int NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `TaskCode_UNIQUE` (`TaskCode`,`Channel`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Score Task';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SequelizeMeta`
--

DROP TABLE IF EXISTS `SequelizeMeta`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SequelizeMeta` (
  `name` varchar(255) CHARACTER SET utf8 COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`name`),
  UNIQUE KEY `name` (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ShareData`
--

DROP TABLE IF EXISTS `ShareData`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ShareData` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Channel` tinyint NOT NULL DEFAULT '1' COMMENT 'target platform, 1: wechat miniP, 2. TikTok..',
  `SharerMemberId` varchar(255) DEFAULT NULL COMMENT '分享者Id, 对标IDServer.AspNetUsers表中的ReferenceUserId',
  `SkuId` varchar(255) DEFAULT NULL,
  `StoreId` bigint DEFAULT NULL,
  `ShareCode` varchar(200) DEFAULT NULL COMMENT '区分',
  `ShareUrl` varchar(1024) DEFAULT NULL COMMENT 'target url',
  `VisitCount` bigint NOT NULL DEFAULT '0',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT 'Creator, can be a customer or a platform manager',
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `ActionName` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id` (`Id`),
  UNIQUE KEY `SkuId` (`SkuId`,`SharerMemberId`),
  UNIQUE KEY `ShareCode` (`ShareCode`),
  KEY `VisitCount` (`VisitCount`),
  KEY `SharerMemberId` (`SharerMemberId`),
  KEY `ActionName` (`ActionName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='YTABC QRCode/ShareURL meta information';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SharePostTemplate`
--

DROP TABLE IF EXISTS `SharePostTemplate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SharePostTemplate` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `PhotoPath` varchar(500) NOT NULL,
  `Description` varchar(200) DEFAULT NULL,
  `Sort` int NOT NULL COMMENT 'the smaller ones on the top',
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `ShareTypes` varchar(200) NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SmsLog`
--

DROP TABLE IF EXISTS `SmsLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SmsLog` (
  `MsgId` bigint NOT NULL AUTO_INCREMENT,
  `CountryCode` int NOT NULL DEFAULT '86',
  `Mobile` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT 'full mobile number with country code',
  `Content` varchar(1024) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT 'message context without signature',
  `SentTime` datetime DEFAULT NULL COMMENT 'sent time stamp',
  `MsgOrderId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'remote message id',
  `ResultCode` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '0' COMMENT 'success: 000000 ',
  `ResultData` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '0' COMMENT 'such as success',
  `ResultTime` bigint unsigned NOT NULL DEFAULT '0' COMMENT 'result time stamp',
  `CallbackTime` datetime DEFAULT NULL,
  `SmsSentTime` bigint DEFAULT NULL,
  `UserId` varchar(64) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT 'associated User Id: a guid',
  `SmsType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL DEFAULT '' COMMENT 'such as VerifyCode, Notification,...',
  `RemoteIpAddr` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `VerifyCodeID` bigint DEFAULT NULL COMMENT 'Verify code record id',
  `Delivered` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `RequestInfo` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ResponseInfo` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CallbackInfo` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  PRIMARY KEY (`MsgId`) USING BTREE,
  KEY `UserId` (`UserId`) USING BTREE,
  KEY `Mobile` (`Mobile`) USING BTREE,
  KEY `ResultCode` (`ResultCode`) USING BTREE,
  KEY `SentTime` (`SentTime`) USING BTREE,
  KEY `SmsType` (`SmsType`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysCfg`
--

DROP TABLE IF EXISTS `SysCfg`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysCfg` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `CfgDesc` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '备注',
  `CfgName` varchar(256) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '参数名',
  `CfgValue` text CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci COMMENT 'the parameter value',
  PRIMARY KEY (`ID`),
  UNIQUE KEY `CfgName` (`CfgName`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='系统参数';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysCustomer`
--

DROP TABLE IF EXISTS `SysCustomer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysCustomer` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `MemberId` varchar(255) NOT NULL COMMENT '会员Id',
  PRIMARY KEY (`ID`) USING BTREE,
  UNIQUE KEY `MemberId` (`MemberId`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='系统管理员手动添加的顾客账号';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysDept`
--

DROP TABLE IF EXISTS `SysDept`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysDept` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `FullName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Num` int DEFAULT NULL,
  `Pid` bigint DEFAULT NULL,
  `Pids` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `SimpleName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Tips` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Version` int DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='部门';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysDict`
--

DROP TABLE IF EXISTS `SysDict`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysDict` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Name` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Num` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Pid` bigint DEFAULT NULL,
  `Tips` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='字典';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysFileInfo`
--

DROP TABLE IF EXISTS `SysFileInfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysFileInfo` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `OriginalFileName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `RealFileName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='文件';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysLinks`
--

DROP TABLE IF EXISTS `SysLinks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysLinks` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint NOT NULL COMMENT 'Creator: a SysUser Id',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  `UpdateBy` bigint NOT NULL COMMENT 'Modifier: a SysUser Id',
  `UpdateTime` datetime NOT NULL COMMENT 'a utc time',
  `DeleteTime` datetime DEFAULT NULL COMMENT 'a utc time',
  `LinkText` varchar(200) DEFAULT NULL COMMENT 'Link text',
  `LinkImage` varchar(255) DEFAULT NULL COMMENT 'Link image',
  `LinkTarget` varchar(255) DEFAULT NULL COMMENT 'a href or a router address',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Link` (`LinkText`,`LinkImage`,`LinkTarget`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Links data maintained by admin';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysLinksReferer`
--

DROP TABLE IF EXISTS `SysLinksReferer`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysLinksReferer` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `RefererType` int NOT NULL COMMENT 'a enum value: 1, front-page-hot-search...',
  `SysLinkId` bigint NOT NULL,
  `OpenInNewTab` int NOT NULL DEFAULT '0' COMMENT '0: NO 1: YES',
  `Sort` int NOT NULL DEFAULT '100' COMMENT 'bigger means more important',
  `CreateBy` bigint NOT NULL COMMENT 'Creator: a SysUser Id',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  `UpdateBy` bigint NOT NULL COMMENT 'Modifier: a SysUser Id',
  `UpdateTime` datetime NOT NULL COMMENT 'a utc time',
  `DeleteTime` datetime DEFAULT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Links Referer data maintained by admin';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysLoginLog`
--

DROP TABLE IF EXISTS `SysLoginLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysLoginLog` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `IP` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `LoginName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Message` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Succeed` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `UserId` int DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='登录日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysMenu`
--

DROP TABLE IF EXISTS `SysMenu`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysMenu` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Code` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '编号',
  `Component` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '組件配置',
  `Hidden` bit(1) DEFAULT NULL COMMENT '是否隐藏',
  `Icon` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '图标',
  `IsMenu` bit(1) NOT NULL COMMENT '是否是菜单1:菜单,0:按钮',
  `IsOpen` bit(1) DEFAULT NULL COMMENT '是否默认打开1:是,0:否',
  `Levels` int NOT NULL COMMENT '级别',
  `Name` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '名称',
  `Num` int NOT NULL COMMENT '顺序',
  `PCode` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL COMMENT '父菜单编号',
  `PCodes` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '递归父级菜单编号',
  `Status` bit(1) NOT NULL COMMENT '状态1:启用,0:禁用',
  `Tips` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '鼠标悬停提示信息',
  `Url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci DEFAULT NULL COMMENT '路由地址',
  PRIMARY KEY (`ID`) USING BTREE,
  UNIQUE KEY `UK_s37unj3gh67ujhk83lqva8i1t` (`Code`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='菜单';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysNotice`
--

DROP TABLE IF EXISTS `SysNotice`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysNotice` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL,
  `CreateTime` datetime DEFAULT NULL,
  `ModifyBy` bigint DEFAULT NULL,
  `ModifyTime` datetime DEFAULT NULL,
  `Content` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Title` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Type` int DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='通知';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysOperationLog`
--

DROP TABLE IF EXISTS `SysOperationLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysOperationLog` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `ClassName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `CreateTime` datetime DEFAULT NULL,
  `LogName` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `LogType` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Message` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '详细信息',
  `Method` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Succeed` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `UserId` int DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='操作日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysRelation`
--

DROP TABLE IF EXISTS `SysRelation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysRelation` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `MenuId` bigint DEFAULT NULL,
  `RoleId` bigint DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='菜单角色关系';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysRole`
--

DROP TABLE IF EXISTS `SysRole`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysRole` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `DeptId` bigint DEFAULT NULL,
  `Name` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Num` int DEFAULT NULL,
  `Pid` bigint DEFAULT NULL,
  `Tips` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Version` int DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='角色';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysTask`
--

DROP TABLE IF EXISTS `SysTask`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysTask` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Concurrent` bit(1) DEFAULT NULL COMMENT '是否允许并发',
  `Cron` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '定时规则',
  `Data` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '执行参数',
  `Disabled` bit(1) DEFAULT NULL COMMENT '是否禁用',
  `ExecAt` datetime DEFAULT NULL COMMENT '执行时间',
  `ExecResult` text CHARACTER SET utf8 COLLATE utf8_general_ci COMMENT '执行结果',
  `JobClass` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '执行类',
  `JobGroup` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '任务组名',
  `Name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '任务名',
  `Note` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '任务说明',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='定时任务';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysTaskLog`
--

DROP TABLE IF EXISTS `SysTaskLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysTaskLog` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `ExecAt` datetime DEFAULT NULL COMMENT '执行时间',
  `ExecSuccess` bit(1) DEFAULT NULL COMMENT '执行结果（成功:1、失败:0)',
  `IdTask` bigint DEFAULT NULL,
  `JobException` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '抛出异常',
  `Name` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '任务名',
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='定时任务日志';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SysUser`
--

DROP TABLE IF EXISTS `SysUser`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SysUser` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间/注册时间',
  `ModifyBy` bigint DEFAULT NULL COMMENT '最后更新人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  `Account` varchar(32) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '账户',
  `Avatar` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
  `Birthday` datetime DEFAULT NULL,
  `DeptId` bigint DEFAULT NULL,
  `Email` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT 'email',
  `Name` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '姓名',
  `Password` varchar(64) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '密码',
  `Phone` varchar(16) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '手机号',
  `RoleId` varchar(128) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '角色id列表，以逗号分隔',
  `Salt` varchar(16) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL COMMENT '密码盐',
  `Sex` int DEFAULT NULL COMMENT '性别 1 男 2 女 3 保密',
  `Status` int DEFAULT NULL,
  `Version` int DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 ROW_FORMAT=DYNAMIC COMMENT='账号';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SystemDictionaryKeys`
--

DROP TABLE IF EXISTS `SystemDictionaryKeys`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SystemDictionaryKeys` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `DictKey` varchar(100) NOT NULL COMMENT 'unique key for the whole system',
  `DictName` varchar(255) NOT NULL COMMENT 'user readable name',
  `DictNameCn` varchar(255) NOT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `DefaultValue` varchar(100) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `IsDynamic` bit(1) NOT NULL COMMENT 'to indicate whether the values of this key comes from value table or from the script',
  `DynamicScript` varchar(500) DEFAULT NULL COMMENT 'a script which used to generate values for the key when isdynamic is true',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `DictKey_UNIQUE` (`DictKey`),
  UNIQUE KEY `Id_UNIQUE` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SystemDictionaryValues`
--

DROP TABLE IF EXISTS `SystemDictionaryValues`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SystemDictionaryValues` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `DictKey` varchar(100) NOT NULL,
  `ValueCode` varchar(100) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `NameCn` varchar(255) NOT NULL,
  `Sort` int DEFAULT '0' COMMENT 'sort index',
  `Description` text,
  `CreateTime` datetime NOT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` bigint NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`),
  UNIQUE KEY `CompinedKey` (`DictKey`,`ValueCode`) /*!80000 INVISIBLE */
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `VerifyCode`
--

DROP TABLE IF EXISTS `VerifyCode`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `VerifyCode` (
  `ID` bigint unsigned NOT NULL AUTO_INCREMENT,
  `code` varchar(32) DEFAULT NULL COMMENT '短信/邮件...验证码',
  `VerifyPurpose` int NOT NULL COMMENT '1登录 2注册 3修改密码 4绑定手机 5绑定邮箱',
  `VerifyWay` tinyint NOT NULL COMMENT '1, mobile; 2, email.',
  `CountryCode` int DEFAULT NULL,
  `Mobile` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `RequestIP` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '验证码请求者IP',
  `VerifyIP` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '验证码验证者IP',
  `UsedStatus` tinyint DEFAULT '0' COMMENT '0,未使用;1,已使用',
  `Remark` varchar(2000) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CreateTime` datetime DEFAULT NULL,
  `ExpireTime` datetime DEFAULT NULL,
  `UsedTime` datetime DEFAULT NULL,
  PRIMARY KEY (`ID`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `YangtaoFiles`
--

DROP TABLE IF EXISTS `YangtaoFiles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `YangtaoFiles` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `Md5` char(32) NOT NULL,
  `Path` char(20) NOT NULL,
  `FileExt` char(5) DEFAULT NULL COMMENT 'File Ext Name with dot',
  `RefCount` int DEFAULT NULL,
  `CreateAt` datetime DEFAULT NULL,
  `UpdateAt` datetime DEFAULT NULL,
  `FileName` char(128) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Md5` (`Md5`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Current Database: `YangtaoUser`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `YangtaoUser` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `YangtaoUser`;

--
-- Current Database: `YangtaoOrders`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `YangtaoOrders` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `YangtaoOrders`;

--
-- Table structure for table `BackupOrders`
--

DROP TABLE IF EXISTS `BackupOrders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `BackupOrders` (
  `OrderId` varchar(255) NOT NULL COMMENT 'guid 主键',
  `OrderNo` varchar(255) NOT NULL COMMENT '有语义的订单号',
  `MemberId` varchar(255) NOT NULL COMMENT 'guid 会员编号',
  `Amount` decimal(10,2) NOT NULL COMMENT '外币订单金额',
  `PaidRmbAmount` decimal(10,2) DEFAULT NULL COMMENT '实付人民币金额',
  `PayTime` datetime DEFAULT NULL COMMENT 'UTC付款时间',
  `PayType` tinyint DEFAULT NULL COMMENT '支付方式： 1 WxPay 2 AliPay',
  `PayResult` tinyint DEFAULT NULL COMMENT '支付结果： 1 支付成 2 支付失败',
  `OrderStatus` tinyint NOT NULL COMMENT '订单状态: 1 未付款 2 已付款 3 已出库 4 运输中 5 已收货',
  `CreateTime` datetime NOT NULL COMMENT 'UTC下单时间',
  `UpdateTime` datetime NOT NULL COMMENT 'UTC订单状态更新时间',
  PRIMARY KEY (`OrderId`),
  UNIQUE KEY `OrderNo` (`OrderNo`) USING BTREE,
  KEY `MemberId` (`MemberId`) USING BTREE,
  KEY `CreateTime` (`CreateTime`) USING BTREE,
  KEY `UpdateTime` (`UpdateTime`) USING BTREE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='用户订单表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Cart`
--

DROP TABLE IF EXISTS `Cart`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Cart` (
  `MemberId` varchar(255) NOT NULL COMMENT '顾客ID: 可能是会员ID,也可能是访客ID',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  PRIMARY KEY (`MemberId`),
  KEY `CreateTime` (`CreateTime`),
  KEY `UpdateTime` (`UpdateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='购物车';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CartItem`
--

DROP TABLE IF EXISTS `CartItem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CartItem` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增长主键',
  `MemberId` varchar(255) NOT NULL COMMENT '顾客ID: 可能是会员ID,也可能是访客ID',
  `SkuId` varchar(255) NOT NULL COMMENT 'Sku编号',
  `Quantity` int NOT NULL DEFAULT '1' COMMENT '数量',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `uniq_cart_item` (`MemberId`,`SkuId`),
  KEY `SkuId` (`SkuId`),
  KEY `CreateTime` (`CreateTime`),
  KEY `UpdateTime` (`UpdateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='购物车商品';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CollectionPointDispatchStoreOrders`
--

DROP TABLE IF EXISTS `CollectionPointDispatchStoreOrders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CollectionPointDispatchStoreOrders` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `DispatchDate` datetime NOT NULL COMMENT 'the date where the order been sent; New Zealand Time zone',
  `DispatchTime` datetime NOT NULL COMMENT 'Disptach Time; utc time',
  `DispatchCPId` varchar(255) NOT NULL,
  `DispatchCPGroupKey` varchar(255) NOT NULL,
  `OperatorId` bigint NOT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `CreateBy` bigint NOT NULL,
  `CreateTime` datetime NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `StoreOrderId` varchar(255) NOT NULL,
  `DeliveryCompanyId` bigint NOT NULL,
  `ExpressNumber` varchar(255) NOT NULL,
  `CPSettlementId` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CollectionPointSettlement`
--

DROP TABLE IF EXISTS `CollectionPointSettlement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CollectionPointSettlement` (
  `Id` varchar(255) NOT NULL,
  `InvoiceNO` varchar(255) DEFAULT NULL,
  `ReferenceCode` varchar(255) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `CreateBy` bigint NOT NULL,
  `UpdateBy` bigint NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Id_UNIQUE` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DeliveryCompany`
--

DROP TABLE IF EXISTS `DeliveryCompany`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DeliveryCompany` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT 'auto increment primary key',
  `NameEn` varchar(128) DEFAULT NULL COMMENT 'Company name in English',
  `NameCn` varchar(128) DEFAULT NULL COMMENT 'Company name in Chinese',
  `Location` varchar(128) DEFAULT NULL COMMENT 'optional',
  `CountryCode` varchar(128) DEFAULT NULL COMMENT 'a unique code defined in CountryList ',
  `CountryNameEn` varchar(128) DEFAULT NULL COMMENT 'Country name in English',
  `CountryNameCn` varchar(128) DEFAULT NULL COMMENT 'Country name in Chinese',
  `Website` varchar(255) DEFAULT NULL COMMENT 'Official website',
  `TrackingUrl` varchar(255) DEFAULT NULL COMMENT 'Tracking url',
  `IsPartner` tinyint NOT NULL DEFAULT '0' COMMENT 'Partner status: 0 non-partner, 1 partner',
  `CanTracking` tinyint NOT NULL DEFAULT '0' COMMENT 'Platform tracking status: 0 cannot track, 1 can track',
  `TrackingMetaData` text COMMENT 'a json string contains the meta information for tracking',
  `CreateBy` bigint DEFAULT NULL COMMENT 'Creator: admin id',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateBy` bigint DEFAULT NULL COMMENT 'Modifier: admin id',
  `UpdateTime` datetime DEFAULT NULL,
  `DeleteTime` datetime DEFAULT NULL COMMENT 'null: normal, non-null: deleted.',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `NameEn` (`NameEn`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Delivery company list';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DeliveryCompanyOld`
--

DROP TABLE IF EXISTS `DeliveryCompanyOld`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DeliveryCompanyOld` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `NameCn` varchar(128) NOT NULL COMMENT '物流公司中国名称',
  `NameEn` varchar(128) DEFAULT NULL COMMENT '物流公司英文名称',
  `Location` varchar(128) DEFAULT NULL COMMENT '总部所在地',
  `Website` varchar(255) DEFAULT NULL COMMENT '公司官网',
  `BasicWeight` int DEFAULT '1000' COMMENT '首重重量(相对于平台) 单位g',
  `BasicPrice` decimal(12,2) DEFAULT NULL COMMENT '首重价格 单位NZD，平台协议价格',
  `StepWeight` int DEFAULT NULL COMMENT '续重单位重量 单位g',
  `AdditionalPrice` decimal(12,2) DEFAULT NULL COMMENT '续重单价 单位NZD，平台协议价格',
  `CurrencyType` varchar(32) DEFAULT 'NZD' COMMENT '计价币种 NZD',
  `Description` varchar(1024) DEFAULT NULL COMMENT '计算说明',
  `CreateBy` int DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` int DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='物流公司表含平台结算价 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `DiscountItem`
--

DROP TABLE IF EXISTS `DiscountItem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `DiscountItem` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `OrderId` varchar(255) NOT NULL,
  `MemberId` varchar(255) NOT NULL,
  `StoreOrderId` varchar(255) DEFAULT NULL,
  `DiscountType` int NOT NULL,
  `DiscountAmount` decimal(12,2) NOT NULL,
  `PayDiscountAmount` decimal(12,2) NOT NULL,
  `ReferenceId` varchar(255) DEFAULT NULL,
  `Description` varchar(500) DEFAULT NULL,
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除',
  `CreateTime` datetime NOT NULL,
  `CreateBy` varchar(255) NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='the discount items of a store order or order';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LogisticsStatistics`
--

DROP TABLE IF EXISTS `LogisticsStatistics`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LogisticsStatistics` (
  `LogisticsTrackingId` bigint NOT NULL,
  `StoreOrderId` varchar(255) NOT NULL DEFAULT '' COMMENT 'redundant information for convenience',
  `TrackingNo` varchar(128) DEFAULT NULL COMMENT 'Logistics order no',
  `ShipmentType` tinyint DEFAULT '1' COMMENT '0 3rd logistics 1 Youtrade logistics',
  `CompanyId` int unsigned NOT NULL COMMENT 'Delivery company Id',
  `CompanyName` varchar(128) DEFAULT NULL,
  `DispatchTime` datetime DEFAULT NULL COMMENT 'Time for dispatching goods',
  `PickupTime` datetime DEFAULT NULL COMMENT 'Time for the first tracking record',
  `ChinaCustomsArrivalTime` datetime DEFAULT NULL COMMENT 'Time for China customs arrival',
  `InChinaPickupTime` datetime DEFAULT NULL COMMENT 'Time for the first tracking record in China',
  `DeliveredTime` datetime DEFAULT NULL COMMENT 'Time for the last tracking record in China when delivered',
  `Delivered` int DEFAULT NULL COMMENT '1 delivered, 0 in transit',
  `PaymentSpentTime` int NOT NULL DEFAULT '0' COMMENT 'StoreOrderPaidTime - StoreOrderCreateTime (in seconds)',
  `DispatchSpentTime` int DEFAULT '0' COMMENT 'Time spent between paid to dispatch',
  `PickupSpentTime` int NOT NULL DEFAULT '0' COMMENT 'PickupTime - DispatchTime (in seconds)',
  `InternationalSpentTime` int DEFAULT '0' COMMENT 'China customs arrival time - pickup time (in seconds)',
  `ClearanceSpentTime` int DEFAULT '0' COMMENT 'First tracking record time in China - China customs arrival time (in seconds)',
  `InChinaSpentTime` int DEFAULT '0' COMMENT 'Receipt time - pickup time in China (in seconds)',
  `TotalSpentTime` int DEFAULT '0' COMMENT 'Receipt time - Store order create time (in seconds)',
  `StoreOrderCreateTime` datetime NOT NULL,
  `StoreOrderPaidTime` datetime NOT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  PRIMARY KEY (`LogisticsTrackingId`),
  KEY `ShipmentType` (`ShipmentType`),
  KEY `StoreOrderId` (`StoreOrderId`),
  KEY `CompanyId` (`CompanyId`),
  KEY `TrackingNo` (`TrackingNo`),
  KEY `TotalSpentTime` (`TotalSpentTime`),
  KEY `StoreOrderCreateTime` (`StoreOrderCreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `LogisticsTracking`
--

DROP TABLE IF EXISTS `LogisticsTracking`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `LogisticsTracking` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `StoreOrderId` varchar(255) NOT NULL DEFAULT '' COMMENT '运单所属店铺订单',
  `ShipmentType` tinyint DEFAULT '1' COMMENT '物流类型 0,卖家自寄;1,平台合作物流; 也可以说承运方式',
  `CompanyId` int unsigned NOT NULL COMMENT '货运公司Id',
  `CompanyName` varchar(128) DEFAULT NULL,
  `TrackingNo` varchar(128) DEFAULT NULL COMMENT '物流单号',
  `StartPoint` varchar(128) DEFAULT NULL COMMENT '物流起点',
  `Destination` varchar(128) DEFAULT NULL COMMENT '物流终点',
  `ItemCount` int DEFAULT NULL COMMENT '包裹内物品数',
  `Status` int DEFAULT NULL COMMENT '物流状态',
  `NextTracking` varchar(1024) DEFAULT NULL COMMENT '下一段快递信息: 一个json字符串',
  `StatusText` varchar(32) DEFAULT NULL COMMENT '运单状态文本',
  `TrackingRecords` mediumtext COMMENT '物流追踪详情，一个json字符串',
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `StoreOrderId` (`StoreOrderId`),
  KEY `ShipmentType` (`ShipmentType`),
  KEY `CompanyId` (`CompanyId`),
  KEY `TrackingNo` (`TrackingNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MemberScore`
--

DROP TABLE IF EXISTS `MemberScore`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MemberScore` (
  `MemberId` varchar(255) NOT NULL,
  `AvailableScore` int NOT NULL COMMENT 'the scores, current user can consume',
  `UpcomingScore` int NOT NULL COMMENT 'Upcoming Score',
  `Achievement` varchar(100) DEFAULT NULL,
  `StatisticData` text COMMENT 'statistic data of each category',
  `ConsumedScore` int NOT NULL,
  `CreateTime` datetime NOT NULL,
  `CreateBy` varchar(100) NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` varchar(100) NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`MemberId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Personal Score statistic data';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `MemberScoreItem`
--

DROP TABLE IF EXISTS `MemberScoreItem`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `MemberScoreItem` (
  `ID` bigint NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(255) NOT NULL,
  `ScoreType` tinyint NOT NULL COMMENT '1: Credit; 0: Debit',
  `ItemEventType` int NOT NULL COMMENT ' represent different retrieve or consume event type',
  `IsActive` tinyint NOT NULL DEFAULT '1' COMMENT 'represent whether this point is upcoming or not; 0: upcoming score; 1: real score; ',
  `ScorePoints` int NOT NULL,
  `OrderNO` varchar(255) DEFAULT NULL,
  `RMBAmount` decimal(10,2) DEFAULT NULL COMMENT 'how much RMB amount deduction in this item',
  `StoreOrderNO` varchar(255) DEFAULT NULL,
  `FriendMemberId` varchar(255) DEFAULT NULL,
  `Quantity` int DEFAULT '0' COMMENT 'how many wish items have been achieved',
  `EventDate` datetime NOT NULL,
  `CreateTime` datetime NOT NULL,
  `CreateBy` varchar(100) NOT NULL,
  `UpdateBy` varchar(100) NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `EventTime` datetime NOT NULL,
  `Description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `OrderItems`
--

DROP TABLE IF EXISTS `OrderItems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `OrderItems` (
  `Id` varchar(255) NOT NULL COMMENT '唯一编号',
  `OrderId` varchar(255) NOT NULL COMMENT '主订单ID',
  `OrderNo` varchar(255) NOT NULL COMMENT '主订单编号',
  `StoreOrderId` varchar(255) NOT NULL COMMENT '子订单ID',
  `StoreOrderNo` varchar(255) NOT NULL COMMENT '子订单编号',
  `StoreId` bigint NOT NULL COMMENT '商铺Id',
  `MemberId` varchar(255) NOT NULL COMMENT '下单用户Id',
  `SkuId` varchar(255) NOT NULL COMMENT 'SkuId',
  `SkuNameCn` varchar(1024) DEFAULT NULL COMMENT 'Sku中文名称',
  `SkuNameEn` varchar(1024) DEFAULT NULL COMMENT 'Sku英文名称',
  `SkuImg` varchar(255) DEFAULT NULL COMMENT 'Sku主图 冗余',
  `SellingPrice` decimal(12,2) DEFAULT NULL COMMENT '购买单价 单位为卖方币种',
  `Weight` int NOT NULL DEFAULT '0' COMMENT '单位重量 g',
  `PayPrice` decimal(12,2) DEFAULT NULL COMMENT '实际支付单价 单位为买方币种,支付完更新,对应支付时汇率，只供参考',
  `Quantity` int DEFAULT NULL COMMENT '购买数量',
  `SpecValue` varchar(3072) DEFAULT NULL COMMENT 'Sku规格（Json）',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除',
  `PromotionEventId` bigint DEFAULT '0' COMMENT '活动编号',
  `PromotionSkuId` bigint DEFAULT '0' COMMENT 'PromotionSkus 表中的自增id，不是常规的skuId',
  `FinalPrice` decimal(12,2) NOT NULL COMMENT '实际购买价-纽币',
  `FreeShipping` tinyint NOT NULL DEFAULT '0' COMMENT '是否包邮 0,不包邮;1,包邮',
  `ShipmentType` tinyint DEFAULT '1' COMMENT '承运方式 0,卖家自寄;1,平台合作物流',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `StoreId` (`StoreId`),
  KEY `UpdateTime` (`UpdateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='订单商品表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `OrderMessage`
--

DROP TABLE IF EXISTS `OrderMessage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `OrderMessage` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '订单留言编号',
  `StoreOrderId` varchar(255) NOT NULL COMMENT '商铺订单Id',
  `StoreOrderNo` varchar(255) NOT NULL COMMENT '商铺订单编号',
  `OrderId` varchar(255) NOT NULL COMMENT '订单Id',
  `OrderNo` varchar(255) NOT NULL COMMENT '订单编号',
  `StoreId` bigint NOT NULL COMMENT '店铺Id',
  `Message` varchar(3072) DEFAULT NULL,
  `MultimediaInfo` varchar(3072) DEFAULT NULL COMMENT 'JSON字符串：留言图片 图片等多媒体信息',
  `MemberId` varchar(255) NOT NULL COMMENT '留言用户 留言用户Id',
  `DestinMemberId` varchar(255) DEFAULT NULL COMMENT 'destination member Id',
  `HasRead` tinyint NOT NULL DEFAULT '0' COMMENT '0,未读; 1,已读',
  `CreateTime` datetime NOT NULL COMMENT '创建时间',
  `UpdateTime` datetime NOT NULL COMMENT '最后编辑时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '删除时间',
  `MessageType` tinyint NOT NULL DEFAULT '0' COMMENT '1:represent from store, 2: represet customer',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='订单留言';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Orders`
--

DROP TABLE IF EXISTS `Orders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Orders` (
  `Id` varchar(255) NOT NULL COMMENT '订单Id',
  `OrderNo` varchar(255) NOT NULL COMMENT '订单编号 生成规则在程序中',
  `MemberId` varchar(255) NOT NULL COMMENT '下单用户编号',
  `RecipientName` varchar(128) NOT NULL COMMENT '收货人姓名',
  `RecipientPhone` varchar(128) NOT NULL COMMENT '收货人联系电话',
  `RecipientAddress` varchar(1024) NOT NULL COMMENT '收货人地址',
  `ProvinceCode` bigint DEFAULT NULL COMMENT '收货人省Code',
  `CityCode` bigint DEFAULT NULL COMMENT '收货人市Code',
  `AreaCode` bigint DEFAULT NULL COMMENT '收货人地区Code',
  `AddressId` int DEFAULT NULL COMMENT '收货人地址Id',
  `CreateTime` datetime DEFAULT NULL COMMENT '订单提交时间UTC',
  `PayOverdueTime` datetime DEFAULT NULL COMMENT '支付截止时间UTC',
  `PayWay` tinyint DEFAULT NULL COMMENT '支付方式 支付方式： 1 WxPay 2 AliPay',
  `ExchangeRate` decimal(4,2) DEFAULT NULL COMMENT '支付时汇率',
  `PaymentId` varchar(255) DEFAULT NULL COMMENT '支付单Id',
  `OrderStatus` int NOT NULL DEFAULT '1' COMMENT '订单状态 0,失败订单;1,待付款;2,已付款;3,已出库;(到ytabc收寄点);4,已发货(已更新物流单信息);5,已完成(用户确认收货);6,已取消;',
  `OrderStatusInfo` varchar(128) DEFAULT NULL COMMENT '订单状态说明 譬如：超时取消,支付失败;',
  `OrderTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '订单总价 卖方币种',
  `ShouldTotalOrderPrice` decimal(12,2) DEFAULT NULL COMMENT 'how much money consumer should pay for this order; seller currency',
  `PayShouldTotalOrderPrice` decimal(12,2) DEFAULT NULL COMMENT 'how much money consumer should pay for this order; buyer currency',
  `SkuTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '商品总价 卖方币种',
  `ExpressPrice` decimal(12,2) DEFAULT NULL COMMENT '运费总价 卖方币种',
  `PayOrderTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '订单总价 买方币种',
  `PaySkuTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '商品总价 买方币种',
  `PayExpressPrice` decimal(12,2) DEFAULT NULL COMMENT '运费总价 买方币种',
  `RealPayAmount` decimal(12,2) DEFAULT NULL COMMENT '实际支付总额 买方币种',
  `RealPayAmountLocal` decimal(12,2) DEFAULT NULL COMMENT '实付总金额，卖方币种含运费和手续费',
  `PayStatus` int DEFAULT '0' COMMENT '订单支付状态 0,未支付;1,支付成功;2,支付失败;',
  `RefundStatus` int DEFAULT NULL COMMENT '退款状态 0,无退款;1,部分退款;2,全部退款;',
  `PaymentNo` varchar(1024) DEFAULT NULL COMMENT '支付单编号',
  `PayTime` datetime DEFAULT NULL COMMENT '订单支付时间UTC',
  `PayConfirmTime` datetime DEFAULT NULL COMMENT '支付确认时间UTC',
  `ConfirmTime` datetime DEFAULT NULL COMMENT '订单确认时间UTC 确认后发货，确认前双方可取消订单。如果取消订单，调用接口退款。',
  `ReachToExpressTime` datetime DEFAULT NULL COMMENT '订单最近一个子订单发货时间UTC 发货前可以申请退款，发货后第二天公司给商户结算',
  `SuccessTime` datetime DEFAULT NULL COMMENT '订单最近一个子订单收货完成时间UTC',
  `UpdateTime` datetime DEFAULT NULL COMMENT '最后更新时间UTC',
  `InvoiceInfo` varchar(1024) DEFAULT NULL COMMENT 'Invoice信息 备用：（暂不提供发票.）发票信息，国外不是发票，但是也有Invoice',
  `Remark` varchar(1024) DEFAULT NULL COMMENT '备注',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `UniqueOrderNo` (`OrderNo`),
  KEY `OrderStatus` (`OrderStatus`,`PayOverdueTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='主订单表 订单ID为GUID;订单编号(OrderNo):有含义订单号生成规则：25位的有效订单号。 18位时间戳（Ticks）+2位GUID做种子的随机数+3位进程号+3位机器号（内网IP最后3位）。订单中的总价格信息由分订单价格信息计算得出。';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PaymentInfo`
--

DROP TABLE IF EXISTS `PaymentInfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PaymentInfo` (
  `Id` varchar(255) NOT NULL COMMENT 'uuid: Yangtaoabc支付Id',
  `PaymentNo` varchar(255) NOT NULL COMMENT '第三方支付编号：such as superpay支付号',
  `OrderNo` varchar(255) NOT NULL COMMENT '支付订单号',
  `MemberId` varchar(255) NOT NULL COMMENT '支付人Id',
  `PayWay` tinyint DEFAULT NULL COMMENT '支付方式 同订单。支付方式： 1 WxPay 2 AliPay',
  `PayStatus` tinyint DEFAULT NULL COMMENT '支付状态 0,待支付;1,支付成功;2,支付失败;3,支付确认中;',
  `CreateTime` datetime DEFAULT NULL COMMENT '支付时间',
  `Amount` decimal(12,2) DEFAULT NULL COMMENT '支付金额',
  `PayFee` decimal(12,2) DEFAULT NULL COMMENT '手续费',
  `RefundStatus` tinyint DEFAULT '0' COMMENT '退款状态 0,无退款;1,部分退款;2,全部退款;',
  `ConfirmTime` datetime DEFAULT NULL COMMENT '支付确认时间 三方支付通知成功或失败的时间',
  `UpdateTime` datetime DEFAULT NULL COMMENT '最后更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='支付信息表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PlatformDeliveryPriceRule`
--

DROP TABLE IF EXISTS `PlatformDeliveryPriceRule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PlatformDeliveryPriceRule` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT 'auto increment primary key',
  `CompanyId` int NOT NULL COMMENT 'Id for delivery company',
  `StartCityCode` varchar(128) DEFAULT NULL COMMENT 'City code of startpoint city',
  `StartCountryCode` varchar(128) DEFAULT NULL COMMENT 'Country code of startpoint country',
  `BasicWeight` int DEFAULT '1000' COMMENT 'Basic weight in gram: <= the basic weight, charges the basic price',
  `BasicPrice` decimal(12,2) DEFAULT NULL COMMENT 'Price for basic weight in NZD',
  `StepWeight` int DEFAULT NULL COMMENT 'Step weight in gram',
  `StepPrice` decimal(12,2) DEFAULT NULL COMMENT 'Step price for step weight: <= the step weight, charges the step price',
  `ExtraWeight` int DEFAULT '300' COMMENT 'Extra weight for parcel box, filling etc',
  `MaxItemCountPerParcel` int NOT NULL DEFAULT '10' COMMENT 'The sku max count per package.',
  `Currency` int DEFAULT '1' COMMENT '1: NZD 2: AUD 3: JPY',
  `Description` varchar(1024) DEFAULT NULL COMMENT 'Rules in detail',
  `CreateBy` bigint DEFAULT NULL COMMENT 'Creator: admin id',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateBy` bigint DEFAULT NULL COMMENT 'Modifier: admin id',
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `CSS` (`CompanyId`,`StartCountryCode`,`StartCityCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Price rules of partner delivery companies';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Settlement`
--

DROP TABLE IF EXISTS `Settlement`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Settlement` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT 'AutoIncrement Primary Key',
  `SettlementNo` varchar(128) NOT NULL COMMENT 'Shorter version of SettlementNum',
  `SettlementNum` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT 'A human friendly Settlement id in numbers',
  `StoreId` bigint NOT NULL COMMENT 'Store Id',
  `StoreOrderCount` int DEFAULT NULL COMMENT 'StoreOrderCount in this settlement',
  `Amount` decimal(12,2) NOT NULL COMMENT 'TotalAmount in NZD',
  `StoreOrderEndTime` datetime DEFAULT NULL COMMENT 'End of CreateTime of StoreOrders in UTC',
  `StoreAccount` varchar(128) NOT NULL COMMENT 'BankAccount of Store for settlement',
  `BankName` varchar(128) NOT NULL COMMENT 'BankName for BankAccount',
  `Status` tinyint NOT NULL COMMENT 'Settlement status: 0,not settle yet;1,settled;3, failed to settle;',
  `CreateBy` int DEFAULT NULL,
  `CreateTime` datetime DEFAULT NULL,
  `UpdateBy` int DEFAULT NULL,
  `UpdateTime` datetime DEFAULT NULL,
  `DeleteTime` datetime DEFAULT NULL COMMENT 'not null means deleted',
  `CommissionRate` decimal(5,2) DEFAULT NULL COMMENT 'Platform take rate, percentage in decimal: such as 6 means 6%',
  `PlatformCommission` decimal(12,2) DEFAULT NULL COMMENT 'Commission: OrderPrice(+ deliveryPrice if non-YouTrade logistics) * CommissionRate / 100',
  `SettlementTime` datetime DEFAULT NULL COMMENT 'payment time',
  `GSTAmount` decimal(12,2) DEFAULT NULL COMMENT 'GST amount for commission',
  `GSTRate` decimal(5,2) DEFAULT NULL COMMENT 'GST rate',
  `SettlementAmount` decimal(12,2) DEFAULT NULL COMMENT 'Settlement amount: OrderPrice(+ deliveryPrice if non-YouTrade logistics) - PlatformCommission',
  `CommissionStatus` tinyint NOT NULL DEFAULT '0' COMMENT '0: haven’t been transferred to company’s account;1:have been transferred;',
  `CommissionTransferTime` datetime DEFAULT NULL COMMENT 'When the commission is transferred to company''s account;',
  PRIMARY KEY (`Id`) USING BTREE,
  UNIQUE KEY `SettlementNo` (`SettlementNo`),
  UNIQUE KEY `SettlementNum` (`SettlementNum`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Settlement records';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SettlementLog`
--

DROP TABLE IF EXISTS `SettlementLog`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SettlementLog` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SettlementId` bigint NOT NULL,
  `SettlementNo` varchar(128) DEFAULT NULL,
  `Way` tinyint DEFAULT NULL COMMENT '0,just create an settlement item;1,create and settle at the same time',
  `TotalAmount` decimal(12,2) DEFAULT NULL COMMENT 'Total amount takes part in current settlement in NZD',
  `SettlementAmount` decimal(12,2) DEFAULT NULL COMMENT 'OrderPrice(+ deliveryPrice if non-YouTrade logistics) - PlatformCommission',
  `CommissionAmount` decimal(12,2) DEFAULT NULL COMMENT 'TotalAmount * CommissionRate / 100',
  `OrderCount` int DEFAULT NULL COMMENT 'Count of StoreOrders take part in current settlement',
  `StoreOrders` text COMMENT 'json array of storeOrderId of StoreOrders take part in current settlement',
  `OperateStatus` tinyint DEFAULT NULL COMMENT '0,Fail;1,Success',
  `CreateBy` int DEFAULT NULL,
  `CreateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Log for settlement';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreOrders`
--

DROP TABLE IF EXISTS `StoreOrders`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreOrders` (
  `StoreOrderId` varchar(255) NOT NULL COMMENT '分订单ID',
  `StoreOrderNo` varchar(255) NOT NULL COMMENT '分订单编号 主订单编号+商铺StoreID6位补0',
  `StoreId` bigint NOT NULL COMMENT '商铺编号',
  `OrderId` varchar(255) NOT NULL COMMENT '主订单ID',
  `OrderNo` varchar(255) NOT NULL COMMENT '主订单编号',
  `StoreName` varchar(128) DEFAULT NULL COMMENT '商铺名 冗余字段',
  `MemberId` varchar(255) NOT NULL COMMENT '下单用户编号',
  `TotalWeight` int NOT NULL DEFAULT '0' COMMENT '商铺订单总重量 g',
  `OrderTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '子订单总价 卖方币种,商品总价+运费总价',
  `ShouldTotalOrderPrice` decimal(12,2) DEFAULT NULL COMMENT 'how much money consumer should pay for this store order; seller currency',
  `PayShouldTotalOrderPrice` decimal(12,2) DEFAULT NULL COMMENT 'how much money consumer should pay for this store order; buyer currency',
  `PayOrderTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '实际支付子订单总价 买方支付币种',
  `SkuTotalPrice` decimal(12,2) DEFAULT NULL COMMENT '子订单商品总价 卖方币种',
  `PaySkuTotalPrice` decimal(12,2) DEFAULT NULL,
  `ExpressPrice` decimal(12,2) DEFAULT NULL COMMENT '子订单运费总价 卖方币种',
  `PayExpressPrice` decimal(12,2) DEFAULT NULL COMMENT 'indicate the express fee that comsumer will pay in buy currency for a store order',
  `ExpressStatus` tinyint DEFAULT NULL COMMENT '物流状态 0,未发货;1,已发货(货物到收寄点或者填物流信息后改为已发货);2:确认收货',
  `LeaveMessage` varchar(1024) DEFAULT NULL COMMENT '买家留言',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `Description` varchar(512) DEFAULT NULL COMMENT '订单说明',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  `StoreOrderStatus` int NOT NULL DEFAULT '1' COMMENT '订单状态 0,失败订单;1,待付款;2,已付款;3,已出库;(到ytabc收寄点);4,已发货(已更新物流单信息);5,已完成(用户确认收货);6,已取消;',
  `ExpressTime` datetime DEFAULT NULL COMMENT '发货时间',
  `SettlementStatus` tinyint NOT NULL DEFAULT '0' COMMENT '平台结算状态 0,未结算;1,已结算;2,结算中;3,结算失败;',
  `SettlementAmount` decimal(12,2) DEFAULT NULL COMMENT '结算金额(纽币):订单金额[+运费]-佣金',
  `PlatformCommission` decimal(12,2) DEFAULT NULL COMMENT '佣金:订单金额[+运费金额(如果商户自己邮寄)] * 佣金比例(目前为基础比例6.9%)',
  `SettlementId` bigint DEFAULT NULL COMMENT '结算单Id',
  `SettlementNo` varchar(128) DEFAULT NULL COMMENT '结算单编号',
  `SettlementTime` datetime DEFAULT NULL COMMENT '结算时间',
  `CommissionRate` decimal(5,2) DEFAULT NULL COMMENT '佣金比例',
  `GSTAmount` decimal(12,2) DEFAULT NULL COMMENT '佣金GST',
  `GSTRate` decimal(5,2) DEFAULT NULL COMMENT 'GST税率',
  `PayProcessFee` decimal(12,2) DEFAULT NULL COMMENT '支付手续费',
  `PayProcessFeeRate` decimal(5,2) DEFAULT NULL COMMENT '支付手续费比例',
  `ShipmentType` tinyint DEFAULT '1' COMMENT '物流类型 0,卖家自寄;1,平台合作物流; 也可以说承运方式',
  PRIMARY KEY (`StoreOrderId`) USING BTREE,
  UNIQUE KEY `StoreOrderNoUnique` (`StoreOrderNo`),
  KEY `StoreOrderStatus` (`StoreOrderStatus`,`CreateTime`),
  KEY `CreateTime` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='分订单表 一个主订单根据商户分成1到多个分订单，分订单号(StoreOrderNo)为主订单号+商户ID（后6位，不足补0，支撑99w商户，超过99万改为7位）';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Current Database: `YangtaoCommodity`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `YangtaoCommodity` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `YangtaoCommodity`;

--
-- Table structure for table `AttributeValues`
--

DROP TABLE IF EXISTS `AttributeValues`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `AttributeValues` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `AttrValue` varchar(32) NOT NULL COMMENT '属性值',
  `ProductId` varchar(255) NOT NULL COMMENT '商品ID',
  `AttrId` int DEFAULT NULL COMMENT '属性Id',
  `ImgUrl` varchar(255) DEFAULT NULL COMMENT '属性图片显示',
  `SpecCn` varchar(128) DEFAULT NULL COMMENT '属性中文显示',
  `SpecEn` varchar(128) DEFAULT NULL COMMENT '属性英文显示',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺Id',
  `CreateBy` varchar(32) DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(32) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='属性值表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Attributes`
--

DROP TABLE IF EXISTS `Attributes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Attributes` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `AttrCn` varchar(32) DEFAULT NULL COMMENT '属性中文名',
  `AttrEn` varchar(32) DEFAULT NULL COMMENT '属性英文名',
  `AttrKey` varchar(32) NOT NULL,
  `Status` int DEFAULT '1' COMMENT '状态： 0 停用  1 启用',
  `CategoryId` int DEFAULT NULL COMMENT '所属类目',
  `CreateBy` varchar(32) DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(32) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `AttrKey` (`AttrKey`,`CategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='属性基础表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Brand`
--

DROP TABLE IF EXISTS `Brand`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Brand` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '品牌ID',
  `NameCn` varchar(128) DEFAULT NULL COMMENT '品牌中文名',
  `NameEn` varchar(128) DEFAULT NULL COMMENT '品牌英文名',
  `Status` int DEFAULT NULL COMMENT '品牌状态 状态:0,禁用;1,启用',
  `VerifyStatus` int DEFAULT NULL COMMENT '品牌审核状态 申请：0,审核中;1,审核通过;2,驳回',
  `LogoImage` varchar(255) DEFAULT NULL COMMENT '品牌Logo',
  `CategoryID` int DEFAULT NULL COMMENT '品牌主类目',
  `CategoryName` varchar(128) DEFAULT NULL COMMENT '品牌主类目名称 冗余字段,中英文同时保存，逗号分隔',
  `CompanyNameCn` varchar(128) DEFAULT NULL COMMENT '品牌所属公司中文',
  `CompanyNameEn` varchar(128) DEFAULT NULL COMMENT '品牌所属公司英文',
  `CountryCode` varchar(128) DEFAULT NULL COMMENT '所属国家Code 所属国家code',
  `CountryNameCn` varchar(128) DEFAULT NULL COMMENT '所属国家中文名',
  `Sort` int DEFAULT '100' COMMENT '展示排序',
  `StoreId` bigint DEFAULT NULL COMMENT '申请商户 商户StoreId',
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人 后台创建管理员',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 品牌只做软删除',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` int DEFAULT NULL COMMENT '审核人 后台管理员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '审核时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='品牌表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Category`
--

DROP TABLE IF EXISTS `Category`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Category` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '类目ID',
  `NameEn` varchar(128) DEFAULT NULL COMMENT '类目英文名称',
  `NameCn` varchar(128) DEFAULT NULL COMMENT '类目中文名称',
  `ParentId` int DEFAULT NULL COMMENT '父类目ID',
  `Level` varchar(32) DEFAULT NULL COMMENT '类目层级 类目层级:0为root',
  `Path` varchar(128) DEFAULT NULL COMMENT '类目Path 遍于找上下级类目',
  `SequenceNo` int DEFAULT '1000' COMMENT '类目排序号',
  `Image` varchar(255) DEFAULT NULL COMMENT '类目展示图',
  `Description` varchar(1024) DEFAULT NULL COMMENT '描述',
  `IsValid` int DEFAULT '1' COMMENT '是否有效 用于过滤整个类别产品的展示.0,无效;1,有效;',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 类目只做软删除',
  `CreateBy` int DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` int DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='基础类目';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CategoryGroup`
--

DROP TABLE IF EXISTS `CategoryGroup`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CategoryGroup` (
  `GroupId` int NOT NULL AUTO_INCREMENT COMMENT '类目别名ID',
  `NameCn` varchar(128) DEFAULT NULL COMMENT '别名中文名称 不成超过50个字',
  `NameEn` varchar(128) DEFAULT NULL COMMENT '别名英文名称 不成超过1200个字符',
  `CategoryIds` varchar(512) NOT NULL COMMENT '包含类目ID 不超过20个类目ID，半角逗号分隔',
  `Image` varchar(255) DEFAULT NULL COMMENT '类目组展示图',
  `ShowLocation` int DEFAULT '0' COMMENT '展示位置， 0 表示不展示',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 类目组合只做软删除',
  `CreateBy` int DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` int DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `Weight` int NOT NULL DEFAULT '100' COMMENT '重要性，数字越大越重要，可用来排序',
  `CCTag` varchar(255) DEFAULT NULL COMMENT 'for some specified tags',
  PRIMARY KEY (`GroupId`),
  UNIQUE KEY `NameCn` (`NameCn`),
  KEY `ShowLocation` (`ShowLocation`),
  KEY `Weight` (`Weight`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='类目组合别名表 类目组合,每个类目别名可包含一个或多个具体类目.只做两级';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CategoryTag`
--

DROP TABLE IF EXISTS `CategoryTag`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CategoryTag` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '标签Id',
  `NameCn` varchar(128) DEFAULT NULL COMMENT '标签中文名',
  `NameEn` varchar(128) DEFAULT NULL COMMENT '标签英文名',
  `CategoryIds` varchar(512) DEFAULT NULL COMMENT '对应类目Id 不超过20个类目ID，半角逗号分隔',
  `CreateBy` int DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` int DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='类目标签表 每个标签对应多个类目检索';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CouponEvent`
--

DROP TABLE IF EXISTS `CouponEvent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CouponEvent` (
  `Id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'coupon name',
  `StartTime` datetime NOT NULL,
  `EndTime` datetime DEFAULT NULL,
  `RetriveCondition` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ConsumeCondition` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Quantity` int NOT NULL COMMENT '-100000 represent infinit',
  `UsedCount` int NOT NULL DEFAULT '0',
  `FetchedCount` int NOT NULL DEFAULT '0',
  `InitialQuantity` int NOT NULL,
  `DurationDay` int DEFAULT NULL COMMENT 'indicate how many days the user should consume it aftr it get the coupon. if it  is null, the validate period is the same as end time as',
  `DiscountType` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Use for decide which type of coupon could be used together or not; in current stage, it is a backup stage',
  `Status` tinyint NOT NULL COMMENT '        /// 0: represent not active\n        /// 1: represent active\n        /// 2: represent cancel',
  `Description` varchar(1000) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `StoreId` int DEFAULT NULL COMMENT 'if it is null represet this coupon is sent by platform; otherwise it is sent by a store.',
  `PlatfomFee` decimal(10,2) DEFAULT NULL,
  `StoreFee` decimal(10,2) DEFAULT NULL,
  `FeeShoulderType` tinyint NOT NULL COMMENT '1, percentage, 2: fix platform first, 3:fix store first',
  `CreateBy` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `UpdateBy` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Channel` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'DipatchBySystem' COMMENT 'how coupon would be dispatched',
  `EffectStartTime` datetime DEFAULT NULL COMMENT 'the time period when users could use this coupon',
  `EffectEndTime` datetime DEFAULT NULL,
  `RowNumber` int NOT NULL DEFAULT '0' COMMENT 'row sort number',
  `ColumnNumber` int NOT NULL DEFAULT '0' COMMENT 'column sort number',
  `LimitNumber` int DEFAULT '1' COMMENT 'indicate how many coupons one customer can get ',
  `RecommendToHomePage` bit(1) DEFAULT b'0' COMMENT 'to indicate whether this item should be displayed in home page',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `CustomerCoupon`
--

DROP TABLE IF EXISTS `CustomerCoupon`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CustomerCoupon` (
  `Id` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `CouponEventID` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `CouponEventName` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `StartTime` datetime NOT NULL,
  `EndTime` datetime DEFAULT NULL,
  `UserId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UseTime` datetime DEFAULT NULL COMMENT 'the time when user use the coupon',
  `ReferenceOrderId` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'the store order when this coupon is consumed',
  `Result` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Active` bit(1) NOT NULL,
  `External` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'to store some additional message like fetch code',
  `ReferenceStoreOrderId` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `CreateBy` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `UpdateBy` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NonSkuWishList`
--

DROP TABLE IF EXISTS `NonSkuWishList`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `NonSkuWishList` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(255) NOT NULL,
  `Description` varchar(4096) NOT NULL DEFAULT '',
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  KEY `MemberId` (`MemberId`),
  KEY `CreateTime` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `NonSkuWishPicture`
--

DROP TABLE IF EXISTS `NonSkuWishPicture`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `NonSkuWishPicture` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `WishId` bigint NOT NULL,
  `MemberId` varchar(255) NOT NULL,
  `ImgUrl` varchar(255) NOT NULL,
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  KEY `CreateTime` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PriceHistory`
--

DROP TABLE IF EXISTS `PriceHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PriceHistory` (
  `Id` bigint DEFAULT NULL COMMENT '自增编号',
  `SkuId` varchar(255) DEFAULT NULL COMMENT 'SkuId',
  `OriginalPrice` decimal(12,2) DEFAULT NULL COMMENT '调整前售价',
  `NewPrice` decimal(12,2) DEFAULT NULL COMMENT '调整后售价',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 商铺管理员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='售价调整历史 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Product`
--

DROP TABLE IF EXISTS `Product`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Product` (
  `Id` varchar(255) NOT NULL COMMENT '商品ID',
  `NameCn` varchar(128) DEFAULT NULL COMMENT '商品中文名',
  `NameEn` varchar(128) NOT NULL COMMENT '商品英文名',
  `SubTitleCn` varchar(128) DEFAULT NULL COMMENT '商品中文副标题',
  `SubTitleEn` varchar(128) DEFAULT NULL COMMENT '商品英文副标题',
  `Slogan` varchar(128) DEFAULT NULL COMMENT '商品简短广告语',
  `CategoryId` int DEFAULT NULL COMMENT '商品类目编号',
  `CategoryName` varchar(128) DEFAULT NULL COMMENT 'Redundant name from table Category',
  `CategoryPath` varchar(32) DEFAULT NULL COMMENT '类目路径 冗余对应类目,用短横分隔，例:0-1-11',
  `Specs` varchar(3072) DEFAULT NULL COMMENT '规格组Json 商品对应展示的规格',
  `Attributes` varchar(3072) DEFAULT NULL COMMENT '属性组Json 商品描述展示的属性',
  `Style` int DEFAULT '0' COMMENT '规格展示形式 0,文字;1,图片;2,文字+图片',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺Id',
  `StoreName` varchar(128) DEFAULT NULL COMMENT '店铺名称 默认中文，无中文则写英文，冗余字段',
  `BrandId` int DEFAULT NULL COMMENT '品牌Id',
  `BrandName` varchar(128) DEFAULT NULL COMMENT '品牌名称',
  `ImgUrl` varchar(255) DEFAULT NULL COMMENT '商品主图',
  `DetailCn` text COMMENT '商品中文详情 可设置所有Sku公用详情',
  `DetailEn` text COMMENT '商品英文详情 可设置所有Sku公用详情',
  `IsPlatform` int DEFAULT '0' COMMENT '自营商品 0,非自营;1,自营;默认非自营,初期不做自营，字段备用;',
  `CommissionRate` int DEFAULT NULL COMMENT '平台佣金比例 0-100,除100代表百分比',
  `State` int DEFAULT NULL COMMENT '商品状态 0,停售;1,在售;2,违规',
  `StateDes` varchar(1024) DEFAULT NULL COMMENT '状态说明',
  `VerifyStatus` int DEFAULT NULL COMMENT '审核状态 0,待审核;1,审核通过;2,审核未通过;',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商品表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `ProductPictures`
--

DROP TABLE IF EXISTS `ProductPictures`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `ProductPictures` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增长的编号',
  `ProductId` varchar(255) NOT NULL COMMENT '商品编号',
  `SkuId` varchar(255) NOT NULL COMMENT 'Sku编号',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺Id',
  `ImgUrl` varchar(255) NOT NULL COMMENT '图片路径',
  `Sort` int DEFAULT '100' COMMENT '排序',
  `IsShow` int DEFAULT '0' COMMENT '是否显示 0,不显示;1,显示;',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 商铺管理员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  PRIMARY KEY (`Id`),
  KEY `SkuId` (`SkuId`),
  KEY `ProductId` (`ProductId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商品图片表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PromotionEvent`
--

DROP TABLE IF EXISTS `PromotionEvent`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PromotionEvent` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '活动编号',
  `StoreId` bigint NOT NULL COMMENT '店铺ID 平台活动则为0',
  `EventName` varchar(128) NOT NULL COMMENT '活动名称',
  `Description` text COMMENT '活动说明，请务必使用英文',
  `StartTime` datetime DEFAULT NULL COMMENT '活动开始时间',
  `EndTime` datetime DEFAULT NULL COMMENT '活动结束时间',
  `MinimumQuantity` int DEFAULT '0' COMMENT '最低购买数限制 购买下限，0:不限制',
  `Status` tinyint NOT NULL DEFAULT '1' COMMENT '状态 状态 0:待审核 1:正常2:取消',
  `EventType` tinyint NOT NULL COMMENT '活动类型 0,单品促销;1,条件促销（满赠属于条件促销）;2,赠品促销（买A商品赠B商品）',
  `PromotionType` tinyint NOT NULL COMMENT '促销类型 0, 折扣;1,特价;2,直降;3,满减;4,满赠;5,买A赠B;',
  `PromotionValue` decimal(12,2) NOT NULL COMMENT '优惠值',
  `EventScope` tinyint NOT NULL DEFAULT '1' COMMENT '活动范围 0,平台活动;1,店铺活动',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 操作人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人 操作人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  `EventTag` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='促销活动';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `PromotionSkus`
--

DROP TABLE IF EXISTS `PromotionSkus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `PromotionSkus` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '编号',
  `PromotionEventId` bigint NOT NULL COMMENT '活动编号',
  `StoreId` bigint NOT NULL COMMENT '店铺ID 平台活动则为0',
  `SkuId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL COMMENT '活动Sku编号',
  `StartTime` datetime NOT NULL COMMENT '活动开始时间',
  `EndTime` datetime NOT NULL COMMENT '活动结束时间',
  `MinQuantity` int DEFAULT '0' COMMENT '最低购买数限制 购买下限，0:不限制',
  `MaxQuantity` int DEFAULT '0' COMMENT '最高购买数限制 购买上限，0:不限制',
  `Status` tinyint NOT NULL DEFAULT '1' COMMENT '状态 状态 0:待审核 1:正常2:取消',
  `EventType` tinyint NOT NULL COMMENT '活动类型 0,单品促销;1,条件促销（满赠属于条件促销）;2,赠品促销（买A商品赠B商品）',
  `PromotionType` tinyint NOT NULL COMMENT '促销类型 0, 折扣;1,特价;2,直降;3,满减;4,满赠;5,买A赠B;',
  `DISCOUNT` decimal(6,2) DEFAULT NULL COMMENT '活动折扣值 折扣值0.01-100.00',
  `PromotionPrice` decimal(12,2) DEFAULT NULL COMMENT 'Sku活动价格 单位纽币',
  `SavedAmount` decimal(12,2) DEFAULT NULL COMMENT '节省价格 单位纽币',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 操作人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人 操作人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `PromotionEventId` (`PromotionEventId`,`SkuId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='促销活动Sku信息';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Sku`
--

DROP TABLE IF EXISTS `Sku`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Sku` (
  `Id` varchar(255) NOT NULL COMMENT 'Sku编号',
  `ProductId` varchar(255) NOT NULL COMMENT '商品编号',
  `NameCn` varchar(128) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'SKU中文名称',
  `NameEn` varchar(128) NOT NULL COMMENT 'SKU英文名称',
  `SubTitleCn` varchar(128) DEFAULT NULL COMMENT 'SKU中文副标题',
  `SubTitleEn` varchar(128) DEFAULT NULL COMMENT 'SKU英文副标题',
  `Slogan` varchar(128) DEFAULT NULL COMMENT '商品简短广告语',
  `StoreId` bigint NOT NULL COMMENT '商铺Id',
  `StoreName` varchar(128) DEFAULT NULL COMMENT '店铺名称 默认中文，无中文则写英文，冗余字段',
  `CategoryId` int DEFAULT NULL COMMENT '商品类目编号',
  `BrandId` int DEFAULT NULL COMMENT '品牌Id',
  `ImgUrl` varchar(255) DEFAULT NULL COMMENT 'Sku主图',
  `DetailCn` text COMMENT '中文详情 Sku详情,保存htmlencode后的代码',
  `DetailEn` text COMMENT '英文详情 Sku详情,保存htmlencode后的代码',
  `Weight` int DEFAULT NULL COMMENT 'SKU单件重量 unit:g',
  `StockTitle` varchar(32) DEFAULT NULL COMMENT '库存单位',
  `Stock` int NOT NULL DEFAULT '0' COMMENT '库存',
  `Specs` varchar(3072) DEFAULT NULL COMMENT 'SKU对应规格Json Sku',
  `MarketPrice` decimal(12,2) DEFAULT '0.00' COMMENT '市场单价(商户当地货币价格)',
  `SellingPrice` decimal(12,2) NOT NULL COMMENT '日常单价(商户当地货币价格)',
  `CurrencyNameCn` varchar(32) DEFAULT NULL COMMENT '当地货币中文名称 例如:新西兰元',
  `CurrencyNameEn` varchar(32) DEFAULT NULL COMMENT '当地货币英文简称 例如:NZD',
  `Symbol` varchar(32) DEFAULT NULL COMMENT '当地货币通用符号 例如:$',
  `State` int NOT NULL DEFAULT '0' COMMENT '销售状态 0,下架;1,正常;2,违规',
  `Ratings` int DEFAULT NULL COMMENT '好评率 百分比,1-100;',
  `StateDes` varchar(1024) DEFAULT NULL COMMENT '状态说明',
  `VerifyStatus` int NOT NULL DEFAULT '0' COMMENT '审核状态 0,待审核;1,审核通过;2,审核未通过;',
  `Sort` int DEFAULT '1000' COMMENT 'sort index, more bigger, more fronter',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 类目只做软删除',
  `FreeShipping` tinyint unsigned NOT NULL DEFAULT '0' COMMENT '0,不包邮;1,包邮',
  `WechatDetail` text COMMENT 'Sku小程序详情,保存htmlencode后的代码',
  `CostPrice` decimal(12,2) DEFAULT '0.00' COMMENT '成本价',
  `VerifyBy` bigint DEFAULT NULL COMMENT '审核人 审核此sku的人',
  `VerifyTime` datetime DEFAULT NULL COMMENT '审核时间UTC',
  `ShipmentType` tinyint DEFAULT '1' COMMENT '承运方式 0,卖家自寄;1,平台合作物流',
  `IsPerfect` tinyint DEFAULT '0',
  `BarCode` varchar(255) NOT NULL DEFAULT '',
  PRIMARY KEY (`Id`) USING BTREE,
  KEY `StoreId` (`StoreId`),
  KEY `State` (`State`,`VerifyStatus`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='SKU表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SkuAccessData`
--

DROP TABLE IF EXISTS `SkuAccessData`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SkuAccessData` (
  `SkuId` varchar(255) NOT NULL COMMENT 'SKU编号',
  `Accesses` bigint DEFAULT '0' COMMENT '访问量',
  `WishCount` int unsigned NOT NULL DEFAULT '0',
  `Sales` bigint DEFAULT '0' COMMENT '总销量',
  `Collections` bigint DEFAULT '0' COMMENT '收藏数',
  `Comments` bigint DEFAULT '0' COMMENT '评论数',
  `Evaluations` varchar(32) DEFAULT NULL COMMENT '评价数(打星数)',
  `CreateBy` int DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `Revision` int DEFAULT NULL COMMENT '乐观锁',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` int DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  PRIMARY KEY (`SkuId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='Sku访问统计表 异步更新';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SkuExtension`
--

DROP TABLE IF EXISTS `SkuExtension`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SkuExtension` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `StoreId` bigint NOT NULL COMMENT 'keep it redundantly for convenience',
  `SkuId` varchar(255) NOT NULL,
  `ExtensionKey` varchar(128) NOT NULL,
  `ExtensionValue` varchar(256) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `CreateBy` varchar(100) NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` varchar(100) NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Description` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `SkuId` (`SkuId`),
  KEY `ExtensionKey` (`ExtensionKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SkuFavorite`
--

DROP TABLE IF EXISTS `SkuFavorite`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SkuFavorite` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '编号',
  `SkuId` varchar(255) NOT NULL COMMENT 'Sku编号',
  `CreateBy` varchar(255) NOT NULL COMMENT '收藏人 用户MemberId',
  `CreateTime` datetime NOT NULL COMMENT '收藏时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SkuId` (`SkuId`,`CreateBy`),
  KEY `CreateBy` (`CreateBy`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商品收藏表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SkuRate`
--

DROP TABLE IF EXISTS `SkuRate`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SkuRate` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `RootId` bigint NOT NULL DEFAULT '0' COMMENT '源头被回复的评价id',
  `ReplyCount` int NOT NULL DEFAULT '0' COMMENT '> 0 表明此comment有回复',
  `SkuId` varchar(255) NOT NULL COMMENT 'Sku编号',
  `StoreOrderId` varchar(255) NOT NULL COMMENT '店铺订单编号',
  `RateType` int NOT NULL DEFAULT '0' COMMENT '评价类型 0,评价;1,回复;2,商家答复;',
  `CommentId` bigint DEFAULT '0' COMMENT '回复的评价ID',
  `ImgOrVideo` varchar(3072) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT '媒体信息Json数据 图片，视频的链接数据串',
  `Comment` varchar(1024) DEFAULT NULL COMMENT '评价内容',
  `Rating` int NOT NULL COMMENT '评分 10分制:1-10',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '评价用户 打分，评价的用户',
  `CreateTime` datetime DEFAULT NULL COMMENT '评价时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  `Hidden` tinyint DEFAULT '0' COMMENT '是否显示 0,显示;1,隐藏;',
  `Remark` varchar(512) DEFAULT NULL COMMENT '备注',
  PRIMARY KEY (`Id`),
  KEY `RootId` (`RootId`),
  KEY `ReplyCount` (`ReplyCount`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商品评分，评价表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SkuSearchHistory`
--

DROP TABLE IF EXISTS `SkuSearchHistory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SkuSearchHistory` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `MemberId` varchar(255) NOT NULL DEFAULT '',
  `Keyword` varchar(255) NOT NULL DEFAULT '',
  `SearchCount` int NOT NULL DEFAULT '0',
  `SearchType` tinyint NOT NULL DEFAULT '0',
  `CreateTime` datetime NOT NULL,
  `UpdateTime` datetime NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `MemberId` (`MemberId`,`SearchType`,`Keyword`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SkuWishList`
--

DROP TABLE IF EXISTS `SkuWishList`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SkuWishList` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `SkuId` varchar(255) NOT NULL DEFAULT '',
  `MemberId` varchar(255) NOT NULL,
  `LatestAchieveTime` datetime DEFAULT NULL,
  `StoreOrderId` varchar(255) DEFAULT NULL,
  `CreateTime` datetime NOT NULL COMMENT 'a utc time',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SkuId` (`SkuId`,`MemberId`),
  KEY `MemberId` (`MemberId`),
  KEY `CreateTime` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Spec`
--

DROP TABLE IF EXISTS `Spec`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Spec` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `SpecCn` varchar(32) DEFAULT NULL COMMENT '规格中文名',
  `SpecEn` varchar(32) DEFAULT NULL COMMENT '规格英文名',
  `SpecKey` varchar(32) NOT NULL,
  `StoreId` bigint DEFAULT NULL COMMENT 'inidcate which store this spec belongs to. Null represent it is a platform specification',
  `Status` int DEFAULT '1' COMMENT '状态： 0 停用  1 启用',
  `CategoryId` int DEFAULT NULL COMMENT '所属类目',
  `CreateBy` varchar(100) DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(100) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除时间',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SpecKey` (`SpecKey`,`CategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='规格基础表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `SpecValues`
--

DROP TABLE IF EXISTS `SpecValues`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `SpecValues` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `SpecValue` varchar(32) NOT NULL COMMENT '规格值',
  `ProductId` varchar(255) NOT NULL COMMENT '商品ID',
  `SpecId` int DEFAULT NULL COMMENT '规格Id',
  `ImgUrl` varchar(255) DEFAULT NULL COMMENT '规格图片显示',
  `SpecCn` varchar(128) DEFAULT NULL COMMENT '规格中文显示',
  `SpecEn` varchar(128) DEFAULT NULL COMMENT '规格英文显示',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺Id',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除时间',
  `CreateBy` varchar(100) DEFAULT NULL COMMENT '创建人',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(100) DEFAULT NULL COMMENT '更新人',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='规格值表 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Current Database: `YangtaoMerchant`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `YangtaoMerchant` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `YangtaoMerchant`;

--
-- Table structure for table `CollectionPoint`
--

DROP TABLE IF EXISTS `CollectionPoint`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `CollectionPoint` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '自增Id',
  `Name` varchar(32) DEFAULT NULL COMMENT '收寄点名称',
  `CityName` varchar(32) DEFAULT NULL COMMENT '收寄点城市',
  `Address` varchar(32) DEFAULT NULL COMMENT '收寄点地址',
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` bigint DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='公司收寄点';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Store`
--

DROP TABLE IF EXISTS `Store`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `Store` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '商铺编号 从10000开始，10000一下保留',
  `StoreType` int NOT NULL DEFAULT '0' COMMENT 'ordinary: 0, direct-sale: 1, etc',
  `StorekeeperId` varchar(255) NOT NULL COMMENT '店主会员编号',
  `StorekeeperTitle` varchar(64) DEFAULT NULL COMMENT '店主展示名称 默认使用会员昵称，可修改',
  `NameCn` varchar(128) NOT NULL COMMENT '商铺中文名称',
  `NameEn` varchar(128) NOT NULL COMMENT '商铺英文名称',
  `Logo` varchar(255) DEFAULT NULL COMMENT '商铺Logo',
  `Banner` varchar(255) DEFAULT NULL COMMENT '商铺Banner',
  `CountryCode` varchar(10) NOT NULL COMMENT '国家数字编码',
  `CountryEmoji` varchar(32) NOT NULL COMMENT '国家Emoji图标',
  `CityNameCn` varchar(128) DEFAULT NULL COMMENT '所在城市名称中文',
  `CityNameEn` varchar(128) DEFAULT NULL COMMENT '所在城市名称英文',
  `CityCode` varchar(32) DEFAULT NULL COMMENT '城市Code 譬如：Auckland,code为：AK',
  `StartShipCityCode` varchar(32) DEFAULT NULL,
  `StartShipCountryCode` varchar(10) DEFAULT NULL,
  `Address` varchar(1024) DEFAULT NULL COMMENT '店铺所在地址',
  `TelCountryCode` varchar(10) DEFAULT NULL COMMENT 'telephone country code',
  `Phone` varchar(32) DEFAULT NULL COMMENT '联系电话',
  `Email` varchar(255) DEFAULT NULL COMMENT '店铺主联系邮箱 默认店主邮箱',
  `StoreRate` int DEFAULT '6' COMMENT '商铺评分 10分制，6分及格分，默认6',
  `Level` int DEFAULT NULL COMMENT '店铺等级 备用字段',
  `VerifyStatus` int DEFAULT '0' COMMENT '审核状态 0,待审核;1,审核通过;2,审核驳回',
  `StoreStatus` int DEFAULT NULL COMMENT '店铺状态 0,已关闭;1,正常;2,续约中;3,维护中',
  `VerifyBy` bigint DEFAULT NULL COMMENT '入驻审核人 入驻商户时的通过审核的审核人',
  `VerifyTime` datetime DEFAULT NULL COMMENT '审核人核准时间',
  `RentBeginTime` datetime DEFAULT NULL COMMENT '租约起始时间，自审核通过时刻起',
  `RentDueTime` datetime DEFAULT NULL COMMENT '租约结束时间，以审核通过时刻计算',
  `CreateBy` bigint DEFAULT NULL COMMENT '创建人 如果后台创建则操作此字段',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` bigint DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  `Introduction` varchar(3072) DEFAULT NULL COMMENT '商铺简介',
  `DeliverType` varchar(20) DEFAULT NULL COMMENT '1、 DeliverByStore 2、DeliverByPlatform',
  `ShipmentType` int NOT NULL DEFAULT '1' COMMENT '0、 DeliverByStore; 1、 DeliverByPlatform; 2、 DeliveryMixed',
  `BasicShipFee` decimal(12,2) DEFAULT NULL COMMENT 'This field indicate the delivery fee from seller''s store to collection point per order. it takes effect only when seller sellect DeliverByPlatform',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `StorekeeperId` (`StorekeeperId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商铺表';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreAccount`
--

DROP TABLE IF EXISTS `StoreAccount`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreAccount` (
  `StoreId` bigint NOT NULL COMMENT '商铺ID',
  `AccountNo` varchar(128) NOT NULL COMMENT '银行账号',
  `AccountName` varchar(1024) NOT NULL COMMENT '账户名',
  `BankName` varchar(128) NOT NULL COMMENT '银行名称',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 商铺Staff',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人 商铺Staff',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL,
  PRIMARY KEY (`StoreId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商铺结算账户 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreCategory`
--

DROP TABLE IF EXISTS `StoreCategory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreCategory` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT '自增Id',
  `StoreId` bigint DEFAULT NULL COMMENT '店铺ID',
  `CategoryId` int DEFAULT NULL COMMENT '类目Id',
  `CategoryName` varchar(128) DEFAULT NULL COMMENT '类目别名',
  `CategoryRootId` int DEFAULT NULL COMMENT '上级类目Id 初期只定2级类目',
  `CategoryRootName` varchar(128) DEFAULT NULL COMMENT '上级类目别名',
  `CategoryPath` varchar(128) DEFAULT NULL COMMENT '类目path 遍于找上下级类目，半角短横线分隔',
  `Description` varchar(1024) DEFAULT NULL COMMENT '说明',
  `Image` varchar(255) DEFAULT NULL COMMENT '类目图片 冗余字段，备用',
  `Status` int DEFAULT '0' COMMENT '状态 0，待审核；1，已通过；2，未通过',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='店铺经营品类';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreConf`
--

DROP TABLE IF EXISTS `StoreConf`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreConf` (
  `StoreId` bigint NOT NULL COMMENT '商铺编号',
  `CollectPointId` int DEFAULT NULL COMMENT '默认收寄点',
  `DeliverCompanyId` int DEFAULT NULL COMMENT '默认选择物流公司',
  `DeliverCompanyName` varchar(128) DEFAULT NULL COMMENT '默认选择物流公司名称',
  `BasicExpressPrice` decimal(12,2) DEFAULT '0.00' COMMENT '境内基础运费',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  `DeleteTime` datetime DEFAULT NULL COMMENT '软删除 软删除,null为正常,有值即为删除状态',
  PRIMARY KEY (`StoreId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商铺设置 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreDeliveryPriceRule`
--

DROP TABLE IF EXISTS `StoreDeliveryPriceRule`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreDeliveryPriceRule` (
  `Id` int NOT NULL AUTO_INCREMENT COMMENT 'auto increment primary key',
  `StoreId` bigint NOT NULL COMMENT 'Store Id',
  `CompanyId` int DEFAULT NULL COMMENT 'optional id for delivery company，value 0 means this rule an doesn''t depend on delivery company.',
  `StartCountryCode` varchar(128) DEFAULT NULL COMMENT 'Country code of start-point country',
  `StartCityCode` varchar(128) DEFAULT NULL COMMENT 'City code of start-point city',
  `BasicWeight` int NOT NULL DEFAULT '1000' COMMENT 'Basic weight in gram: <= the basic weight, charges the basic price',
  `BasicPrice` decimal(12,2) NOT NULL COMMENT 'Price for basic weight in NZD',
  `StepWeight` int NOT NULL COMMENT 'Step weight in gram',
  `StepPrice` decimal(12,2) NOT NULL COMMENT 'Step price for step weight: <= the step weight, charges the step price',
  `ExtraWeight` int NOT NULL DEFAULT '0' COMMENT 'Extra weight for parcel box, filling etc',
  `MaxItemCountPerParcel` int NOT NULL DEFAULT '10' COMMENT 'maximum item count in one parcel',
  `Currency` int NOT NULL DEFAULT '1' COMMENT '1: NZD 2: AUD 3: JPY',
  `Description` varchar(1024) DEFAULT NULL COMMENT 'Rules in detail',
  `IsDefault` tinyint DEFAULT '0' COMMENT '0,not default rule;1,default rule.',
  `CreateBy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Store manager''s memberId',
  `CreateTime` datetime DEFAULT NULL,
  `UpdateBy` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL COMMENT 'Store manager''s memberId',
  `UpdateTime` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `SCSS` (`StoreId`,`CompanyId`,`StartCountryCode`,`StartCityCode`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='customized shipment price rules made by sellers';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreExtension`
--

DROP TABLE IF EXISTS `StoreExtension`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreExtension` (
  `Id` bigint NOT NULL AUTO_INCREMENT,
  `StoreId` bigint NOT NULL COMMENT 'inidcate which store this setting belongs to ',
  `ExtensionKey` varchar(128) NOT NULL,
  `ExtensionValue` varchar(256) DEFAULT NULL,
  `CreateTime` datetime NOT NULL,
  `CreateBy` varchar(100) NOT NULL,
  `UpdateTime` datetime NOT NULL,
  `UpdateBy` varchar(100) NOT NULL,
  `DeleteTime` datetime DEFAULT NULL,
  `Description` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreNavigation`
--

DROP TABLE IF EXISTS `StoreNavigation`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreNavigation` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增编号',
  `ParentId` int NOT NULL COMMENT '上级ID 根导航为0',
  `StoreId` bigint NOT NULL COMMENT '商铺ID',
  `IdPath` varchar(128) NOT NULL COMMENT 'ID路径 从左到右代表层级关系，短横线分隔：0-1-22',
  `TitleCn` varchar(32) NOT NULL COMMENT '中文标题',
  `TitleEn` varchar(32) NOT NULL COMMENT '英文标题',
  `Url` varchar(255) DEFAULT NULL COMMENT '链接',
  `Target` varchar(32) DEFAULT NULL COMMENT '打开方式 _blank或者当前页面',
  `Image` varchar(255) DEFAULT NULL COMMENT '展示图片',
  `FontIcon` varchar(32) DEFAULT NULL COMMENT '字体图标',
  `Sort` int DEFAULT '200' COMMENT '排序 asc排序',
  `Status` int DEFAULT '0' COMMENT '状态 0，隐藏;1,显示',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 店铺操作人',
  `ModifyBy` varchar(255) DEFAULT NULL COMMENT '更新人 店铺操作人',
  `ModifyTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商铺主导航 ';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `StoreStaff`
--

DROP TABLE IF EXISTS `StoreStaff`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `StoreStaff` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增主键',
  `StoreId` bigint DEFAULT NULL COMMENT '商铺ID',
  `StaffMemberId` varchar(255) DEFAULT NULL COMMENT '员工的会员编号',
  `Status` int DEFAULT NULL COMMENT '状态 0,停用;1,正常;',
  `CreateBy` varchar(255) DEFAULT NULL COMMENT '创建人 系统管理员或操作员',
  `CreateTime` datetime DEFAULT NULL COMMENT '创建时间',
  `UpdateBy` varchar(255) DEFAULT NULL COMMENT '更新人 系统管理员或操作员',
  `UpdateTime` datetime DEFAULT NULL COMMENT '更新时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商铺管理员';
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `WatchedStore`
--

DROP TABLE IF EXISTS `WatchedStore`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!40101 SET character_set_client = utf8 */;
CREATE TABLE `WatchedStore` (
  `Id` bigint NOT NULL AUTO_INCREMENT COMMENT '自增Id',
  `MemberId` varchar(255) NOT NULL COMMENT '收藏用户 系统管理员或操作员',
  `StoreId` bigint NOT NULL COMMENT '商铺Id',
  `CreateTime` datetime NOT NULL COMMENT '收藏时间时间',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci COMMENT='商铺收藏表';
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

