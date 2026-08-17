extends StaticBody2D

func _ready() -> void:
	add_to_group("ground")
	$GroundSprite.texture = Placeholder.make_texture(Color(0.32, 0.24, 0.18), Vector2i(2000, 100))
