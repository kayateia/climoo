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
	public void While()
	{
		string program = @"
i = """"
j = 0
while j < 10:
	i += ""a""
	j += 1

k = """"
l = 0
while l < 10:
	k += ""b""
	l += 1
	if l >= 5:
		break

m = -1
ns = [""a"", ""b"", ""c"", ""d"", ""e""]
n = """"
while m < 5:
	m += 1
	if m == 3:
		continue
	n += ns[m]
";
		runAndDump( "While", program );
	}
}

}
