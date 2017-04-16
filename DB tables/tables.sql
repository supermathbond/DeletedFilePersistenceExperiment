-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Server version:               10.1.21-MariaDB - mariadb.org binary distribution
-- Server OS:                    Win32
-- HeidiSQL Version:             9.3.0.4984
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;

-- Dumping database structure for dfpt
CREATE DATABASE IF NOT EXISTS `dfpt` /*!40100 DEFAULT CHARACTER SET latin1 */;
USE `dfpt`;


-- Dumping structure for table dfpt.agentstatus
CREATE TABLE IF NOT EXISTS `agentstatus` (
  `ID` int(11) NOT NULL,
  `Status` varchar(40) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.clientsdetails
CREATE TABLE IF NOT EXISTS `clientsdetails` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `Cpu` varchar(70) DEFAULT NULL,
  `Ram` double DEFAULT NULL,
  `FreeSpace` bigint(20) DEFAULT NULL,
  `DiskSize` bigint(20) DEFAULT NULL,
  `OS` varchar(40) DEFAULT NULL,
  `ServicePack` varchar(20) DEFAULT NULL,
  `is64Bit` tinyint(1) DEFAULT NULL,
  `HostName` varchar(30) DEFAULT NULL,
  `UserName` varchar(30) DEFAULT NULL,
  `Version` varchar(15) DEFAULT NULL,
  `CurrentFolder` varchar(1024) DEFAULT NULL,
  `isFromCloud` tinyint(1) NOT NULL,
  `FreeText` varchar(1024) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.errors
CREATE TABLE IF NOT EXISTS `errors` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `ClientId` int(11) NOT NULL,
  `Error` varchar(4096) NOT NULL,
  `TimeOfError` datetime NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.experimentdetails
CREATE TABLE IF NOT EXISTS `experimentdetails` (
  `ExpId` int(11) NOT NULL AUTO_INCREMENT,
  `DateOfAttack` datetime NOT NULL,
  `TimeGap` int(11) NOT NULL,
  `ClusterData` text NOT NULL,
  `NumOfClustersToCreate` int(11) NOT NULL,
  PRIMARY KEY (`ExpId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.experimentresultssummary
CREATE TABLE IF NOT EXISTS `experimentresultssummary` (
  `resultSummaryId` bigint(20) NOT NULL AUTO_INCREMENT,
  `MissionId` bigint(20) NOT NULL,
  `DateTime` datetime NOT NULL,
  `HowManyAlive` int(11) NOT NULL,
  PRIMARY KEY (`resultSummaryId`),
  KEY `MissionId` (`MissionId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.keepalive
CREATE TABLE IF NOT EXISTS `keepalive` (
  `ClientId` int(11) NOT NULL,
  `KeepAliveDate` datetime NOT NULL,
  `statusId` int(11) DEFAULT NULL,
  PRIMARY KEY (`ClientId`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.missions
CREATE TABLE IF NOT EXISTS `missions` (
  `ID` int(11) NOT NULL AUTO_INCREMENT,
  `ClientID` int(11) DEFAULT NULL,
  `isMissionFinished` tinyint(1) NOT NULL,
  `MissionTypeID` int(11) NOT NULL,
  `DateTime` datetime NOT NULL,
  `TempFolder` varchar(200) DEFAULT NULL,
  `ExpID` int(11) DEFAULT NULL,
  `MoreDetails` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.


-- Dumping structure for table dfpt.missiontypes
CREATE TABLE IF NOT EXISTS `missiontypes` (
  `ID` int(11) NOT NULL,
  `MissionType` varchar(18) NOT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- Data exporting was unselected.
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
