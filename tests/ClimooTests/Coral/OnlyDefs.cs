#region License
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
#endregion
namespace Kayateia.Climoo.Tests
{
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Kayateia.Climoo.Scripting.Coral;

/// <summary>
/// Tests for the Coral scripting language.
/// </summary>
public partial class CoralTest
{
	[Test]
	public void OnlyDefs()
	{
		string progPositive1 = @"
def a():
	pass
";
		string progPositive2 = @"
def a():
	pass
def b():
	pass
";
		string progNegative1 = @"
if a:
	pass
";
		string progNegative2 = @"
def a():
	pass
if b:
	pass
";
		string rv = "";
		rv += testOnlyDefs( progPositive1 );
		rv += testOnlyDefs( progPositive2 );
		rv += testOnlyDefs( progNegative1 );
		rv += testOnlyDefs( progNegative2 );
		TestCommon.CompareRef( Path.Combine( "Coral", "OnlyDefs" ), rv );
	}

	string testOnlyDefs( string code )
	{
		var cf = Compiler.Compile( "test", code );
		return "Test is{0} compliant\r\n".FormatI( cf.verifyOnlyDefs() ? "": " not" );
	}
}

}
