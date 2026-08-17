extends CanvasLayer
class_name ChoiceMenu

@export var pool: Array[StatPickupData] = []

var current_options: Array[StatPickupData] = [null, null, null]

@onready var option_a: Button = $Center/Options/OptionA
@onready var option_b: Button = $Center/Options/OptionB
@onready var option_c: Button = $Center/Options/OptionC

func _ready() -> void:
	add_to_group("choice_menu")
	visible = false
	option_a.pressed.connect(_choose.bind(0))
	option_b.pressed.connect(_choose.bind(1))
	option_c.pressed.connect(_choose.bind(2))

func open() -> void:
	if pool.size() < 3:
		push_warning("ChoiceMenu needs at least 3 entries in its pool.")
		return
	if not GameState.request_open_choice_menu():
		return
	_roll_options()
	visible = true

func _roll_options() -> void:
	var shuffled: Array[StatPickupData] = pool.duplicate()
	shuffled.shuffle()
	current_options[0] = shuffled[0]
	current_options[1] = shuffled[1]
	current_options[2] = shuffled[2]

	option_a.text = _describe(current_options[0])
	option_b.text = _describe(current_options[1])
	option_c.text = _describe(current_options[2])

func _describe(data: StatPickupData) -> String:
	var stat_name: String = PlayerData.StatType.keys()[data.stat_type]
	var amount_text: String = str(int(data.amount)) if data.amount == floor(data.amount) else ("%.1f" % data.amount)
	return "+%s %s" % [amount_text, stat_name]

func _choose(index: int) -> void:
	var chosen: StatPickupData = current_options[index]
	var player := get_tree().get_first_node_in_group("player") as Player
	if player != null and chosen != null:
		player.apply_stat_boost(chosen.stat_type, chosen.amount)
	visible = false
	GameState.request_close_choice_menu()
