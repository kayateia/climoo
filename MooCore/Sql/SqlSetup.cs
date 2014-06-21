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

namespace Kayateia.Climoo.MooCore.Sql {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Doesn't actually work right now.
static public class SqlSetup {
	[STAThread]
	static void Main(string[] args) {
		Console.WriteLine("Creating SQL database...");
		using (var context = new Sql.MooCoreSqlDataContext()) {
			context.Connection.Open();
			context.CreateDatabase();
		}
		Console.WriteLine("Creating default world...");
		MooCore.World w = MooCore.World.CreateDefault();
		w.saveToSql();
		Console.WriteLine("Database created.");
	}
}

}
