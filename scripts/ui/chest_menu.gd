extends CanvasLayer
class_name ChestMenu

var chest: Chest = null
var player: Player = null

@onready var chest_list: VBoxContainer = $Panel/Margin/Root/Columns/ChestArea/ChestList
@onready var player_list: VBoxContainer = $Panel/Margin/Root/Columns/PlayerArea/PlayerList

func _ready() -> void:
	add_to_group("chest_menu")
	visible = false

func is_open_for(target_chest: Chest) -> bool:
	return visible and chest == target_chest

func open(target_chest: Chest, target_player: Player) -> void:
	if not GameState.request_open_chest_menu():
		return
	chest = target_chest
	player = target_player
	visible = true
	_refresh()

func close() -> void:
	if not GameState.request_close_chest_menu():
		return
	visible = false
	chest = null
	player = null

func _refresh() -> void:
	if chest == null or player == null:
		return
	InventoryListView.populate(chest_list, chest.inventory, _on_transfer.bind(chest.inventory, player.player_data.inventory))
	InventoryListView.populate(player_list, player.player_data.inventory, _on_transfer.bind(player.player_data.inventory, chest.inventory))

func _on_transfer(source: Inventory, target: Inventory, stack: ItemStack, button: Button) -> void:
	if source.transfer_to(target, stack.item, 1):
		_refresh()
	else:
		_flash_failure(button)

func _flash_failure(button: Button) -> void:
	button.modulate = Color(1.0, 0.45, 0.45)
	var tween := create_tween()
	tween.tween_property(button, "modulate", Color.WHITE, 0.25)
