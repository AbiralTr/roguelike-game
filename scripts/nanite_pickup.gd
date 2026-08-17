extends CharacterBody2D
class_name NanitePickup

const GRAVITY: float = 980.0
const POP_SIDE_SPEED: float = 80.0
const POP_UP_SPEED_MIN: float = 180.0
const POP_UP_SPEED_MAX: float = 260.0
const NANITE_COLOR: Color = Color(0.3, 0.6, 1.0)
const NANITE_SIZE: Vector2i = Vector2i(8, 16)

@export var amount: int = 1

@onready var sprite: Sprite2D = $Sprite2D
@onready var pickup_area: Area2D = $PickupArea

func _ready() -> void:
	add_to_group("nanite_pickups")
	sprite.texture = Placeholder.make_texture(NANITE_COLOR, NANITE_SIZE)
	velocity = Vector2(randf_range(-POP_SIDE_SPEED, POP_SIDE_SPEED), -randf_range(POP_UP_SPEED_MIN, POP_UP_SPEED_MAX))
	pickup_area.body_entered.connect(_on_pickup_area_body_entered)

func _physics_process(delta: float) -> void:
	if not is_on_floor():
		velocity.y += GRAVITY * delta
	else:
		velocity = Vector2.ZERO
	move_and_slide()

func _on_pickup_area_body_entered(body: Node2D) -> void:
	if not (body is Player):
		return
	GameState.add_currency(amount)
	queue_free()
