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

namespace Kayateia.Climoo.Controllers {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using Trace = System.Diagnostics.Trace;
using Kayateia.Climoo.Models;

/// <summary>
/// Controls the main game screen / loop.
/// </summary>
public class GameController : Session.SessionFreeController {
	// The actual main page view.
	public ActionResult Index() {
		/* if (_user.player == null) {
			string result = Game.Login.LogUserIn(_user, "kayateia", "");
			if (result != null) {
				_user.outputPush("Could not log you in: " + result);
				return View("Console");
			}
		}

		MooCore.InputParser.ProcessInput("look", _user.player); */

		return View("Console");
	}

	// Called periodically from the page for long-poll "push" notifications
	// of console output.
	[OutputCache(NoStore=true, Duration=0, VaryByParam="")]
	public JsonResult PushCheck()
	{
		// Wait for new output, and fail if we don't get any by 25 seconds.
		ConsoleCommand command;
		try
		{
			if( !_user.outputWait( 25000 ) )
				command = new ConsoleCommand();
			else
			{
				// Get what's there, if anything is left.
				command = _user.outputPop();
			}
		}
		catch( Exception e )
		{
			command = new ConsoleCommand() { text = e.ToString() };
			// newText = e.Message;
		}

		return Json( command, JsonRequestBehavior.AllowGet );
	}

	// Called by the page when the user types a command. This may return
	// data immediately rather than waiting for the push.
	[OutputCache(NoStore=true, Duration=0, VaryByParam="")]
	public JsonResult ExecCommand(string cmd) {
		string output;
		try {
			using( var world = Game.WorldData.GetShadow() )
			{
				output = _user.inputPush( cmd, world );
			}
		} catch (System.Exception ex) {
			output = "<span class=\"error\">Exception: {0}</span>".FormatI(ex.ToString());
		}
		var result = new Models.ConsoleCommand() {
			text = output,
			sidebar = "/Game/Sidebar"
		};

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	[OutputCache(NoStore=true, Duration=0, VaryByParam="")]
	public ActionResult Sidebar()
	{
		if( !_user.inGame )
			return null;

		SidebarInfo model = new SidebarInfo();

		// This world is read-only, so it's okay to just let it go for the GC.
		var world = Game.WorldData.GetShadow();
		if( _user != null && _user.player != null )
		{
			model.player = _user.player;
			model.playerMob = world.findObject( _user.player.id );
			model.location = model.playerMob.location;
		}
		model.world = world;

		return View( "SidebarInfo", model );
	}

	// Implements the retrieval of the "get a URL for an attribute" functionality.
	[OutputCache(NoStore=true, Duration=0, VaryByParam="")]
	public ActionResult ServeAttribute(int objectId, string attributeName) {
		if (!_user.inGame)
			return null;

		using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob mob = world.findObject( objectId );
			if (mob == null)
				return null;

			var attr = mob.findAttribute(attributeName);
			return this.File(attr.item.getContents<byte[]>(), attr.item.mimetype);
		}
	}

	// Returns information about an MOO object to the client-side scripting. This is used for editing.
	public JsonResult GetObject(string objectId) {
		if (!_user.inGame)
			return null;

		using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob obj = MooCore.InputParser.MatchName( objectId, world.findObject( _user.player.id ) );
			MooCore.Mob parent = obj.parent;
			string parentId = "";
			if (parent != null && parent.id > 0) {
				string fqpn = parent.fqpn;
				if (!string.IsNullOrEmpty(fqpn))
					parentId = fqpn;
				else
					parentId = "#{0}".FormatI(obj.parentId);
			}
			var result = new {
				valid = obj != MooCore.Mob.None && obj != MooCore.Mob.Ambiguous,
				id = obj.id,
				name = obj.name,
				parent = parentId,
				pathid = obj.pathId,
				desc = obj.desc
			};
			return Json(result, JsonRequestBehavior.AllowGet);
		}
	}

	// Sets information on a MOO object from the client-side scripting. This is used for editing.
	[HttpPost]
	public JsonResult SetObject(int? id, string name, string parent, string pathid, string desc) {
		if (!_user.inGame)
			return null;

		object result;

		using( var world = Game.WorldData.GetShadow() )
		{
			try {
				// If it doesn't exist yet, make it. Otherwise we're setting on the existing one.
				MooCore.Mob obj;
				if (!id.HasValue)
				{
					obj = world.createObject( new {}, location: world.findObject( _user.player.id ).locationId );
					obj.ownerId = _user.player.id;
				}
				else
					obj = world.findObject( id.Value );
				if (obj == null || obj == MooCore.Mob.None)
					result = new { valid = false, message = "Invalid object ID" };
				else {
					int? parentId = null;
					if (parent.StartsWithI("#"))
						parentId = CultureFree.ParseInt(parent.Substring(1));
					else if (parent.StartsWithI(MooCore.Mob.PathSep+""))
						parentId = obj.world.findObject(parent).id;

					obj.name = name;
					if (parentId.HasValue)
						obj.parentId = parentId.Value;
					obj.pathId = pathid;
					obj.desc = desc;

					result = new { valid = true, message = "", id = obj.id };
				}
			} catch (Exception ex) {
				result = new { valid = false, message = ex.Message };
			}
		}

		return Json(result, JsonRequestBehavior.DenyGet);
	}

