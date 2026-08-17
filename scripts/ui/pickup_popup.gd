extends CanvasLayer
class_name PickupPopup

const DISPLAY_DURATION: float = 1.0
const FADE_DURATION: float = 0.5
const STAT_LABELS: Array[String] = ["Max Health", "Damage", "Speed"]

@onready var label: Label = $Label

var fade_tween: Tween

func _ready() -> void:
	add_to_group("pickup_popup")
	label.modulate.a = 0.0

func show_popup(stat_type: PlayerData.StatType, amount: float) -> void:
	var label_name: String = STAT_LABELS[stat_type]
	var amount_text: String = str(int(amount)) if amount == floor(amount) else ("%.1f" % amount)
	label.text = "+%s %s" % [amount_text, label_name]

	if fade_tween != null and fade_tween.is_valid():
		fade_tween.kill()

	label.modulate.a = 1.0
	fade_tween = create_tween()
	fade_tween.tween_interval(DISPLAY_DURATION)
	fade_tween.tween_property(label, "modulate:a", 0.0, FADE_DURATION)
