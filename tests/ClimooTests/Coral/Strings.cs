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
	// Works
	[Test]
	public void Strings()
	{
		string program = @"
a = ""bob""
b = a[1]
c = a.length()
d = ""{0} {0}"".format(b)
e = ""{0}_{0}""
f = e.format(b)
g = ""a b c d"".split("" "")
h = ""a b c d"".split("" "",2)
i = ""a b,c d"".split(["" "", "",""],2)
j = string.join("","", [""bar"",1,b])
k = ""test"".replace(""es"", ""o"")
";
		runAndDump( "Strings", program );
	}
}

}