	// Gets information a verb from a MOO object for client-side scripting. This is used for editing.
	public JsonResult GetVerb(string objectId, string verb) {
		if (!_user.inGame)
			return null;

		object result;

		using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob obj = MooCore.InputParser.MatchName(objectId, world.findObject( _user.player.id ) );
			if (obj == MooCore.Mob.None) {
				result = new { valid = false, message = "Unknown object" };
			} else if (obj == MooCore.Mob.Ambiguous) {
				result = new { valid = false, message = "Ambiguous object" };
			} else {
				MooCore.Verb v = obj.verbGet(verb);
				if (v == null)
					result = new { valid = true, message = "Unknown verb", id = obj.id, code = "" };
				else {
					result = new { valid = true, message = "",
						id = obj.id,
						code = v.code
					};
				}
			}
		}

		return Json(result, JsonRequestBehavior.AllowGet);
	}

	// Sets information on a verb on a MOO object from client-side scripting. This is used for editing.
	[HttpPost]
	public JsonResult SetVerb(int objectId, string verb, string code) {
		if (!_user.inGame)
			return null;

		object result;

		using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob obj = world.findObject( objectId );
			if (obj == null) {
				result = new { valid = false, message = "Unknown object" };
			} else {
				string message = "";
				bool valid = true;
				try {
					MooCore.Verb v = new MooCore.Verb() {
						name = verb,
						code = code
					};
					obj.verbSet(verb, v);
				} catch (System.Exception ex) {
					message = "<span class=\"error\">Exception: {0}</span>".FormatI(ex.Message);
					valid = false;
				}
				result = new { valid = valid, message = message };
			}
		}

		return Json(result, JsonRequestBehavior.DenyGet);
	}

	// Gets permissions from a MOO object for client-side scripting. This is used for editing.
	public JsonResult GetPerms( string objectId )
	{
		if( !_user.inGame )
			return null;

		Permissions result;

		using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob obj = MooCore.InputParser.MatchName(objectId, world.findObject( _user.player.id ) );
			if (obj == MooCore.Mob.None) {
				result = new Permissions()
				{
					success = false,
					message = "Unknown object"
				};
			} else if (obj == MooCore.Mob.Ambiguous) {
				result = new Permissions()
				{
					success = false,
					message = "Ambiguous object"
				};
			} else {
				MooCore.Perm[] perms = obj.permissions;
				if (perms == null)
					result = new Permissions()
					{
						success = true,
						message = "No permissions",
						objectId = obj.id,
						perms = new Permission[0]
					};
				else {
					result = new Permissions()
					{
						success = true,
						message = "",
						objectId = obj.id,
						perms = perms
							.Select( x => new Permission()
							{
								actorId = x.actorId,
								permBits = x.permBits,
								specificString = x.specificString,
								type = x.typeString
							} )
							.ToArray()
					};
				}
			}
		}

		return Json( result, JsonRequestBehavior.AllowGet );
	}

	// Sets permissions on a MOO object from client-side scripting. This is used for editing.
	[HttpPost]
	public JsonResult SetPerms( int objectId, Permissions perms )
	{
		if( !_user.inGame )
			return null;

		if( !ModelState.IsValid )
		{
			return Json( new Permissions()
			{
				success = false,
				message = "Inputs were not valid"
			} );
		}

		object result = new {};

		/*using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob obj = world.findObject( objectId );
			if (obj == null) {
				result = new { valid = false, message = "Unknown object" };
			} else {
				string message = "";
				bool valid = true;
				try {
					MooCore.Verb v = new MooCore.Verb() {
						name = verb,
						code = code
					};
					obj.verbSet(verb, v);
				} catch (System.Exception ex) {
					message = "<span class=\"error\">Exception: {0}</span>".FormatI(ex.Message);
					valid = false;
				}
				result = new { valid = valid, message = message };
			}
		} */

		return Json(result, JsonRequestBehavior.DenyGet);
	}

	// Shows the form for uploading binary data such as images.
	public ActionResult UploadFrame() {
		if (!_user.inGame)
			return null;

		dynamic result = new System.Dynamic.ExpandoObject();
		result.initial = true;
		return View("UploadBinaryFrame", result);
	}

	// Sets a binary attribute on a MOO object, from client-side scripting. This is used for editing.
	[HttpPost]
	public ActionResult SetBinaryAttribute(string objectId, string name, string mimetype, HttpPostedFileBase fileData) {
		if (!_user.inGame)
			return null;

		dynamic result = new System.Dynamic.ExpandoObject();
		result.initial = false;

		using( var world = Game.WorldData.GetShadow() )
		{
			MooCore.Mob obj = MooCore.InputParser.MatchName(objectId, world.findObject( _user.player.id ) );
			if (obj == MooCore.Mob.None) {
				result.message = "Unknown object";
			} else if (obj == MooCore.Mob.Ambiguous) {
				result.message = "Ambiguous object";
			} else {
				if (fileData.ContentLength > 500*1024)
					result = new { valid = false, message = "File too large" };
				byte[] bytes = new byte[fileData.ContentLength];
				fileData.InputStream.Read(bytes, 0, fileData.ContentLength);

				if (string.IsNullOrEmpty(mimetype))
					mimetype = fileData.ContentType;

				var ta = new MooCore.TypedAttribute() {
					contents = bytes,
					mimetype = mimetype
				};
				obj.attrSet(name, ta);

				result.message = "Save was successful!";
			}
		}

		return View("UploadBinaryFrame", result);
	}
}

}
