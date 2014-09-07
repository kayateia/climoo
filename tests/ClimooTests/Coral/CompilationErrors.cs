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
	public void CompilationErrors()
	{
		string rv = "";

		rv += "Test 1:\r\n";
		rv += catchCompileErrors( @"
// some text
def thisworks(args):
	pass
"
		);

		rv += "Test 2:\r\n";
		rv += catchCompileErrors( @"
// some text
def this is a parsing error(args):
	pass
"
		);

		rv += "Test 3:\r\n";
		rv += catchCompileErrors( @"
// some text
def thisworks(args):
	if args.length() > 5:
		args = [asdf qwef asdf asdf]
"
		);

		TestCommon.CompareRef( Path.Combine( "Coral", "CompilationErrors" ), rv );
	}

	string catchCompileErrors( string code )
	{
		string rv = "";
		try
		{
			CodeFragment cf = Compiler.Compile( "test", code );
			if( cf.success )
				rv += "compiled successfully\r\n";
			else
			{
				foreach( var err in cf.errors )
				{
					rv += "line {0} col {1}: {2}\r\n".FormatI( err.line, err.col, err.message );
				}
			}
		} catch( Exception ex )
		{
			rv += "exception:\r\n" + ex.ToStringI() + "\r\n";
		}

		return rv;
	}
}

}
