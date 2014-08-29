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
using System.IO;
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
f = !e.bar
g = e.foogle
h = e[""boogle""]
i = null
j = !null
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

	class PtTest : IExtensible
	{
		[CoralPassthrough]
		public void test( int a, string b, string[] c, bool d, PtTest e )
		{
			_a = a;
			_b = b;
			_c = c;
			_d = d;
			_e = e;
			_f = "";
		}

		public int _a;
		public string _b;
		public string[] _c;
		public bool _d;
		public PtTest _e;
		public string _f;

		[CoralPassthrough]
		public string property
		{
			get
			{
				return _prop;
			}

			set
			{
				_prop = value;
			}
		}
		public string _prop;

		public object getProperty( State state, string name )
		{
			if( name == "arbitrary" )
				return "it's arbitrary, yo";
			else
				return "something else";
		}

		public bool hasProperty( State state, string name )
		{
			return name == "arbitrary" || name == "other";
		}

		public void setProperty( State state, string name, object value )
		{
			if( name == "arbitrary" )
				_f = (string)value;
		}

		public object callMethod( State state, string name, object[] args )
		{
			if( name == "test2" )
				return "test worked " + String.Join( ",", args.Select( x => x.ToStringI() ).ToArray() );
			else
				return null;
		}

		public bool hasMethod( State state, string name )
		{
			return name == "test2";
		}
	}

	// Works
	[Test]
	public void Passthrough()
	{
		PtTest pt = new PtTest();
		Passthrough pter = new Passthrough( pt );

		string program = @"
pt.test(5, ""bob"", [""1"", ""2""], true, pt)
pt.property = ""bar""
a = pt.property
b = pt.arbitrary
c = pt.other
pt.arbitrary = ""new value""
d = pt.arbitrary
pt.test2(1, 2, ""3"")
";
		Runner r = new Runner();
		pter.registerConst( r.state.constScope, "pt" );
		runAndDump( "Passthrough", r, program,
			() => "object dump: {0} {1} {2} {3} {4} {5}\r\n".FormatI(
				dumpObject( pt._a ), dumpObject( pt._b ), dumpObject( pt._c ), dumpObject( pt._d ), dumpObject( pt._e ), dumpObject( pt._f )
			) );
	}

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
				object rv = r.callFunction( "fib", new object[] { 5 }, typeof( int ) );
				return "function return value: {0}".FormatI( dumpObject( rv ) );
			}
		);
	}

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
				CodeFragment cf = Compiler.Compile( program2 );
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
			CodeFragment cf = Compiler.Compile( code );
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

	void runAndDump( string name, string code )
	{
		Runner r = new Runner();
		runAndDump( name, r, code, null );
	}

	void runAndDump( string name, Runner r, string code, Func<string> extra )
	{
		CodeFragment cf = Compiler.Compile( code );
		r.runSync( cf );
		string results = dumpScope( r.state );
		if( extra != null )
			results += extra();
		TestCommon.CompareRef( Path.Combine( "Coral", name ), results );
	}

	string dumpScope( State s )
	{
		string rv = "";
		string[] names = s.scope.getNames();
		foreach( string n in names )
		{
			// Don't include things from the const values.
			if( s.constScope.has( n ) )
				continue;

			object val = s.scope.get( n );
			rv += "{0} = {1}\r\n".FormatI( n, dumpObject( val ) );
		}

		return rv;
	}

	string dumpObject( object o )
	{
		if( o is List<object> )
			return dumpArray( (List<object>)o );
		else if( o is string[] )
			return dumpArray( new List<object>( (string[])o ) );
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
