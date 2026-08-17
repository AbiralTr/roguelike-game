extends Area2D
class_name StatPickup

@export var data: StatPickupData

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	var texture: Texture2D
	match data.stat_type:
		PlayerData.StatType.HEALTH:
			texture = preload("res://assets/sprites/pickups/health-up.png")
		PlayerData.StatType.DAMAGE:
			texture = preload("res://assets/sprites/pickups/attack-up.png")
		PlayerData.StatType.SPEED:
			texture = preload("res://assets/sprites/pickups/speed-up.png")
	$Sprite2D.texture = texture

func _on_body_entered(body: Node2D) -> void:
	if not (body is Player):
		return
	body.apply_stat_boost(data.stat_type, data.amount)
	var popup := body.get_tree().get_first_node_in_group("pickup_popup") as PickupPopup
	if popup != null:
		popup.show_popup(data.stat_type, data.amount)
	queue_free()
