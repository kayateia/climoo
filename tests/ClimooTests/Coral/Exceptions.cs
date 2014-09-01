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
[TestFixture]
public partial class CoralTest
{
	[Test]
	public void Exceptions()
	{
		string program = @"
bb = 0
cc = 0
dd = 0

def deeper():
	try:
		throw { ""name"":""bar"" }
	except:
		throw { ""name"":""baz"" }
	finally:
		dd = ""yep""
try:
	a = 10
	// throw { ""name"":""Foo"" }
	deeper()
except b:
	bb = b
finally:
	cc = ""foo""

ee = 0
try:
	try:
		throw { ""name"": ""e"" }
	except:
		pass
	finally:
		throw { ""name"": ""f"" }
except c:
	ee = c

ff = 0
try:
	bob = 10 / 0
except c:
	ff = c
";
		runAndDump( "Exceptions", new Runner(), program, () =>
			{
				string program2 = @"throw { ""name"": ""escaped"", ""message"":""Whee, I'm free!"" }";
				CodeFragment cf = Compiler.Compile( "test", program2 );
				try
				{
					var r = new Runner();
					r.runSync( cf );
					return "second test failed";
				}
				catch( CoralException ex )
				{
					return "second test succeeded: {0}, {1}".FormatI( ex.name, ex.Message );
				}
			}
		);
	}
}

}
