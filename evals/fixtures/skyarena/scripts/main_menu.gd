extends Control

@onready var name_edit: LineEdit = $Panel/VBox/Name
@onready var address_edit: LineEdit = $Panel/VBox/Address
@onready var host_button: Button = $Panel/VBox/Buttons/Host
@onready var join_button: Button = $Panel/VBox/Buttons/Join
@onready var lobby_list: ItemList = $Panel/VBox/Lobby
@onready var start_button: Button = $Panel/VBox/Start
@onready var status: Label = $Panel/VBox/Status


func _ready() -> void:
	host_button.pressed.connect(func() -> void: Network.host_game(name_edit.text))
	join_button.pressed.connect(func() -> void: Network.join_game(address_edit.text, name_edit.text))
	start_button.pressed.connect(Network.start_game)
	Network.player_list_changed.connect(_refresh)
	Network.connection_failed_message.connect(func(text: String) -> void: status.text = text)
	_refresh()


func _refresh() -> void:
	lobby_list.clear()
	for id in Network.players:
		lobby_list.add_item(Network.players[id])
	# Only the host can start, once someone joined.
	start_button.visible = multiplayer.has_multiplayer_peer() and multiplayer.is_server()
	start_button.disabled = Network.players.size() < 2
