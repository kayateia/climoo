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
	// Needs "continue" once that's in. Otherwise works.
	[Test]
	public void For()
	{
		string program = @"
j = 0
for i in [1,2,3,4,5]:
	j += i
	if i == 3:
		break

k = 0
for i in [1,2,3,4,5]:
	if i == 3:
		continue
	k += i

m = 0
for l=5,l<10,l+=1:
	m += l

n = 0
for l=5,l<10,l+=1:
	n += l
	if l == 7:
		break

p = 0
for l=5,l<10,l+=1:
	if l == 7:
		continue
	p += l
";
		runAndDump( "For", program );
	}
}

}
