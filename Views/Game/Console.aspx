﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head>
<!--
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
    along with this program.  If not, see http://www.gnu.org/licenses/.
-->

	<title>CliMOO -- FlowerBox</title>
	<script type="text/javascript" src="/Scripts/jquery-1.4.1.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.timers-1.0.0.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.hotkeys.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.evenifhidden.js"></script>
	<script type="text/javascript" src="/Scripts/kayateia.term.js"></script>
	<script type="text/javascript" src="/Scripts/kayateia.modalpopup.js"></script>
	<script type="text/javascript" src="/Scripts/kayateia.climoo.editors.js"></script>
	<link rel="Stylesheet" href="/Content/term.css" />
	<link rel="Stylesheet" href="/Content/game.css" />
	<link rel="Stylesheet" href="/Content/modalpopup.css" />
</head>
<body>
	<div class="header-box">
		<h1>CliMOO -- FlowerBox</h1>
	</div>
	<div class="terminal themed">
		<div id="term-text"></div>
		<div id="input">
			<!-- These have to stay on one line not to trigger the 'pre' whitespace -->
			<span id="input-prompt" class="prompt"></span><span id="input-left"></span><span id="input-cursor" class="cursor cursor-size cursor-flash">&nbsp;</span><span id="input-right"></span>
		</div>
	</div>
	<img id="input-spinner-template" class="input-spinner" src="/Content/spiral-spinner-000.gif" alt="[spinner]" />

	<% Html.RenderPartial("ObjectEditor"); %>
	<% Html.RenderPartial("TextEditor"); %>
	<% Html.RenderPartial("UploadBinary"); %>
	<% Html.RenderPartial("LoginBox"); %>
</body>
</html>
