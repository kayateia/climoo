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

namespace Kayateia.Climoo.Game {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kayateia.Climoo.MooCore;

public class Login {
	static public string LogUserIn(Session.UserContext cxt, string login, string password) {
		int mobId;

		using( World w = Game.WorldData.GetShadow() )
		{
			string passwordHash = password.Sha1Hash();
			Mob playerTemplate = w.findObject( World.WellKnownObjects.Player );
			Mob mob = w.findObject( (m) => m.parentId == playerTemplate.id
				&& m.attrHas( Mob.Attributes.Login ) && m.attrGet( Mob.Attributes.Login ).str == login
				&& m.attrHas( Mob.Attributes.Password ) && m.attrGet( Mob.Attributes.Password ).str == passwordHash );
			if( mob == null )
				return "Invalid user name or password";

			mobId = mob.id;

			cxt.player = new Player( mobId );
			mob.player = cxt.player;
			/*} else {
				// Make a new Mob for the user.
				var mob = w.createObject(new {
						name = u.name
					},
					location: w.findObject("/entry").id,
					parent: w.findObject("/templates/player").id);

				// Save out their new Mob id to their account.
				u.objectid = mob.id;
				cxt.db.update(token, Models.User.Table, u.id, new Dictionary<string, object>() {
					{ "objectid", mob.id }
				});

				mobId = mob.id;

				cxt.player = new Player( mobId );
				mob.player = cxt.player;
			} */
		}

		return null;
	}

	static public void LogUserOut(Session.UserContext cxt) {
		if( cxt.player.id == Mob.Anon.id )
			return;

		using( World w = Game.WorldData.GetShadow() )
		{
			Mob m = w.findObject( cxt.player.id );
			if( m != null )
				m.player = null;
			cxt.player = new Player( Mob.Anon.id );
		}
	}
}

}
