class_name Placeholder
extends RefCounted

static func make_texture(color: Color, size: Vector2i = Vector2i(32, 32)) -> ImageTexture:
	var image := Image.create(size.x, size.y, false, Image.FORMAT_RGBA8)
	image.fill(color)
	return ImageTexture.create_from_image(image)
