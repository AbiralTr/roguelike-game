extends CanvasLayer
class_name StatMenu

var is_open: bool = false
var player: Player = null
var item_action_menu: PopupMenu
var pending_stack: ItemStack = null

@onready var health_label: Label = $Panel/StatsArea/StatsBox/HealthLabel
@onready var damage_label: Label = $Panel/StatsArea/StatsBox/DamageLabel
@onready var speed_label: Label = $Panel/StatsArea/StatsBox/SpeedLabel
@onready var crit_label: Label = $Panel/StatsArea/StatsBox/CritLabel
@onready var attack_speed_label: Label = $Panel/StatsArea/StatsBox/AttackSpeedLabel
@onready var weapon_label: Label = $Panel/InvArea/WeaponLabel
@onready var inventory_list: VBoxContainer = $Panel/InvArea/InventoryList
@onready var currency_label: Label = $Panel/TerminalArea/TerminalBox/CurrencyLabel
@onready var score_label: Label = $Panel/TerminalArea/TerminalBox/ScoreLabel
@onready var time_label: Label = $Panel/TerminalArea/TerminalBox/TimeLabel

func _ready() -> void:
	visible = false
	player = get_tree().get_first_node_in_group("player") as Player
	item_action_menu = PopupMenu.new()
	add_child(item_action_menu)
	item_action_menu.id_pressed.connect(_on_item_action_selected)

func _input(event: InputEvent) -> void:
	if event.is_action_pressed("toggle_stat_menu"):
		toggle()

func toggle() -> void:
	var accepted: bool = GameState.request_close_stat_menu() if is_open else GameState.request_open_stat_menu()
	if accepted:
		is_open = not is_open
		visible = is_open
		if is_open:
			_refresh_inventory()
		else:
			item_action_menu.hide()

func _process(_delta: float) -> void:
	if not is_open or player == null:
		return
	var pd: PlayerData = player.player_data
	health_label.text = "Health: %s / %s" % [int(pd.current_health), int(pd.max_health)]
	damage_label.text = "Melee Damage: %s\nProjectile Damage: %s" % [pd.melee_damage, pd.projectile_damage]
	speed_label.text = "Speed: %.1f" % pd.move_speed
	crit_label.text = "Crit Chance: %d%%" % int(round(pd.crit_chance * 100.0))
	attack_speed_label.text = "Attack Speed: +%d%%" % int(round(pd.attack_speed_bonus * 100.0))
	weapon_label.text = "Weapon: %s" % (player.equipped_weapon.weapon_name if player.equipped_weapon != null else "None")

	currency_label.text = "Nanites: %d" % GameState.run_currency
	score_label.text = "Score: %d" % GameState.run_score
	var total_seconds: int = int(GameState.run_elapsed_time)
	time_label.text = "Time: %02d:%02d" % [total_seconds / 60, total_seconds % 60]

func _refresh_inventory() -> void:
	InventoryListView.populate(inventory_list, player.player_data.inventory, _on_item_pressed)

func _on_item_pressed(stack: ItemStack, _button: Button) -> void:
	pending_stack = stack
	var actions: Array[String] = stack.item.get_actions()

	item_action_menu.clear()
	for i in actions.size():
		item_action_menu.add_item(actions[i], i)

	item_action_menu.position = Vector2i(get_viewport().get_mouse_position())
	item_action_menu.popup()

func _on_item_action_selected(id: int) -> void:
	if pending_stack == null:
		return
	var stack: ItemStack = pending_stack
	pending_stack = null

	match stack.item.get_actions()[id]:
		"Drop":
			_drop_item(stack)

func _drop_item(stack: ItemStack) -> void:
	var inv: Inventory = player.player_data.inventory
	var item: ItemData = stack.item
	var amount: int = stack.quantity
	if not inv.remove_item(item, amount):
		return

	var pickup: ItemPickup = preload("res://scenes/ItemPickup.tscn").instantiate()
	get_tree().current_scene.add_child(pickup)
	pickup.global_position = player.global_position
	pickup.init(item, amount)

	_refresh_inventory()
