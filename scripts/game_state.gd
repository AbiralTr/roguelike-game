extends Node

enum State { MAIN_MENU, PLAYING, PAUSED, STAT_MENU, CHOICE_MENU, DEAD }

var current: State = State.MAIN_MENU

var run_currency: int = 0
var run_score: int = 0
var run_elapsed_time: float = 0.0

func _process(delta: float) -> void:
	if current == State.PLAYING:
		run_elapsed_time += delta

func add_currency(amount: int) -> void:
	run_currency += amount

func add_score(amount: int) -> void:
	run_score += amount

func show_main_menu() -> void:
	current = State.MAIN_MENU
	Engine.time_scale = 0.0

func start_game() -> void:
	current = State.PLAYING
	Engine.time_scale = 1.0
	run_currency = 0
	run_score = 0
	run_elapsed_time = 0.0

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
