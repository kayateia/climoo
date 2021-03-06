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
using System.Text.RegularExpressions;

/// <summary>
/// Handles all aspects of dealing with MooCode, our faux HTML based
/// on the likes of BBcode/phpBBCode/etc.
/// </summary>
public class MooCode {
	/* class MooTag {
		public string tag;
		public string parameter;
	} */

	static public string PrepareForClient(string input) {
		// First thing we want to do is escape all HTML in the input. This will
		// ensure that nothing unwanted reaches the browser, and things that are
		// wanted (like lt/gt) DO reach the browser.
		// input = input.Replace(
		StringBuilder sb = new StringBuilder(input);
		sb.Replace("&", "&amp;");
		sb.Replace("<", "&lt;");
		sb.Replace(">", "&gt;");

		// Newlines become breaks.
		sb.Replace( "\r", "" );
		sb.Replace("\n", "<br/>");

		// The rest we'll parse through a simple BBCode parser.
		// FIXME: This should be done later, with a Stack<> and all. Otherwise we are
		// opening ourselves to XSS through unmatched tag games.
		/* string regexInput = sb.ToString();
		sb.Clear();

		var matches = Regex.Matches(regexInput, @"\[/?(<?tag>[^]]+\]", RegexOptions.CultureInvariant);
		int lastIdx = 0;
		foreach (Match match in matches) {
			if (match.Index > lastIdx) {
				// The match is 
			}
		} */

		// Look for a couple of common MooCode marks and replace them.
		foreach (var item in TagMapping) {
			sb.Replace(string.Format("[{0}]", item.Key), string.Format("<{0}>", item.Value));
			sb.Replace(string.Format("[/{0}]", item.Key), string.Format("</{0}>", item.Value));
		}

		// The rest are regex that require a string.
		string rv = sb.ToString();

		// Color. [color=#515]foo[/color]
		rv = Regex.Replace(rv, @"\[color=(?<color>\#[0-9a-f]{3})\]", @"<span style=""color:${color}"">");
		rv = Regex.Replace(rv, @"\[/color\]", "</span>");

		// Links. [url]http://foo[/url] or [url=http://foo]bar[/url]
		// We can make the first form in one go because of its nature, but not the second one.
		rv = Regex.Replace(rv, @"\[url\](?<url>[^\[]*)\[/url\]", @"<a href=""${url}"">${url}</a>");
		rv = Regex.Replace(rv, @"\[url=(?<url>[^\]]*)\]", @"<a href=""${url}"">");
		rv = Regex.Replace(rv, @"\[/url\]", "</a>");

		// Images. [img]http://foo[/img]
		// This one can also be in one go.
		rv = Regex.Replace(rv, @"\[img\](?<url>[^\[]*)\[/img\]", @"<image src=""${url}"" />");

		// Float blocks. [float=left]foo[/float]
		rv = Regex.Replace(rv, @"\[float=(?<which>left|right)\]",
			@"<span style=""float:${which}"">");
		rv = Regex.Replace(rv, @"\[/float\]", "</span>");

		// Table tags.
		rv = Regex.Replace(rv, @"\[table\]", @"<table border=""0"">");
		rv = Regex.Replace(rv, @"\[/table\]", @"</table>");
		rv = Regex.Replace(rv, @"\[tr\]", @"<tr>");
		rv = Regex.Replace(rv, @"\[/tr\]", @"</tr>");
		rv = Regex.Replace(rv, @"\[td\]", @"<td>");
		rv = Regex.Replace(rv, @"\[/td\]", @"</td>");

		return rv;
	}

	static Dictionary<string, string> TagMapping = new Dictionary<string,string> {
		{ "b", "b" },
		{ "i", "i" },
		{ "u", "u" }
	};
}

}
