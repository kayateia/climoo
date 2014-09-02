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
	class TestSC : ISecurityContext
	{
		public TestSC( string name )
		{
			this.name = name;
		}

		public string  name
		{
			get; private set;
		}
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
			else if( name == "dumpcontext" )
			{
				var cxt = state.securityContext;
				return "Current context: " + ( cxt == null ? "none" : cxt.name );
			}
			else if( name == "complex" )
			{
				int which = (int)args[0];
				if( which == 1 )
				{
					return new AsyncAction()
					{
						action = AsyncAction.Action.Call,
						function = (FValue)args[1],
						args = new object[] { "added\r\n" },
						frame = new StackTrace.StackFrame()
						{
							line = 0,
							col = 0,
							unitName = "test",
							funcName = "PtTest.{0}".FormatI( name )
						}
					};
				}
				else if( which == 2 )
				{
					return new AsyncAction[]
					{
						new AsyncAction()
						{
							action = AsyncAction.Action.Variable,
							name = "shouldexist",
							value = "oh cool"
						},
						new AsyncAction()
						{
							action = AsyncAction.Action.Callback,
							callback = st =>
								{
									var rv = "{0}".FormatI( st.scope.get( "shouldexist" ) );
									st.pushResult( rv );
								}
						}
					};
				}
				else if( which == 3 )
				{
					var constscope = new ConstScope( state.scope );
					constscope.setConstant( "testconst", "bob" );
					return new AsyncAction[]
					{
						new AsyncAction()
						{
							action = AsyncAction.Action.Code,
							code = Compiler.Compile( "test", @"
def innerfunc(x):
	return [x + 1, testconst]
" )
						},
						new AsyncAction()
						{
							action = AsyncAction.Action.Call,
							name = "innerfunc",
							args = new object[] { 5 },
							frame = new StackTrace.StackFrame()
							{
								line = 0,
								col = 0,
								unitName = "test",
								funcName = "PtTest.{0}".FormatI( name )
							}
						},
						new AsyncAction()
						{
							action = AsyncAction.Action.PushScope,
							scope = constscope
						}
					};
				}
				else if( which == 4 )
				{
					return new AsyncAction[]
					{
						new AsyncAction()
						{
							action = AsyncAction.Action.Code,
							code = Compiler.Compile( "test", @"
def innerfunc(pt):
	return pt.dumpcontext()
" )
						},
						new AsyncAction()
						{
							action = AsyncAction.Action.Call,
							name = "innerfunc",
							args = new object[] { this },
							frame = new StackTrace.StackFrame()
						},
						new AsyncAction()
						{
							action = AsyncAction.Action.PushSecurityContext,
							securityContext = new TestSC( "Context 1" )
						}
					};
				}
				else /* if( which == 5 ) */
				{
					return new AsyncAction[]
					{
						new AsyncAction()
						{
							action = AsyncAction.Action.Code,
							code = Compiler.Compile( "test", @"
def innerfunc(pt):
	return pt.dumpcontext()
" )
						},
						new AsyncAction()
						{
							action = AsyncAction.Action.Call,
							name = "innerfunc",
							args = new object[] { this },
							frame = new StackTrace.StackFrame()
						}
					};
				}
			}
			else
				return null;
		}

		public bool hasMethod( State state, string name )
		{
			return name == "test2" || name == "complex" || name == "dumpcontext";
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

e = """"
def func(x):
	e += x

pt.complex(1, func)
f = pt.complex(2)
g = pt.complex(3)
h = pt.complex(4)
i = pt.complex(5)
j = pt.dumpcontext()
";
		Runner r = new Runner();
		r.pushSecurityContext( new TestSC( "Base Context" ) );
		pter.registerConst( r.state.constScope, "pt" );
		runAndDump( "Passthrough", r, program,
			() => "object dump: {0} {1} {2} {3} {4} {5}\r\n".FormatI(
				dumpObject( pt._a ), dumpObject( pt._b ), dumpObject( pt._c ), dumpObject( pt._d ), dumpObject( pt._e ), dumpObject( pt._f )
			) );
	}
}

}
