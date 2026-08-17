extends CanvasLayer
class_name StartMenu

@onready var start_button: Button = $Center/Options/StartButton
@onready var quit_button: Button = $Center/Options/QuitButton

func _ready() -> void:
	GameState.show_main_menu()
	visible = true
	start_button.pressed.connect(_start_game)
	quit_button.pressed.connect(_quit_game)

func _start_game() -> void:
	GameState.start_game()
	visible = false

func _quit_game() -> void:
	get_tree().quit()
