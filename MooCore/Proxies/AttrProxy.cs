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
using Kayateia.Climoo.Scripting.Coral;

/// <summary>
/// MOO Proxy object for a MOO object attribute. This is available to MOO scripts.
/// </summary>
public class AttrProxy {
	public AttrProxy(SourcedItem<TypedAttribute> attr, Player player) {
		_attr = attr;
		_player = player;
	}

	public AttrProxy(TypedAttribute attr, Player player) {
		_attr = new SourcedItem<TypedAttribute>(null, "<anon>", attr);
		_player = player;
	}

	/// <summary>
	/// The MOO object which owns this attribute.
	/// </summary>
	/// <value>A proxy object for the mob in question.</value>
	[CoralPassthrough]
	public MobProxy source {
		get { return new MobProxy(_attr.source, _player); }
	}

	/// <summary>
	/// The name of this attribute.
	/// </summary>
	[CoralPassthrough]
	public string name {
		get { return _attr.name; }
	}

	/// <summary>
	/// The MIME type of this attribute's value.
	/// </summary>
	[CoralPassthrough]
	public string mime {
		get { return _attr.item.mimetype; }
	}

	/// <summary>
	/// A display string for the attribute's value.
	/// </summary>
	[CoralPassthrough]
	public string display {
		get { return _attr.item.display; }
	}

	/// <summary>
	/// The raw bytes comprising the attribute's value.
	/// </summary>
	[CoralPassthrough]
	public byte[] bytes {
		get
		{
			if( _attr.item.contents is byte[] )
				return (byte[])_attr.item.contents;
			else
				return null;
		}
	}

	readonly SourcedItem<TypedAttribute> _attr;
	readonly Player _player;
}

}
