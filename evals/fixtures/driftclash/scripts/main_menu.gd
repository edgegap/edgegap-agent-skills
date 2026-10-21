extends Control

@onready var status: Label = $Status
@onready var address: LineEdit = $Address
@onready var race_button: Button = $RaceButton


func _ready() -> void:
	race_button.disabled = true
	PlayFab.logged_in.connect(func(id): status.text = "Logged in as %s" % id; race_button.disabled = false)
	PlayFab.failed.connect(func(msg): status.text = msg)
	Network.connected.connect(func(): get_tree().change_scene_to_file("res://scenes/race.tscn"))
	Network.connection_failed.connect(func(): status.text = "Could not connect"; race_button.disabled = false)
	race_button.pressed.connect(_on_race)
	PlayFab.login()


func _on_race() -> void:
	race_button.disabled = true
	status.text = "Connecting..."
	Network.join(address.text)
