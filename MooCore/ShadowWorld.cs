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

namespace Kayateia.Climoo.MooCore
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Implements the "shadow universe" concept for copy-on-write, aka MOOCOW.
/// </summary>
public class ShadowWorld : IWorld, IDisposable
{
	public ShadowWorld( CanonWorld canon )
	{
		_canon = canon;
	}

	public void Dispose()
	{
		if( _updates.Count > 0 || _deletes.Count > 0 )
			waitForMerge();
	}

	/// <summary>
	/// Checks for a time slot during which we can update to the canon database, and if we
	/// get one, update. If someone else is doing it, we have to wait.
	/// </summary>
	/// <returns>True if we updated</returns>
	public bool tryMerge( ShadowMob mob )
	{
		// If this came in by way of a mob, add it to the update list.
		if( mob != null )
			_updates[mob.id] = mob;

		// Check to see if we have a valid slot on the canon world for updating.
		CanonWorld.MergeToken token = _canon.getMergeToken();
		if( token != null )
		{
			mergeCommon( token );
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// Waits until there's a time slot during which we can update, and does so.
	/// </summary>
	public void waitForMerge()
	{
		mergeCommon( _canon.waitMergeToken() );
	}

	void mergeCommon( CanonWorld.MergeToken token )
	{
		using( CanonWorld.MergeToken t = token )
		{
			// Do any deletes first. We'll skip those in the updates.
			foreach( int id in _deletes )
				_canon.destroyObject( id );

			// Now do object updates. Run each update lambda.
			foreach( var kp in _updates )
			{
				if( _deletes.Contains( kp.Key ) )
					continue;

				CanonMob cm = kp.Value.canon;
				var queue = kp.Value.emptyActionQueue();
				foreach( var action in queue )
					action( cm );
			}
		}
	}

	public long ticks
	{
		get
		{
			return _canon.ticks;
		}
	}

	public World.UrlGenerator attributeUrlGenerator
	{
		get
		{
			return _canon.attributeUrlGenerator;
		}
		set
		{
			_canon.attributeUrlGenerator = value;
		}
	}

	// Not implementing the checkpointers for now.
	public WorldCheckpoint[] checkpoints
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public void checkpoint( string name )
	{
		throw new NotImplementedException();
	}

	public void checkpointRemove( ulong id )
	{
		throw new NotImplementedException();
	}

	public IMob createObject()
	{
		// This is safe to do directly here since it won't affect any other object.
		return pullNewOrCached( _canon.createObject() );
	}

	public IMob findObject( int id )
	{
		if( _objects.ContainsKey( id ) )
			return _objects[id];

		CanonMob cm = _canon.findObject( id );
		if( cm == null )
			return null;
		else
			return pullNewOrCached( cm );
	}

	public IEnumerable<IMob> findObjects( Func<IMob, bool> predicate )
	{
		// This is sort of wasteful, and it's possible that something might slip through
		// the cracks here if someone decides to modify stuff during the traversal. But
		// we'll just specify that it can't be done.
		IEnumerable<CanonMob> cms = _canon.findObjects(
			(cm) => predicate( pullNewOrCached( cm ) )
		);
		return cms.Select( cm => pullNewOrCached( cm ) );
	}

	ShadowMob pullNewOrCached( CanonMob cm )
	{
		if( _objects.ContainsKey( cm.id ) )
			return _objects[cm.id];

		ShadowMob sm = new ShadowMob( this, cm );
		_objects[cm.id] = sm;
		return sm;
	}

	public void destroyObject( int id )
	{
		// We have to queue these still.
		_objects.Remove( id );
		_deletes.Add( id );
		tryMerge( null );
	}

	CanonWorld _canon;
	HashSet<int> _deletes = new HashSet<int>();
	Dictionary<int, ShadowMob> _updates = new Dictionary<int,ShadowMob>();

	// We keep a cached list of shadowmobs to make updates coherent (only one copy
	// of each mob per world, please.)
	Dictionary<int, ShadowMob> _objects = new Dictionary<int,ShadowMob>();
}

}
