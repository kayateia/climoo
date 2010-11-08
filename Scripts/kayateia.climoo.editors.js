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

	popdown: function() {
		TextEditor._popup.popdown();
		Term.active = true;
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

VerbEditor = {
	ajaxUrlGet: "/Game/GetVerb",
	ajaxUrlSet: "/Game/SetVerb",
	init: function() {
		TermLocal.setHandler("`verb ", true, function(cmd, spn) {
			var rest = cmd.substr(6, cmd.length - 6);
			var objectIdx = rest.indexOf(" ");
			var verbName = rest.substr(0, objectIdx);
			var objName = rest.substr(objectIdx+1, rest.length - (objectIdx+1));
			Term.write("Looking up verb '" + verbName + "' on object '" + objName + "'...");

			$.getJSON(VerbEditor.ajaxUrlGet
				+ "?objectId=" + escape(objName)
				+ "&verb=" + escape(verbName)
				+ "&datehack=" + new Date().getTime(),
				function (data) {
					spn.finish();
					if (data.valid) {
						title = "Editing verb '" + escape(verbName) + "' on object '" + escape(objName) + "'";
						TextEditor.edit(title, data.id + "." + verbName, data.code,
							function(id, text, success) {
								if (success) {
									chunks = id.split(".");
									$.getJSON(VerbEditor.ajaxUrlSet
										+ "?objectId=" + escape(chunks[0])
										+ "&verb=" + escape(chunks[1])
										+ "&code=" + escape(text)
										+ "&datehack=" + new Date().getTime(),
										function(data) {
											if (data.valid) {
												Term.write("Verb was written.");
												TextEditor.popdown();
											} else
												Term.write("Verb was not written: " + data.message);
										}
									);
								}
							}
						);
					} else
						Term.write("Object was not valid: " + data.message);
				}
			);
		});
	}
};

$(document).ready(function() {
	VerbEditor.init();
});

$(document).ready(function() {
	TermLocal.setHandler("local ", false, function(cmd) {
		Term.write("Hey, you typed " + cmd.substr(6, cmd.length));
	});
});
