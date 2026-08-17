extends Node

enum State { MAIN_MENU, PLAYING, PAUSED, STAT_MENU, CHOICE_MENU, DEAD }

var current: State = State.MAIN_MENU

func show_main_menu() -> void:
	current = State.MAIN_MENU
	Engine.time_scale = 0.0

func start_game() -> void:
	current = State.PLAYING
	Engine.time_scale = 1.0

func request_pause() -> bool:
	if current != State.PLAYING:
		return false
	current = State.PAUSED
	Engine.time_scale = 0.0
	return true

func request_resume() -> bool:
	if current != State.PAUSED:
		return false
	current = State.PLAYING
	Engine.time_scale = 1.0
	return true

func request_open_stat_menu() -> bool:
	if current != State.PLAYING:
		return false
	current = State.STAT_MENU
	Engine.time_scale = 0.0
	return true

func request_close_stat_menu() -> bool:
	if current != State.STAT_MENU:
		return false
	current = State.PLAYING
	Engine.time_scale = 1.0
	return true

func request_open_choice_menu() -> bool:
	if current != State.PLAYING:
		return false
	current = State.CHOICE_MENU
	Engine.time_scale = 0.0
	return true

func request_close_choice_menu() -> bool:
	if current != State.CHOICE_MENU:
		return false
	current = State.PLAYING
	Engine.time_scale = 1.0
	return true

func die() -> void:
	current = State.DEAD
	Engine.time_scale = 0.0
