/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

ALTER TABLE `mob` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `verb` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `verb` CHANGE COLUMN
  `mobid` `mobid` bigint unsigned NOT NULL;

ALTER TABLE `attribute` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `attribute` CHANGE COLUMN
  `mobid` `mobid` bigint unsigned NOT NULL;

ALTER TABLE `mobtable` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `mobtable` CHANGE COLUMN
  `mobid` `mobid` bigint unsigned NOT NULL;

ALTER TABLE `mobtable` CHANGE COLUMN
  `checkpointid` `checkpointid` bigint unsigned NOT NULL;

ALTER TABLE `checkpoint` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `config` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `config` CHANGE COLUMN
  `checkpointid` `checkpointid` bigint unsigned NOT NULL;

ALTER TABLE `screen` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `user` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;

ALTER TABLE `test` CHANGE COLUMN
  `id` `id` bigint unsigned NOT NULL AUTO_INCREMENT;