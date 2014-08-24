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

namespace Kayateia.Climoo.Tests
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Kayateia.Climoo.Scripting.Coral;

/// <summary>
/// This simple test just tests NUnit's functioning. This may go away eventually.
/// </summary>
[TestFixture]
public class CoralTest
{
	// Fails due to mystery empty function definition error
	[Test]
	public void Parser()
	{
		// The goal here is not to get an exception during parsing.
		// Basically everything else will be tested in further tests.
		//
		// Quite a few things are commented out; these aren't in the AST compiler yet.
		string program = @"
// single line comment
/*
  multi
  line
  comment
*/
a[:-5]
a[b.f.a(10,a):]
a[2:5]
[1,2,3][5] = 10
a(f).b.c = 4+5
bar(b)
foo.bar(b)
if c:
//    a = bob.foo + $.baz() + #10.woot
	pass
elif d:
    d = 10
elif d:
    d = 10
else:
    b = a + c
    def bob(x):
        return x
return b
// #10.woot = 5
// $.bar = ""baz""
/*try:
    a = b
except FooException:
    b = c
except BarException bar:
    c = d
finally:
    d = e */
f = [1,2,3,4]
g = [a(1), b(2)]
h = { 1:2, 4:[1, 2, 3] }

for x in g:
    print(x)
	break
//for x=0,x<5,x++:
//    print(x)
    
def fib(n):
    if n == 0:
        return 1
    elif n == 1:
        return 1
    else:
        return fib(n-2) + fib(n-1)

x = fib(5)
";
		CodeFragment cf = Compiler.Compile( program );
	}

	// Works
	[Test]
	public void Arrays()
	{
		string program = @"
a = [1,2,3,4,5,6]
b = a[3]
c = [1, ""foo"", b]
d = a[:2]
e = a[2:]
f = a[1:3]
g = a[1:-1]
h = a[-4:20]
";
		runAndDump( "Arrays", program );
	}

	[Test]
	public void Dictionaries()
	{
		string program = @"
a = { 1:10, ""foo"":[1,2,3,4] }
b = a[1]
c = a[""foo""]
d = c[1]
e = a.foo[1]
";
		runAndDump( "Dictionaries", program );
	}

	// Works
	[Test]
	public void Assignment()
	{
		string program = @"
a = 5
b = 10
c = a
d = a + c
e = {}
e.bar = ""baz""
";
		runAndDump( "Assignment", program );
	}

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
";
		runAndDump( "For", program );
	}

	// Works
	[Test]
	public void If()
	{
		string program = @"
a = 5

if a < 10:
	b = a

if a < 4:
	c = ""oops""
else:
	c = ""yay""

if a < 4:
	d = ""oops1""
elif a == 5:
	d = ""yay2""
";
		runAndDump( "If", program );
	}

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
test([1,2,3,4])
";
		runAndDump( "Functions", program );
	}

	// Works
	[Test]
	public void Strings()
	{
		string program = @"
a = ""bob""
b = a[1]
c = a.length()
d = ""{0} {0}"".format(b)
";
		runAndDump( "Strings", program );
	}

	// Works \m/
	[Test]
	public void Metal()
	{
		Runner r = new Runner();
		Action<string> adder = s =>
			r.state.scope.set( "output",
				r.state.scope.get( "output" ) + s + "\n" );
		r.state.scope.set( "testfunc",
			new FValue( (state, ps) => adder( "Native write: {0}\n".FormatI( ps[0] ) ) )
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

		string program = @"
testfunc(""stuff!"")
a = metal.foo
metal.foo = 10
metal.bar = a
b = metal[5]
metal[""bear""] = ""kitten""
";
		runAndDump( "Metal", r, program );
	}

	void runAndDump( string name, string code )
	{
		Runner r = new Runner();
		runAndDump( name, r, code );
	}

	void runAndDump( string name, Runner r, string code )
	{
		CodeFragment cf = Compiler.Compile( code );
		r.runSync( cf );
		string results = dumpScope( r.state );
		// TestCommon.CompareRef( testName, results );
	}

	string dumpScope( State s )
	{
		string rv = "";
		string[] names = s.scope.getNames();
		foreach( string n in names )
		{
			object val = s.scope.get( n );
			rv += "{0} = {1}\n".FormatI( n, dumpObject( val ) );
		}

		return rv;
	}

	string dumpObject( object o )
	{
		if( o is List<object> )
			return dumpArray( (List<object>)o );
		else if( o is Dictionary<object,object> )
			return dumpDict( (Dictionary<object,object>)o );
		else if( o is FValue )
			return dumpFValue( (FValue)o );
		else
			return o.ToStringI();
	}

	string dumpArray( List<object> arr )
	{
		return "[{0}]".FormatI( String.Join( ",", ( arr.Select( i => dumpObject( i ) ).ToArray() ) ) );
	}

	string dumpDict( Dictionary<object, object> dict )
	{
		return "{{{0}}}".FormatI( String.Join( ",", ( dict.Select(
			kv => "{0}:{1}".FormatI( dumpObject( kv.Key ), dumpObject( kv.Value ) )
		).ToArray() ) ) );
	}

	string dumpFValue( FValue fv )
	{
		if( fv.func != null )
			return fv.func.ToStringI();
		else
			return "<metal function>";
	}
}

}
