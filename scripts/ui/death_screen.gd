extends CanvasLayer
class_name DeathScreen

@onready var restart_button: Button = $Center/Options/RestartButton
@onready var stats_label: Label = $Center/Options/StatsLabel

func _ready() -> void:
	add_to_group("death_screen")
	visible = false
	restart_button.pressed.connect(_restart)

func show_screen() -> void:
	var total_seconds: int = int(GameState.run_elapsed_time)
	stats_label.text = "Score: %d   Nanites: %d   Time: %02d:%02d" % [
		GameState.run_score, GameState.run_currency, total_seconds / 60, total_seconds % 60
	]
	visible = true

func _restart() -> void:
	Engine.time_scale = 1.0
	get_tree().reload_current_scene()
