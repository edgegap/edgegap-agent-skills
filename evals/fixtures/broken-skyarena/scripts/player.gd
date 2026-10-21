extends CharacterBody2D

const SPEED := 300.0

@onready var camera: Camera2D = $Camera2D


func _enter_tree() -> void:
	set_multiplayer_authority(int(str(name)))


func _ready() -> void:
	camera.enabled = is_multiplayer_authority()


func _physics_process(_delta: float) -> void:
	if not is_multiplayer_authority():
		return
	var direction := Input.get_vector("ui_left", "ui_right", "ui_up", "ui_down")
	velocity = direction * SPEED
	move_and_slide()
