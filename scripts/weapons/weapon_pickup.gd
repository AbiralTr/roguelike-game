extends InteractableArea
class_name WeaponPickup

@export var weapon_data: WeaponData

@onready var sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	super._ready()
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
