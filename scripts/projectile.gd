extends Area2D
class_name Projectile

var direction: Vector2 = Vector2.RIGHT
var speed: float = 500.0
var damage: int = 10
var max_distance: float = 350.0
var target_group: String = "enemies"
var start_position: Vector2

func launch(dir: Vector2, dmg: int, spd: float, dist: float, group: String) -> void:
	direction = dir.normalized() if dir.length() > 0.0 else Vector2.RIGHT
	damage = dmg
	speed = spd
	max_distance = dist
	target_group = group
	$Sprite2D.flip_h = direction.x < 0.0
	start_position = global_position

func _ready() -> void:
	$Sprite2D.texture = preload("res://assets/sprites/player/bullet.png")
	body_entered.connect(_on_body_entered)

func _physics_process(delta: float) -> void:
	global_position += direction * speed * delta
	if global_position.distance_to(start_position) >= max_distance:
		queue_free()

func _on_body_entered(body: Node2D) -> void:
	if body.is_in_group(target_group):
		if body.has_method("take_damage"):
			if target_group == "player":
				body.take_damage(damage, global_position)
			else:
				body.take_damage(damage)
		queue_free()
	elif body.is_in_group("ground"):
		queue_free()
	elif body.is_in_group("platforms"):
		queue_free()
