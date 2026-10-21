extends Control

@onready var nickname_edit: LineEdit = $Box/Nickname
@onready var url_edit: LineEdit = $Box/Url
@onready var roster_label: Label = $Box/Roster
@onready var status_label: Label = $Box/Status
@onready var start_button: Button = $Box/Start
@onready var color_rect: ColorRect = $Box/Target


func _ready() -> void:
	$Box/Row/Host.pressed.connect(func() -> void:
		Net.nickname = nickname_edit.text
		Net.host())
	$Box/Row/Join.pressed.connect(func() -> void:
		Net.nickname = nickname_edit.text
		Net.join(url_edit.text))
	start_button.pressed.connect(Net.start_round)
	for i in Net.COLORS.size():
		var button := Button.new()
		button.custom_minimum_size = Vector2(64, 64)
		button.modulate = Net.COLORS[i]
		button.pressed.connect(func() -> void: Net.tap.rpc_id(1, Net.COLORS[i]))
		$Box/Colors.add_child(button)
	Net.roster_changed.connect(_refresh)
	Net.status.connect(func(text: String) -> void: status_label.text = text)
	Net.round_started.connect(func(color: Color) -> void: color_rect.color = color)
	Net.round_finished.connect(func(scores: Dictionary) -> void: status_label.text = "Scores: %s" % str(scores))
	_refresh()


func _refresh() -> void:
	roster_label.text = ", ".join(Net.roster.values())
	start_button.visible = multiplayer.has_multiplayer_peer() and multiplayer.is_server()
