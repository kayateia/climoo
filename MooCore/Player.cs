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

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Coral = Kayateia.Climoo.Scripting.Coral;

/// <summary>
/// Represents a user interacting with the world.
/// </summary>
public class Player {
	public delegate void OutputNotification(string text);
	public OutputNotification NewOutput;
	public OutputNotification NewErrorOutput;
	public OutputNotification NewSound;

	public Player( int id ) {
		_id = id;

		// Default to acting as the player.
		_coralState = new Coral.State();
		Coral.Runner r = new Coral.Runner( _coralState );
		r.pushSecurityContext( new SecurityContext( "base", _id ) );

		// If we're anon, make a mob for it.
		if( id == Mob.Anon.id )
		{
			_anonWorld = new AnonWorld();
			_anonMob = new AnonMob( _anonWorld, this );
			_anonWorld.anonMob = _anonMob;
		}
	}

	/// <summary>
	/// This should be set whenever a context change happens (move to a new ShadowWorld).
	/// </summary>
	public World world {
		get
		{
			if( _anonWorld != null )
				return World.Wrap( _anonWorld );
			else
				return _world;
		}
		set
		{
			_world = value;
			if( _anonWorld != null )
			{
				if( _world == null )
					_anonWorld.real = null;
				else
					_anonWorld.real = _world.get;
			}
		}
	}

	public int id
	{
		get { return _id; }
	}

	/// <summary>
	/// True if the user is actually logged in and active.
	/// </summary>
	public bool isActive {
		get { return this.NewOutput != null; }
	}

	/// <summary>
	/// Write the specified text to the player's console.
	/// </summary>
	public void write(string text) {
		if (this.NewOutput != null) {
			string moocoded = MooCode.PrepareForClient(text);
			this.NewOutput(moocoded);
		}
	}

	public void writeError(string text) {
		if( this.NewErrorOutput != null )
		{
			string moocoded = MooCode.PrepareForClient(text);
			this.NewErrorOutput(moocoded);
		}
	}

	/// <summary>
	/// Plays a sound effect on the player's console.
	/// </summary>
	/// <param name="source">Mob the sound effect is located on.</param>
	/// <param name="attrName">Attribute name of the sound.</param>
	/// <param name="w">The world to use for its attribute generator.</param>
	/// <remarks>
	/// It's necessary to pass in a world here because, when this is called as part of a pulse
	/// notification, the player won't necessarily have a world.
	/// </remarks>
	public void playSound( Mob source, string attrName, World w )
	{
		if( this.NewSound != null )
		{
			string url = w.attributeUrlGenerator( source, attrName );
			this.NewSound( url );
		}
	}

	/// <summary>
	/// Detach this player from its in-game instance.
	/// </summary>
	public void detach() {
		this.NewOutput = null;
		this.NewErrorOutput = null;
		this.NewSound = null;
	}

	/// <summary>
	/// Pushes an actor context onto the Coral state. Do this before running code.
	/// </summary>
	public void actorContextPush( string name, int id )
	{
		Coral.Runner r = new Coral.Runner( _coralState );
		r.pushSecurityContext( new SecurityContext( name, id ) );
	}

	/// <summary>
	/// Returns the current actor context. This will be what code should be executing
	/// under at any given moment.
	/// </summary>
	public int actorContext
	{
		get
		{
			SecurityContext cxt = (SecurityContext)_coralState.securityContext;
			return cxt.actorId;
		}
	}

	/// <summary>
	/// Returns the current actor context stack. The entry nearest the top is the current one.
	/// This is just a debug tool.
	/// </summary>
	public int[] actorContextStack
	{
		get
		{
			return _coralState.getSecurityContextStack()
				.Select( cxt => ((SecurityContext)cxt).actorId )
				.ToArray();
		}
	}

	/// <summary>
	/// The Coral state associated with the player.
	/// </summary>
	public Coral.State coralState
	{
		get
		{
			return _coralState;
		}
	}

	/// <summary>
	/// Our anonymous mob, if we have such a thing.
	/// </summary>
	public Mob anonMob
	{
		get
		{
			if( _anonMob == null )
				return null;
			else
				return Mob.Wrap( _anonMob );
		}
	}


	World _world;
	int _id;
	Coral.State _coralState;

	// These are only used if this represents a player who isn't logged in yet.
	AnonMob _anonMob;
	AnonWorld _anonWorld;
}

}
