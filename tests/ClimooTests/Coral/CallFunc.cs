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
	public void CallFunc()
	{
		string program = @"
def fib(n):
    if n == 0:
        return 1
    elif n == 1:
        return 1
    else:
        return fib(n-2) + fib(n-1)
";
		Runner r = new Runner();
		runAndDump( "CallFunc", r, program, () =>
			{
				object rv = r.callFunction( "fib", new object[] { 5 }, typeof( int ), new StackTrace.StackFrame() );
				return "function return value: {0}".FormatI( dumpObject( rv ) );
			}
		);
	}
}

}
