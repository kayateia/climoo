﻿#region License
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
	public void Functions()
	{
		string program = @"
def fib(n):
    if n == 0:
        return 1
    elif n == 1:
        return 1
    else:
        return fib(n-2) + fib(n-1)
x = fib(5)

out = """"
def test(i):
	pass
	for j in i:
		out += ""foo""
y = test([1,2,3,4])

out2 = 0
out3 = 0
def test2(i):
	out2 = arguments
	out3 = i
test2(1, 2, 3)

outi = 0
outj = 0
outk = 0
def test3(i, j, k):
	outi = i
	outj = j
	outk = k
test3(1)
";
		runAndDump( "Functions", program );
	}
}

}
