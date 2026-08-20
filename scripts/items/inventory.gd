extends Resource
class_name Inventory

@export var capacity: int = 10

var stacks: Array[ItemStack] = []

func get_quantity(item: ItemData) -> int:
	for stack in stacks:
		if stack.item == item:
			return stack.quantity
	return 0

func is_full() -> bool:
	return stacks.size() >= capacity

## Adds up to `quantity` of `item`, filling existing stacks before opening new
## slots. Returns the amount that didn't fit (0 if it all fit).
func add_item(item: ItemData, quantity: int = 1) -> int:
	var remaining: int = quantity
	var stack_limit: int = max(1, item.max_stack)

	for stack in stacks:
		if stack.item != item or stack.quantity >= stack_limit:
			continue
		var added: int = min(stack_limit - stack.quantity, remaining)
		stack.quantity += added
		remaining -= added
		if remaining <= 0:
			break

	while remaining > 0 and stacks.size() < capacity:
		var added: int = min(stack_limit, remaining)
		var new_stack := ItemStack.new()
		new_stack.item = item
		new_stack.quantity = added
		stacks.append(new_stack)
		remaining -= added

	if remaining < quantity:
		emit_changed()
	return remaining

## Removes up to `quantity` of `item`. Returns false (and removes nothing) if
## the inventory doesn't hold that much.
func remove_item(item: ItemData, quantity: int = 1) -> bool:
	if get_quantity(item) < quantity:
		return false

	var remaining: int = quantity
	for i in range(stacks.size() - 1, -1, -1):
		var stack: ItemStack = stacks[i]
		if stack.item != item:
			continue
		var removed: int = min(stack.quantity, remaining)
		stack.quantity -= removed
		remaining -= removed
		if stack.quantity <= 0:
			stacks.remove_at(i)
		if remaining <= 0:
			break

	emit_changed()
	return true

## Moves up to `quantity` of `item` from this inventory into `target`,
## respecting target's capacity. Returns false if nothing was moved.
func transfer_to(target: Inventory, item: ItemData, quantity: int = 1) -> bool:
	if get_quantity(item) < quantity:
		return false
	var leftover: int = target.add_item(item, quantity)
	var moved: int = quantity - leftover
	if moved <= 0:
		return false
	remove_item(item, moved)
	return true
