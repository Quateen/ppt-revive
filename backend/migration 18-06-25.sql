CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `AuthPolicy` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserType` int NOT NULL,
    `Enforce2FactorVerification` tinyint(1) NOT NULL,
    `EnforcePasswordChangeOnFirstLogin` tinyint(1) NOT NULL,
    `EnforceBackendActivation` tinyint(1) NOT NULL,
    `EnforceEmailConfirmation` tinyint(1) NOT NULL,
    `EnforceMobileConfirmation` tinyint(1) NOT NULL,
    `EnforceProfileCompletion` tinyint(1) NOT NULL,
    `BaseTokenDurationMinutes` int NOT NULL,
    `FullTokenDurationMinutes` int NOT NULL,
    `RefreshTokenDurationMinutes` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_AuthPolicy` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Right` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_Right` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Role` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `Description` varchar(500) CHARACTER SET utf8mb4 NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_Role` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `User` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `FirstName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DisplayName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Address` longtext CHARACTER SET utf8mb4 NULL,
    `LastLoginDate` datetime(6) NULL,
    `PasswordChangedAt` datetime(6) NULL,
    `UserTypeId` int NOT NULL,
    `AuthKey` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SmsKey` longtext CHARACTER SET utf8mb4 NOT NULL,
    `EmailKey` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsProfileCompleted` tinyint(1) NOT NULL,
    `LastLoginAttempt` int NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_User` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `RoleRight` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` int NOT NULL,
    `RightId` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_RoleRight` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_RoleRight_Right_RightId` FOREIGN KEY (`RightId`) REFERENCES `Right` (`Id`),
    CONSTRAINT `FK_RoleRight_Role_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Role` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `LoginAttempts` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `RejectionType` int NOT NULL,
    `LoginType` int NOT NULL,
    `IsRevoked` tinyint(1) NOT NULL,
    `AttemptDate` datetime(6) NOT NULL,
    `IsSuccess` tinyint(1) NOT NULL,
    `IpAddress` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ClientInformation` longtext CHARACTER SET utf8mb4 NOT NULL,
    `TwoFacVerified` tinyint(1) NOT NULL,
    `DeviceRegistration` longtext CHARACTER SET utf8mb4 NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_LoginAttempts` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_LoginAttempts_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserProfile` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `FirstName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LastName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Address` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `MobileNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DateOfBirth` date NOT NULL,
    `Gender` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_UserProfile` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserProfile_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserRight` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `RightId` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_UserRight` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserRight_Right_RightId` FOREIGN KEY (`RightId`) REFERENCES `Right` (`Id`),
    CONSTRAINT `FK_UserRight_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserRole` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `RoleId` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_UserRole` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserRole_Role_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `Role` (`Id`),
    CONSTRAINT `FK_UserRole_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_LoginAttempts_UserId` ON `LoginAttempts` (`UserId`);

CREATE INDEX `IX_RoleRight_RightId` ON `RoleRight` (`RightId`);

CREATE INDEX `IX_RoleRight_RoleId` ON `RoleRight` (`RoleId`);

CREATE INDEX `EmailIndex` ON `User` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `User` (`NormalizedUserName`);

CREATE UNIQUE INDEX `IX_UserProfile_UserId` ON `UserProfile` (`UserId`);

CREATE INDEX `IX_UserRight_RightId` ON `UserRight` (`RightId`);

CREATE INDEX `IX_UserRight_UserId` ON `UserRight` (`UserId`);

CREATE INDEX `IX_UserRole_RoleId` ON `UserRole` (`RoleId`);

CREATE INDEX `IX_UserRole_UserId` ON `UserRole` (`UserId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250508131228_InitialMigration', '8.0.2');

COMMIT;

START TRANSACTION;

ALTER TABLE `User` ADD `GoogleAud` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `GoogleAzp` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `GoogleExp` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `GoogleIat` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `GoogleIss` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `GooglePicture` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `GoogleSub` varchar(200) CHARACTER SET utf8mb4 NULL;

ALTER TABLE `User` ADD `SocialApp` varchar(200) CHARACTER SET utf8mb4 NULL;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250616103050_GoogleLoginFieldsAdded', '8.0.2');

COMMIT;

START TRANSACTION;

CREATE TABLE `Subscriptions` (
    `Id` int NOT NULL,
    `Name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `PeriodInDays` int NOT NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_Subscriptions` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `UserSubscriptions` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `SubscriptionId` int NOT NULL,
    `StartDateTime` datetime(6) NOT NULL,
    `EndDateTime` datetime(6) NOT NULL,
    `SubscriptionId1` int NULL,
    `UserId1` int NULL,
    `Created` datetime(6) NOT NULL,
    `CreatedBy` int NULL,
    `LastModified` datetime(6) NOT NULL,
    `LastModifiedBy` int NULL,
    CONSTRAINT `PK_UserSubscriptions` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_UserSubscriptions_Subscriptions_SubscriptionId` FOREIGN KEY (`SubscriptionId`) REFERENCES `Subscriptions` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserSubscriptions_Subscriptions_SubscriptionId1` FOREIGN KEY (`SubscriptionId1`) REFERENCES `Subscriptions` (`Id`),
    CONSTRAINT `FK_UserSubscriptions_User_UserId` FOREIGN KEY (`UserId`) REFERENCES `User` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_UserSubscriptions_User_UserId1` FOREIGN KEY (`UserId1`) REFERENCES `User` (`Id`)
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_UserSubscriptions_SubscriptionId` ON `UserSubscriptions` (`SubscriptionId`);

CREATE INDEX `IX_UserSubscriptions_SubscriptionId1` ON `UserSubscriptions` (`SubscriptionId1`);

CREATE INDEX `IX_UserSubscriptions_UserId` ON `UserSubscriptions` (`UserId`);

CREATE INDEX `IX_UserSubscriptions_UserId1` ON `UserSubscriptions` (`UserId1`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20250616122939_SubscriptionTablesAdded', '8.0.2');

COMMIT;

