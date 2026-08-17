extends SceneTree

func _init() -> void:
	_add_action("move_left", [_key_event(KEY_A)])
	_add_action("move_right", [_key_event(KEY_D)])
	_add_action("jump", [_key_event(KEY_W)])
	_add_action("melee_attack", [_mouse_event(MOUSE_BUTTON_LEFT)])
	_add_action("ranged_attack", [_mouse_event(MOUSE_BUTTON_RIGHT)])
	_add_action("dash", [_key_event(KEY_SPACE)])
	_add_action("interact", [_key_event(KEY_E)])
	_add_action("pause", [_key_event(KEY_ESCAPE)])
	_add_action("toggle_stat_menu", [_key_event(KEY_TAB)])

	var err := ProjectSettings.save()
	print("setup_input: ProjectSettings.save() -> ", err)
	quit()

func _key_event(keycode: Key) -> InputEventKey:
	var event := InputEventKey.new()
	event.physical_keycode = keycode
	return event

func _mouse_event(button: MouseButton) -> InputEventMouseButton:
	var event := InputEventMouseButton.new()
	event.button_index = button
	return event

func _add_action(action_name: String, events: Array) -> void:
	var setting_name := "input/%s" % action_name
	var action := {
		"deadzone": 0.5,
		"events": events,
	}
	ProjectSettings.set_setting(setting_name, action)
