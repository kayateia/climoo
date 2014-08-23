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
namespace Kayateia.Climoo.Scripting.Coral
{
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// For loops. This is for both "foreach" style as well as C-style.
/// </summary>
class AstFor : AstNode
{
	/// <summary>
	/// The name of the loop variable we'll use in foreach mode.
	/// </summary>
	public string loopVariable { get; private set; }

	/// <summary>
	/// The set of values we'll loop over in foreach mode.
	/// </summary>
	public AstNode loopOver { get; private set; }

	/// <summary>
	/// The code we'll run per loop.
	/// </summary>
	public AstNode block { get; private set; }

	public override bool convert( Irony.Parsing.ParseTreeNode node )
	{
		if( node.Term.Name == "ForStmt" && node.ChildNodes.Count == 5 && node.ChildNodes[2].Token.Text == "in" )
		{
			// Get our loop variable.
			this.loopVariable = node.ChildNodes[1].Token.Text;

			// Convert the loop-over expression.
			this.loopOver = Compiler.ConvertNode( node.ChildNodes[3] );

			// Convert the inner block.
			this.block = Compiler.ConvertNode( node.ChildNodes[4] );

			return true;
		}

		return false;
	}

	// This IEnumerable implementation will be very inefficient for large arrays.
	void oneIteration( State state, IEnumerable<object> array, int curIndex )
	{
		if( curIndex < array.Count() - 1 )
		{
			state.pushAction( new Step( this,
				st =>
				{
					oneIteration( st, array, curIndex + 1 );
				},
				"for: next iteration" )
			);
		}

		this.block.run( state );

		state.pushAction( new Step( this,
			st =>
			{
				st.scope.set( this.loopVariable, array.Skip( curIndex ).First() );
			},
			"for: assign iterator " + this.loopVariable )
		);
	}

	public override void run( State state )
	{
		// We execute by first evaluating the loopOver and then pushing one iteration of
		// the loop onto the action stack. Each iteration, if it completes successfully,
		// will push the next on. The array and current index are curried.
		state.pushAction( new Step( this, st =>
		{
			object over = st.popResult();
			IEnumerable<object> overTyped;
			if( over is List<object> )
				overTyped = (IEnumerable<object>)over;
			else if( over is Dictionary<object,object> )
				overTyped = ((Dictionary<object,object>)over).Keys;
			else
				throw new ArgumentException( "Value is not enumerable" );

			IScope forScope = new ParameterScope( st.scope, new string[] { this.loopVariable } );
			state.pushActionAndScope( new Step( this, a => {}, "for: scope" ), forScope );
			state.scope.set( this.loopVariable, null );

			oneIteration( st, overTyped, 0 );
		} ) );
		this.loopOver.run( state );
	}

	public override string ToString()
	{
		return "<for {0} in {1}, do {2}>".FormatI( this.loopVariable, this.loopOver, this.block );
	}
}

}