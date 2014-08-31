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
	void runAndDump( string name, string code )
	{
		Runner r = new Runner();
		runAndDump( name, r, code, null );
	}

	void runAndDump( string name, Runner r, string code, Func<string> extra )
	{
		CodeFragment cf = Compiler.Compile( "test", code );
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
