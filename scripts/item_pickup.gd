extends InteractableArea
class_name ItemPickup

@export var item: ItemData
@export var quantity: int = 1

@onready var sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	super._ready()
	_apply_visual()

func init(item_data: ItemData, amount: int) -> void:
	item = item_data
	quantity = amount
	_apply_visual()

func _apply_visual() -> void:
	if sprite == null:
		return
	if item != null and item.icon != null:
		sprite.texture = item.icon
	else:
		sprite.texture = Placeholder.make_texture(Color(0.6, 0.6, 0.6), Vector2i(14, 14))

func _process(_delta: float) -> void:
	if player_in_range == null or item == null:
		return
	if Input.is_action_just_pressed("interact"):
		var leftover: int = player_in_range.player_data.inventory.add_item(item, quantity)
		if leftover <= 0:
			queue_free()
		else:
			quantity = leftover
