extends Area2D
class_name WeaponPickup

@export var weapon_data: WeaponData

var player_in_range: Player = null

@onready var sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	body_exited.connect(_on_body_exited)
	_apply_visual()

func init(data: WeaponData) -> void:
	weapon_data = data
	_apply_visual()

func _apply_visual() -> void:
	if sprite == null:
		return
	if weapon_data != null and weapon_data.icon != null:
		sprite.texture = weapon_data.icon
	else:
		sprite.texture = Placeholder.make_texture(Color(0.7, 0.5, 0.9), Vector2i(14, 14))

func _process(_delta: float) -> void:
	if player_in_range == null:
		return
	if Input.is_action_just_pressed("interact"):
		player_in_range.equip(weapon_data, global_position)
		queue_free()

func _on_body_entered(body: Node2D) -> void:
	if body is Player:
		player_in_range = body

func _on_body_exited(body: Node2D) -> void:
	if body == player_in_range:
		player_in_range = null
