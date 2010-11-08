﻿/*

Editor popups for Climoo
Copyright (C) 2010 Kayateia

Requires:
	jquery-x.x.x.js
	jquery.timers-1.0.0.js
    kayateia.term.js
	kayateia.modalpopup.js
*/

ObjectEditor = {
	ajaxUrlGet: "/Game/GetObject",
	ajaxUrlSet: "/Game/SetObject",
	_popup: null,

	init: function() {
		ObjectEditor._popup = new ModalPopup('#objeditor');
		TermLocal.setHandler("`edit ", true, function(cmd, spn) {
			objname = cmd.substr(6, cmd.length - 6);
			Term.write("Looking up object '" + objname + "'...");

			$.getJSON(ObjectEditor.ajaxUrlGet + "?objectId="
				+ escape(objname)
				+ "&datehack=" + new Date().getTime(),
				function (data) {
					spn.finish();
					if (data.valid) {
						data.title = "Editing '" + escape(objname) + "'";
						ObjectEditor.loadEditor(data);
						ObjectEditor.popEditor(true);
					} else
						Term.write("Object was not valid.");
				}
			);
		});

		TermLocal.setHandler("`create", false, function(cmd) {
			data = {
				title: "Create new object"
			};
			ObjectEditor.loadEditor(data);
			ObjectEditor.popEditor(true);
		});

		$('#objeditor .savebtn').click(function() {
			data = $('#objeditor form').serialize();
			$.ajax({
				type: "POST",
				url: ObjectEditor.ajaxUrlSet,
				dataType: "json",
				data: data,
				success: function(data) {
					if (!data.valid)
						Term.write("Error saving: " + data.message);
					else {
						Term.write("Object was saved.");
						ObjectEditor._popup.popdown();
						Term.active = true;
					}
				},
				error: function(req, status, err) {
					Term.write("Error saving: " + err);
				}
			});
		});

		$('#objeditor .cancelbtn').click(function() {
			ObjectEditor.popEditor(false);
		});
	},

	loadEditor: function(data) {
		$('#objeditor .left').text(data.title);
		$('#objeditor .editid').val(data.id);
		$('#objeditor .editname').val(data.name);
		$('#objeditor .editpath').val(data.pathid);
		$('#objeditor .editparent').val(data.parent);
		$('#objeditor .editdesc').text(data.desc);
	},

	popEditor: function(up) {
		if (up) {
			ObjectEditor._popup.popup();
			Term.active = false;
			$('#objeditor .editdesc').focus();
		} else {
			ObjectEditor._popup.popdown();
			Term.active = true;
		}
	}
};

$(document).ready(function() {
	ObjectEditor.init();
});

TextEditor = {
	_popup: null,
	_callback: null,

	init: function() {
		if (!TextEditor._popup)
			TextEditor._popup = new ModalPopup('#texteditor');

		$('#texteditor .savebtn').click(function() {
			var id = $('#texteditor .editid').val();
			var text = $('#texteditor .edittext').val();
			if (TextEditor._callback(id, text, true)) {
				TextEditor._popup.popdown();
				Term.active = true;
			}
		});

		$('#texteditor .cancelbtn').click(function() {
			var id = $('#texteditor .editid').val();
			var text = $('#texteditor .edittext').val();
			if (TextEditor._callback(id, text, false)) {
				TextEditor._popup.popdown();
				Term.active = true;
			}
		});
	},

	// Callback should be in this form:
	// function[bool] callback(id, text, success[bool]);
	// If it returns true, the editor will pop down.
	edit: function(title, id, text, callback) {
		$('#texteditor .left').text(title);
		$('#texteditor .editid').val(id);
		$('#texteditor .edittext').val(text);
		TextEditor._callback = callback;
		TextEditor._popup.popup();
		Term.active = false;
		$('#texteditor .edittext').focus();
	}
};

$(document).ready(function() {
	TextEditor.init();
});

$(document).ready(function() {
	TermLocal.setHandler("edit ", false, function(cmd) {
		TextEditor.edit(cmd, 0, cmd, function() {
			return true;
		});
	});
});

$(document).ready(function() {
	TermLocal.setHandler("local ", false, function(cmd) {
		Term.write("Hey, you typed " + cmd.substr(6, cmd.length));
	});
});