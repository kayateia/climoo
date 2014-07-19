﻿/*
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

namespace Kayateia.Climoo.MooCore.Proxies {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayateia.Climoo.Scripting.SSharp;

/// <summary>
/// Provides really simple constants for use in scripts.
/// It's fine to just pull the static instance of this
/// and put that into every script; nothing is writable.
/// </summary>
public class PermBitsProxy : DynamicObjectBase {
	private PermBitsProxy() { }
	static public PermBitsProxy Static {
		get {
			return s_perm;
		}
	}
	static PermBitsProxy s_perm = new PermBitsProxy();

	[Passthrough]
	public ulong ar { get { return PermBits.AR; } }

	[Passthrough]
	public ulong aw { get { return PermBits.AW; } }

	[Passthrough]
	public ulong ao { get { return PermBits.AO; } }

	[Passthrough]
	public ulong vr { get { return PermBits.VR; } }

	[Passthrough]
	public ulong vw { get { return PermBits.VW; } }

	[Passthrough]
	public ulong or { get { return PermBits.OR; } }

	[Passthrough]
	public ulong ow { get { return PermBits.OW; } }

	[Passthrough]
	public ulong om { get { return PermBits.OM; } }

	[Passthrough]
	public ulong of { get { return PermBits.OF; } }

	[Passthrough]
	public ulong attr { get { return PermBits.Attr; } }

	[Passthrough]
	public ulong verb { get { return PermBits.Verb; } }

	[Passthrough]
	public ulong obj { get { return PermBits.Obj; } }
}

/// <summary>
/// Proxy for the Perm class, which represents one ACE from an ACL.
/// </summary>
public class PermProxy : DynamicObjectBase {
	public PermProxy( World w, Player p )
	{
		_world = w;
		_player = p;
	}

	public PermProxy( World w, Player player, Perm perm )
	{
		_world = w;
		_player = player;
		_perm = perm;
	}

	[Passthrough]
	public MobProxy actor
	{
		get
		{
			Mob m = _world.findObject( _perm.actorId );
			if( m == null )
				return null;
			else
				return new MobProxy( m, _player );
		}

		set
		{
			_perm.actorId = value.id;
		}
	}

	[Passthrough]
	public int actorId
	{
		get
		{
			return _perm.actorId;
		}

		set
		{
			_perm.actorId = value;
		}
	}

	[Passthrough]
	public StringI type
	{
		get
		{
			if( _perm.type == Perm.Type.Allow )
				return "allow";
			else
				return "deny";
		}

		set
		{
			if( value == "allow" )
				_perm.type = Perm.Type.Allow;
			else if( value == "deny" )
				_perm.type = Perm.Type.Deny;
			else
				throw new ArgumentException( "Must be 'allow' or 'deny'" );
		}
	}

	[Passthrough]
	public ulong permbits
	{
		get
		{
			return _perm.perms;
		}

		set
		{
			_perm.perms = value;
		}
	}

	[Passthrough]
	public StringI specific
	{
		get
		{
			return _perm.specific;
		}

		set
		{
			_perm.specific = value;
		}
	}

	public Perm get
	{
		get
		{
			return _perm;
		}
	}

	[Passthrough]
	public override string ToString()
	{
		var names = new List<string>();
		for( int i=0; i<64; ++i )
		{
			ulong mask = 1UL << i;
			if( _perm.perms & mask )
			{
				string name = PermBits.PermNames[mask];
				names.Add( name );
			}
		}
		string bitString = String.Join( "/", names );

		return CultureFree.Format( "<Perm: #{0} {1} {2}{3}>",
			this.actorId, this.type, bitString, this.specific != null ? " "+this.specific : new StringI( "" ) );
	}

	Perm _perm = new Perm()
	{
		perms = new PermBits()
	};
	World _world;
	Player _player;
}

}
