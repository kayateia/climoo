﻿namespace Kayateia.Climoo.MooCore {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class InputParser {
	static public string ProcessInput(string input, Player player) {
		// Split the input.
		string[] pieces = input.Trim().Split(' ', '\t', '\n', '\r');
		if (pieces.Length == 0)
			return "";

		// For now, the verb is always one word.
		string verb = pieces[0];
		if (verb == "\"") verb = "say";
		if (verb == "/me") verb = "emote";

		// Skip forward until we find a preposition.
		var remaining = pieces.Skip(1);
		var start = remaining;
		Verb.PrepMatch p = Verb.PrepMatch.None;
		string dobjName = null;
		for (int skip=0; skip<remaining.Count(); ++skip) {
			var chunk = remaining.Skip(skip);
			p = Verb.MatchPrep(chunk);
			if (p.isReal) {
				// Skip over the preposition.
				remaining = chunk.Skip(p.words.Count());
				dobjName = string.Join(" ", start.Take(skip));
				break;
			}
		}

		if (p.prep == Verb.Prep.None) {
			// No preposition -> the rest of the string is the direct object.
			dobjName = string.Join(" ", remaining);
		}

		// For now, the indirect object is always the rest of the phrase.
		string iobjName = null;
		if (remaining.Count() > 0) {
			iobjName = string.Join(" ", remaining);
		}

		// Look for objects around the player that might match the direct and indirect objects.
		Mob dobj = ObjectMatch(dobjName, player);
		Mob iobj = ObjectMatch(iobjName, player);

		// Look for a matching verb.
		var param = new Verb.VerbParameters() {
			input = input,
			self = null,
			dobj = dobj,
			prep = p.prep,
			iobj = iobj,
			player = player
		};
		var selectedVerb = SearchVerbsFrom(player.avatar, verb, param);
		if (selectedVerb.Count() == 0)
			selectedVerb = SearchVerbsFrom(player.avatar.location, verb, param);
		if (selectedVerb.Count() == 0 && dobj != null)
			selectedVerb = SearchVerbsFrom(dobj, verb, param);
		if (selectedVerb.Count() == 0 && iobj != null)
			selectedVerb = SearchVerbsFrom(iobj, verb, param);

		// Couldn't find one?
		if (selectedVerb.Count() != 1)
			return "Sorry, I don't know what that means.";

		// Execute the verb.
		var v = selectedVerb.First();
		param.self = v.Item1;
		v.Item2.invoke(param);

		// Any output will come from the script.
		return "";
	}

	static Mob ObjectMatch(string objName, Player player) {
		if (string.IsNullOrEmpty(objName))
			return null;

		IEnumerable<Mob> objOptions =
			from m in player.avatar.contained
				.Concat(player.avatar.location.contained)
			where m.name.StartsWith(objName, StringComparison.OrdinalIgnoreCase)
			select m;
		IEnumerable<Mob> exactMatches =
			from m in objOptions
			where m.name == objName
			select m;
		Mob dobj = null;
		if (objOptions.Count() == 0)
			dobj = Mob.None;
		else if (objOptions.Count() > 1) {
			if (exactMatches.Count() == 1)
				dobj = exactMatches.First();
			else
				dobj = Mob.Ambiguous;
		} else
			dobj = objOptions.First();

		return dobj;
	}

	static IEnumerable<Tuple<Mob,Verb>> SearchVerbsFrom(Mob m, string verbName,
		Verb.VerbParameters param)
	{
		param.self = m;
		foreach (var v in m.allVerbs)
			if (v.Value.name == verbName) {
				if (v.Value.match(param).Count() > 0)
					yield return Tuple.Create(m, v.Value);
			}
	}
}

}
