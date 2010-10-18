﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<html>
<head>
	<title>Game Console</title>
	<script type="text/javascript" src="/Scripts/jquery-1.4.1.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.timers-1.0.0.js"></script>
	<script type="text/javascript" src="/Scripts/jquery.hotkeys.js"></script>
	<script type="text/javascript">
		$(document).ready(function () {
			// Set the prompt.
			var prompt = "climoo&gt; ";
			$('#input-prompt').html(prompt);

			// AJAX spinner
			var cmdCount = 0;
			function commandStart() {
				var thisCmd = ++cmdCount;
				var spinnerCode = $('#input-spinner-template').clone();
				spinnerCode
					.attr('id', 'spinner-' + thisCmd)
					.fadeIn(100);
				return { 'id':thisCmd, 'dom':spinnerCode };
			}

			function commandFinish(cmdId) {
				$('#spinner-' + cmdId).fadeOut(100, function() {
					$(this).remove();
				});
			}

			// Cursor flashing
			$('.cursor-flash').everyTime(500, "cursor-flash", function () {
				$(this).toggleClass('on');
			});

			// Scroll handling
			function scroll(pages) {
				var display = $('.display-area');
				display.animate({
					scrollTop: display.scrollTop() + pages * (display.height() * .75)
				}, 100, 'linear');
			}
			function scrollToBottom() {
				var display = $('.display-area');
				display.animate({
					scrollTop: display.attr('scrollHeight')
				}, 100, 'linear');
			}
			$(document).bind('keydown', 'pageup', function(evt) {
				scroll(-1);
				return false;
			});
			$(document).bind('keydown', 'pagedown', function(evt) {
				scroll(1);
				return false;
			});

			// Input handler
			var curLine = "";
			var cursorPos = 0;
			function inputLineUpdate() {
				// Find the left half of the line.
				var left = curLine.substring(0, cursorPos);
				var onCursor = "&nbsp;";
				var right = "";
				if (cursorPos < curLine.length) {
					onCursor = curLine.substring(cursorPos, cursorPos + 1);
					right = curLine.substring(cursorPos + 1, curLine.length);
				}

				$('#input-left').html(left);
				$('#input-cursor').html(onCursor);
				$('#input-right').html(right);
			}
			function inputLineSet(newval) {
				curLine = newval;
				inputLineUpdate();
			}
			function inputLineCheckOvershoot() {
				if (cursorPos > curLine.length)
					cursorPos = curLine.length;
			}
			function inputLineLeft() {
				inputLineCheckOvershoot();
				if (--cursorPos < 0)
					cursorPos = 0;
				inputLineUpdate();
			}
			function inputLineRight() {
				inputLineCheckOvershoot();
				if (++cursorPos > curLine.length)
					cursorPos = curLine.length;
				inputLineUpdate();
			}
			function inputLineInsert(ch) {
				inputLineCheckOvershoot();
				if (cursorPos == curLine.length)
					curLine += ch;
				else if (cursorPos == 0)
					curLine = ch + curLine;
				else
					curLine = curLine.substring(0, cursorPos) + ch + curLine.substring(cursorPos, curLine.length);
				++cursorPos;
				inputLineUpdate();
			}
			function inputLineBackspace() {
				inputLineCheckOvershoot();
				if (cursorPos > 0) {
					curLine = curLine.substring(0, cursorPos - 1) + curLine.substring(cursorPos, curLine.length);
					--cursorPos;
				}
				inputLineUpdate();
			}

			var commandHistory = [];
			var commandHistoryIdx = 0;
			var commandHistorySavedLine = "";
			function historyAdd(line) {
				commandHistory.push(line);
				commandHistoryIdx = commandHistory.length;
			}
			function historyUp() {
				if (commandHistoryIdx == 0)
					return;
				if (commandHistoryIdx == commandHistory.length)
					commandHistorySavedLine = curLine;
				else {
					if (commandHistory[commandHistoryIdx].length == cursorPos)
						cursorPos = commandHistory[commandHistoryIdx - 1].length;
				}

				inputLineSet(commandHistory[--commandHistoryIdx]);
			}
			function historyDown() {
				if (commandHistoryIdx == commandHistory.length)
					return;
				if (++commandHistoryIdx == commandHistory.length) {
					inputLineSet(commandHistorySavedLine);
					commandHistorySavedLine = "";
					return;
				} else {
					if (commandHistory[commandHistoryIdx - 1].length == cursorPos)
						cursorPos = commandHistory[commandHistoryIdx].length;
				}

				inputLineSet(commandHistory[commandHistoryIdx]);
			}

			$(document).bind('keypress', 'return', function(evt) {
				var execLine = curLine;
				inputLineSet("");
				historyAdd(execLine);
				var spinnerId = writeOutput('<span class="old-command"><span class="prompt">' + prompt + '</span>' + execLine, execLine);
				if (execLine) {
					$.getJSON("/Game/ExecCommand?cmd="
						+ escape(execLine)
						+ "&datehack=" + new Date().getTime(),
						function (data) {
							commandFinish(spinnerId);
							if (data.resultText)
								writeOutput(data.resultText);
						}
					);
				}

				return false;
			});
			$(document).bind('keydown', 'left', function(evt) {
				inputLineLeft();
			});
			$(document).bind('keydown', 'right', function(evt) {
				inputLineRight();
			});
			$(document).bind('keydown', 'up', function(evt) {
				historyUp();
			});
			$(document).bind('keydown', 'down', function(evt) {
				historyDown();
			});
			$(document).bind('keypress', 'backspace', function(evt) {
				inputLineBackspace();
				return false;
			});
			$(document).keypress(function (evt) {
				if (evt.which >= 32 && evt.which <= 126) {
					var ch = String.fromCharCode(evt.which);
					if (ch) {
						evt.preventDefault();
						inputLineInsert(String.fromCharCode(evt.which));
					}
				}
			});

			// Output handler
			function writeOutput(text, needSpinner) {
				$('#term-text').append(text);
				var spinnerId;
				if (needSpinner) {
					var spinnerInfo = commandStart();
					spinnerId = spinnerInfo['id'];
					$('#term-text').append(spinnerInfo['dom']);
				}
				$('#term-text').append('<br/>');
				scrollToBottom();

				return spinnerId;
			}

			// Handle unrequested input from server -- this uses a long-poll
			// AJAX request (30 seconds). If something fires, it will return
			// immediately with results, and we will query again immediately;
			// otherwise the timeout will happen and we'll start again.
			function pushBegin() {
				function errorFunction(xhr, status, err) {
					if (status == "timeout") {
						pushBegin();
					} else {
						// Wait a bit on error, in case something is flooded.
						alert("error" + status + " " + err);
						$(document).oneTime(5000, "push-reset", function() {
							pushBegin();
						});
					}
				}
				$.ajax({
					url: "/Game/PushCheck" + "?datehack=" + new Date().getTime(),
					dataType: 'json',
					data: {},
					success:
						function (data) {
							if (data.resultText)
								writeOutput(data.resultText);
							pushBegin();
						},
					error: errorFunction,
					timeout: 30000
				});
			}
			pushBegin();
		});
	</script>
	<style type="text/css">
		body {
			background-color: #004;
			color: #888;
			padding: 0px 0px 0px 0px;
		}
		div {
			margin: 0px 0px 0px 0px;
		}
		.debug {
			border-style: dotted;
			border-width: 1px;
			border-color: #f88;
		}
		.display-area {
			position: relative;
			width: 100%;
			height: 90%;
			background-color: #000;
			color: #aaa;
			font-family: Consolas, Terminal, Fixed;
			font-size: 12pt;
			padding: 2px 2px 2px 2px;
			overflow: hidden;
		}
		.display-area p, .display-area span, .display-area a {
			white-space: pre-wrap;
		}
		
		.size-test {
			font-family: Consolas, Terminal, Fixed;
			font-size: 12pt;
		}
		
		.cursor {
			display: inline-block;
		}
		.cursor.on {
			background-color: #aaa;
			color: #000;
		}
		
		.prompt {
			font-weight: bold;
		}
		.old-command {
			color: #595;
		}
		
		.header-box {
			margin: 0px 0px 0px 0px;
			padding: 0px 0px 0px 0px;
			position: relative;
		}
		h1 {
			font-size: 14pt;
			font-weight: bold;
		}
		/* .spinner {
			display: none;
			position: absolute;
			right: 0px;
			top: 0px;
			background-image: url(/Content/chakram-spinner-000.gif);
			background-position: left center;
			background-repeat: no-repeat;
			width: 16px;
			height: 16px;
		} */
		.input-spinner {
			display: none;
			width: 12px;
			height: 12px;
		}
	</style>
</head>
<body>
	<div class="header-box">
		<h1>Game Terminal</h1>
		<div id="global-spinner" class="spinner"></div>
	</div>
	<div class="display-area">
		<div id="term-text"></div>
		<div id="input">
			<!-- These have to stay on one line not to trigger the 'pre' whitespace -->
			<span id="input-prompt" class="prompt"></span><span id="input-left"></span><span id="input-cursor" class="cursor cursor-size cursor-flash">&nbsp;</span><span id="input-right"></span>
		</div>
	</div>
	<img id="input-spinner-template" class="input-spinner" src="/Content/spiral-spinner-000.gif" alt="[spinner]" />
</body>
</html>
