extends CanvasLayer
class_name DamageOverlay

@onready var rect: ColorRect = $Rect

var material: ShaderMaterial

func _ready() -> void:
	add_to_group("damage_overlay")
	material = rect.material as ShaderMaterial

func trigger(strength: float = 0.02, duration: float = 0.25) -> void:
	if material == null:
		return
	material.set_shader_parameter("strength", strength)
	var tween := create_tween()
	tween.set_ignore_time_scale(true)
	tween.tween_method(func(v: float) -> void: material.set_shader_parameter("strength", v), strength, 0.0, duration)
