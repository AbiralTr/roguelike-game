extends CanvasLayer
class_name PauseMenu

var is_paused: bool = false

@onready var resume_button: Button = $Panel/ButtonArea/Options/ResumeButton
@onready var quit_button: Button = $Panel/ButtonArea/Options/QuitButton

func _ready() -> void:
	visible = false
	resume_button.pressed.connect(resume)
	quit_button.pressed.connect(_quit_game)

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("pause"):
		toggle()

func toggle() -> void:
	if is_paused:
		resume()
	else:
		_pause()

func _pause() -> void:
	if not GameState.request_pause():
		return
	is_paused = true
	visible = true

func resume() -> void:
	if not GameState.request_resume():
		return
	is_paused = false
	visible = false

func _quit_game() -> void:
	get_tree().quit()
