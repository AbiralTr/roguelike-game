extends Resource
class_name ItemData

@export var item_name: String = ""
@export var description: String = ""
@export var icon: Texture2D
@export var max_stack: int = 99

## Actions offered in the inventory context menu. Override in subclasses to
## add more (e.g. a consumable item appending "Eat").
func get_actions() -> Array[String]:
	return ["Drop"]
