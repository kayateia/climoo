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
	// Works \m/
	[Test]
	public void Metal()
	{
		Runner r = new Runner();
		Action<string> adder = s =>
			r.state.scope.set( "output",
				r.state.scope.get( "output" ) + s + "\r\n" );
		r.state.scope.set( "testfunc",
			new FValue( (state, ps) => adder( "Native write: {0}\r\n".FormatI( ps[0] ) ) )
			{
				scope = r.state.scope
			}
		);
		r.state.scope.set( "metal",
			new MetalObject()
			{
				indexLookup = (state, idx) =>
				{
					adder( "Index lookup for {0}".FormatI( idx ) );
					state.pushResult( new LValue()
					{
						read = st => { adder( "Index {0} was read".FormatI( idx ) ); return 0; },
						write = (st,val) => { adder( "Index {0} was written: {1}".FormatI( idx, val ) ); }
					} );
				},
				memberLookup = (state, name) =>
				{
					adder( "Member lookup for {0}".FormatI( name ) );
					state.pushResult( new LValue()
					{
						read = st => { adder( "Member {0} was read".FormatI( name ) ); return 0; },
						write = (st,val) => { adder( "Member {0} was written: {1}".FormatI( name, val ) ); }
					} );
				}
			}
		);
		r.state.scope.set( "output", "" );
		r.setScopeCallback( st =>
			{
				if( st.StartsWithI( "#" ) )
				{
					return st;
				}
				else
					return null;
			}
		);

		string program = @"
testfunc(""stuff!"")
a = metal.foo
metal.foo = 10
metal.bar = a
b = metal[5]
metal[""bear""] = ""kitten""
c = #10
";
		runAndDump( "Metal", r, program, null );
	}
}

}
