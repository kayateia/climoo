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
namespace Kayateia.Climoo.Models {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// A single permission.
/// </summary>
public class Permission
{
	/// <summary>
	/// The object ID of the actor.
	/// </summary>
	public int actorId = 0;

	/// <summary>
	/// "Allow" or "Deny".
	/// </summary>
	public string type = "Allow";

	/// <summary>
	/// Permission bits.
	/// </summary>
	public int permBits = 0;

	/// <summary>
	/// Specific string (verb/attribute name) if needed.
	/// </summary>
	public string specificString = null;
}

/// <summary>
/// A set of permissions.
/// </summary>
public class Permissions {
	/// <summary>
	/// The ID of the object in question.
	/// </summary>
	public int objectId = 0;

	/// <summary>
	/// True if we were able to retrieve the permissions info. If false, check 'message'.
	/// </summary>
	public bool success = false;

	/// <summary>
	/// If success is false, an error message will be here.
	/// </summary>
	public string message = null;

	/// <summary>
	/// The permissions.
	/// </summary>
	public Permission[] perms;
}

}
