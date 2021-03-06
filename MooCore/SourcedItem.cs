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

namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Used to carry "source" information about a particular item.
/// </summary>
/// <remarks>
/// This, being the object's link back to its parent, also provides a way to
/// specify that things have changed.
/// </remarks>
public class SourcedItem<T> where T:class {
	public SourcedItem(Mob source, string name, T item) {
		_src = source;
		_name = name;
		_item = item;
	}

	public Mob source { get { return _src; } }
	public string name { get { return _name; } }
	public T item { get { return _item; } }

	readonly Mob _src;
	readonly string _name;
	readonly T _item;
}

}
