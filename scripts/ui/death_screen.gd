extends CanvasLayer
class_name DeathScreen

@onready var restart_button: Button = $Center/Options/RestartButton

func _ready() -> void:
	add_to_group("death_screen")
	visible = false
	restart_button.pressed.connect(_restart)

func show_screen() -> void:
	visible = true

func _restart() -> void:
	Engine.time_scale = 1.0
	get_tree().reload_current_scene()
