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
using Kayateia.Climoo.Scripting.Coral;

/// <summary>
/// MOO Proxy object for a player object, providing all the functionality of a
/// typical mob proxy, plus some player specific functions. This is available to MOO scripts.
/// </summary>
/// <remarks>
/// We pass a world in here because realistically, we need this to execute in the context
/// of whoever is calling it, not any original user world. Also the player may not *have* a
/// world at this point, if it's a pulse call.
/// </remarks>
public class PlayerProxy : MobProxy {
	public PlayerProxy( Player player, World w )
		: base( w.findObject( player.id ), player )
	{
		_player = player;
	}
	Player _player;

	/// <summary>
	/// Write the specified text to the user's terminal.
	/// </summary>
	[Passthrough]
	[CoralPassthrough]
	public void write(string text) {
		_player.write(text);
	}

	/// <summary>
	/// True if the player is actively logged in, or false if they are logged out.
	/// </summary>
	[Passthrough]
	[CoralPassthrough]
	public bool active {
		get {
			return _player.isActive;
		}
	}
}

}
