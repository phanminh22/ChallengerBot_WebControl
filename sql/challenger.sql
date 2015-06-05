-- phpMyAdmin SQL Dump
-- version 4.4.7
-- http://www.phpmyadmin.net
--
-- Host: 127.0.0.1
-- Generation Time: Jun 06, 2015 at 12:22 AM
-- Server version: 5.6.17
-- PHP Version: 5.5.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Database: `challenger`
--

-- --------------------------------------------------------

--
-- Table structure for table `accounts`
--

CREATE TABLE IF NOT EXISTS `accounts` (
  `id` int(7) NOT NULL,
  `level` int(2) NOT NULL DEFAULT '1',
  `money` int(11) NOT NULL DEFAULT '0',
  `account` varchar(64) NOT NULL,
  `password` varchar(64) NOT NULL,
  `champion` varchar(64) NOT NULL,
  `queue` int(3) NOT NULL DEFAULT '32',
  `difficulty` enum('EASY','MEDIUM') NOT NULL,
  `maxlevel` int(2) NOT NULL DEFAULT '30',
  `autoboost` tinyint(1) NOT NULL DEFAULT '0',
  `connected` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `console`
--

CREATE TABLE IF NOT EXISTS `console` (
  `id` int(7) NOT NULL,
  `content` text NOT NULL,
  `timestamp` int(11) NOT NULL,
  `player` varchar(64) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `settings`
--

CREATE TABLE IF NOT EXISTS `settings` (
  `label` varchar(21) NOT NULL DEFAULT 'CBot',
  `players` int(3) NOT NULL DEFAULT '5',
  `platform` varchar(32) NOT NULL DEFAULT 'EUW',
  `difficulty` enum('-','EASY','MEDIUM') NOT NULL DEFAULT 'MEDIUM',
  `queue` int(3) NOT NULL DEFAULT '33',
  `gamepath` text NOT NULL,
  `response` int(11) NOT NULL DEFAULT '0',
  `hashid` varchar(64) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Dumping data for table `settings`
--

INSERT INTO `settings` (`label`, `players`, `platform`, `difficulty`, `queue`, `gamepath`, `response`, `hashid`) VALUES
('CBot', 3, 'EUW', 'MEDIUM', 33, 'D:\\Program files\\League of Legends\\', 0, '0');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `account` (`account`);

--
-- Indexes for table `console`
--
ALTER TABLE `console`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `settings`
--
ALTER TABLE `settings`
  ADD PRIMARY KEY (`label`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `accounts`
--
ALTER TABLE `accounts`
  MODIFY `id` int(7) NOT NULL AUTO_INCREMENT;
--
-- AUTO_INCREMENT for table `console`
--
ALTER TABLE `console`
  MODIFY `id` int(7) NOT NULL AUTO_INCREMENT;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
