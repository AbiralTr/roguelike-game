extends CanvasLayer
class_name PickupPopup

const DISPLAY_DURATION: float = 1.0
const FADE_DURATION: float = 0.5
const STAT_LABELS: Array[String] = ["Max Health", "Damage", "Speed", "Crit Chance", "Attack Speed", "Dash Cooldown"]
const PERCENT_STATS: Array[PlayerData.StatType] = [PlayerData.StatType.CRIT_CHANCE, PlayerData.StatType.ATTACK_SPEED]

@onready var label: Label = $Label

var fade_tween: Tween

func _ready() -> void:
	add_to_group("pickup_popup")
	label.modulate.a = 0.0

static func format_boost(stat_type: PlayerData.StatType, amount: float) -> String:
	var amount_text: String
	if stat_type in PERCENT_STATS:
		amount_text = "%d%%" % int(round(amount * 100.0))
	else:
		amount_text = str(int(amount)) if amount == floor(amount) else ("%.2f" % amount)
	return "+%s %s" % [amount_text, STAT_LABELS[stat_type]]

func show_popup(stat_type: PlayerData.StatType, amount: float) -> void:
	label.text = format_boost(stat_type, amount)

	if fade_tween != null and fade_tween.is_valid():
		fade_tween.kill()

	label.modulate.a = 1.0
	fade_tween = create_tween()
	fade_tween.tween_interval(DISPLAY_DURATION)
	fade_tween.tween_property(label, "modulate:a", 0.0, FADE_DURATION)
