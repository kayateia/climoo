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

namespace Kayateia.Climoo.Tests.Console
{
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kayateia.Climoo.Database;
using Kayateia.Climoo.Database.Xml;
using Kayateia.Climoo.MooCore;

class Program
{
	static void Main( string[] args )
	{
		if( args.Length != 2 )
		{
			Console.WriteLine( "Please specify the filename of an ImpExporter database config file and a player login name." );
			return;
		}

		// Load up the config.
		ImpExporterConfig cfg = XmlPersistence.Load<ImpExporterConfig>( args[0] );

		// Create a world, sans realtime save and pulse.
		Assembly asm = Assembly.Load( cfg.DatabaseAssembly );
		Type dbType = asm.GetType( cfg.DatabaseClass );
		IDatabase db = (IDatabase)Activator.CreateInstance( dbType );
		db.setup( cfg.ConnectionString, new TableInfo() );
		var coredb = new CoreDatabase( db );
		var worlddb = new WorldDatabase( coredb );
		var world = CanonWorld.FromWorldDatabase( worlddb, true, true );
		world.attributeUrlGenerator = ( obj, name ) => CultureFree.Format( "<attr:{0}.{1}>", obj.name, name );

		// Look up the player. If they passed "anon", then we use an anonymous, pre-login player.
		int mobid;
		if( args[1] == "anon" )
		{
			mobid = Mob.Anon.id;
		}
		else
		{
			using( var token = coredb.token() )
			{
				var users = coredb.select( token, new DBUser() { login = args[1] }, new string[] { "login" } );
				if( users.Count() != 1 )
				{
					Console.WriteLine( "Couldn't match exactly one user with the login '{0}'", args[1] );
					return;
				}

				mobid = users.First().@object;
			}
		}

		// Make a shadow world for us to use.
		var shadowWorld = new ShadowWorld( world );
		Mob playerMob = mobid > 0 ? Mob.Wrap( shadowWorld.findObject( mobid ) ) : null;

		// Make a player object.
		Player player = new Player( mobid );
		player.NewOutput = (o) =>
		{
			Console.WriteLine( "{0}", o );
		};
		player.NewSound = (o) =>
		{
			Console.WriteLine( "Would play sound: {0}", o );
		};
		player.NewErrorOutput = (o) =>
		{
			Console.WriteLine( "[error] {0}", o );
		};
		player.world = World.Wrap( shadowWorld );
		if( playerMob != null )
			playerMob.player = player;

		while( true )
		{
			Console.Write( "climoo> " );

			string command = Console.ReadLine();
			if( command == "exit" )
				break;

			string result = MooCore.InputParser.ProcessInput( command, player );
			if( !result.IsNullOrEmpty() )
				Console.WriteLine( result );

			shadowWorld.waitForMerge();
		}

		// Clear out any changes and timers.
		shadowWorld.Dispose();
		world.Dispose();
	}
}

}
