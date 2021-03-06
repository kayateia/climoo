﻿/*
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

namespace Kayateia.Climoo.Tests
{
using Kayateia.Climoo.Database;
using NUnit.Framework;

/// <summary>
/// Tests for the DatabaseMySql assembly
/// </summary>
/// <remarks>
/// This requires the Test table to be installed in the database, which it ought to
/// be if you used the mysql.sql file to create it. You should NOT point this at your
/// production database.
/// </remarks>
[TestFixture]
public class DatabaseMySqlTest
{
	[Test]
	public void connectionTest()
	{
		_dbc.connectionTest( new MySqlDatabase() );
	}

	[Test]
	public void simpleInsertAndSelectTest()
	{
		_dbc.simpleInsertAndSelectTest( new MySqlDatabase() );
	}

	DatabaseCommon _dbc = new DatabaseCommon();
}

}
