extends Area2D
class_name ChoicePickup

func _ready() -> void:
	body_entered.connect(_on_body_entered)
	$Sprite2D.texture = preload("res://assets/sprites/pickups/choice-up.png")

func _on_body_entered(body: Node2D) -> void:
	if not (body is Player):
		return
	var choice_menu := get_tree().get_first_node_in_group("choice_menu") as ChoiceMenu
	if choice_menu != null:
		choice_menu.open()
	queue_free()
