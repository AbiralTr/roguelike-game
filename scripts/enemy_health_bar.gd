extends Node2D

const WIDTH: float = 24.0
const HEIGHT: float = 4.0

@onready var fill: Sprite2D = $Fill
@onready var background: Sprite2D = $Background

var enemy: Node = null

func _ready() -> void:
	enemy = get_parent()
	background.centered = false
	background.texture = Placeholder.make_texture(Color(0.15, 0.15, 0.15), Vector2i(int(WIDTH) + 2, int(HEIGHT) + 2))
	fill.centered = false
	fill.position = Vector2(1, 1)
	fill.texture = Placeholder.make_texture(Color(0.9, 0.2, 0.2), Vector2i(int(WIDTH), int(HEIGHT)))

func _process(_delta: float) -> void:
	if enemy == null or not ("current_health" in enemy) or enemy.enemy_data == null:
		return
	if enemy.enemy_data.max_health <= 0.0:
		return
	var fraction: float = clampf(enemy.current_health / enemy.enemy_data.max_health, 0.0, 1.0)
	fill.scale.x = fraction
