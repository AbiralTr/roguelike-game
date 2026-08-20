extends InteractableArea
class_name Chest

@export var capacity: int = 8
@export var starting_items: Array[ItemStack] = []

var inventory: Inventory

@onready var sprite: Sprite2D = $Sprite2D

func _ready() -> void:
	super._ready()
	add_to_group("chests")

	inventory = Inventory.new()
	inventory.capacity = capacity
	for stack in starting_items:
		if stack != null and stack.item != null:
			inventory.add_item(stack.item, stack.quantity)

	_apply_visual()

func _apply_visual() -> void:
	if sprite == null:
		return
	sprite.texture = preload("res://assets/sprites/world/objects/chest.png")

func _process(_delta: float) -> void:
	if player_in_range == null:
		return
	if not Input.is_action_just_pressed("interact"):
		return
	var menu := get_tree().get_first_node_in_group("chest_menu") as ChestMenu
	if menu == null:
		return
	if menu.is_open_for(self):
		menu.close()
	else:
		menu.open(self, player_in_range)
