class_name InventoryListView
extends RefCounted

## Rebuilds `list` with one button per stack in `inventory`. `on_pressed` is
## invoked as on_pressed.call(stack, button) when a row is clicked.
static func populate(list: VBoxContainer, inventory: Inventory, on_pressed: Callable) -> void:
	for child in list.get_children():
		child.queue_free()

	if inventory.stacks.is_empty():
		var empty_label := Label.new()
		empty_label.text = "Empty"
		list.add_child(empty_label)
		return

	for stack in inventory.stacks:
		var button := Button.new()
		button.text = "%s x%d" % [stack.item.item_name, stack.quantity]
		button.icon = stack.item.icon
		button.expand_icon = true
		button.pressed.connect(on_pressed.bind(stack, button))
		list.add_child(button)
