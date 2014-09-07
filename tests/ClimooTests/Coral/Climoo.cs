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
	class ClimooObj : IExtensible
	{
		public ClimooObj( string code )
		{
			_code = Compiler.Compile( "second", code );
		}

		CodeFragment _code;

		public object getProperty( State state, string name ) { return null; }
		public bool hasProperty( State state, string name ) { return false; }
		public void setProperty( State state, string name, object value ) { }

		public object callMethod( State state, string name, object[] args )
		{
			if( name == "other" )
			{
				Runner r = new Runner();
				r.runSync( _code );
				return new AsyncAction()
				{
					action = AsyncAction.Action.Call,
					function = (FValue)r.state.scope.get( "verb" ),
					args = args,
					frame = new StackTrace.StackFrame()
				};
			}
			else if( name == "thrower" )
			{
				Runner r = new Runner();
				r.runSync( _code );
				return new AsyncAction()
				{
					action = AsyncAction.Action.Call,
					function = (FValue)r.state.scope.get( "thrower" ),
					args = args,
					frame = new StackTrace.StackFrame()
				};
			}
			else
				throw new NotImplementedException();
		}

		public bool hasMethod( State state, string name ) { return true; }

		// This is tested elsewhere.
		public CoralException filterException( Exception ex ) { return CoralException.GetForAny( ex ); }
	}

	[Test]
	public void Climoo()
	{
		// This test does its best to simulate some conditions that might occur during
		// normal use within Climoo. It tests things like cross-module calls and exceptions.
		string program1 = @"
def verb(arg1, arg2):
	arg1.other(arg2)

def verb2(arg1):
	arg1.thrower()
";
		string program2 = @"
def verb(arg):
	arg.foo = ""test""

def thrower():
	inner()

def inner():
	throw { ""name"":""inner_exception"" }
";
		CodeFragment prog1 = Compiler.Compile( "verb1", program1 );
		var obj = new ClimooObj( program2 );

		Runner r = new Runner();
		r.runSync( prog1 );
		var dictObj = new Dictionary<object, object>();
		dictObj["foo"] = "unset";
		r.state.scope.set( "test", dictObj );

		string rv = "";
		try
		{
			r.callFunction( "verb", new object[] {
				obj,
				dictObj
			}, typeof( object ), new StackTrace.StackFrame() );
		}
		catch( Exception ex )
		{
			rv += "Exception: " + ex + "\r\n";
		}

		try
		{
			r.callFunction( "verb2", new object[] {
				obj
			}, typeof( object ), new StackTrace.StackFrame() );
		}
		catch( CoralException ex )
		{
			rv += "CoralException: " + dumpObject( ex.data ) + "\r\n";
		}
		catch( Exception ex )
		{
			rv += "Exception: " + ex + "\r\n";
		}

		string results = dumpScope( r.state ) + rv;
		TestCommon.CompareRef( Path.Combine( "Coral", "Climoo" ), results );
	}
}

}
