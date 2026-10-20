extends Node

## Networking autoload "Net". The host is peer 1 and also plays.

signal roster_changed
signal round_started(color: Color)
signal round_finished(scores: Dictionary)
signal status(text: String)

const PORT := 8910
const ROUND_SECONDS := 60.0
const COLORS := [Color.RED, Color.GREEN, Color.BLUE, Color.YELLOW]

var nickname := "Player"
var roster: Dictionary = {}
var scores: Dictionary = {}
var current_color := Color.WHITE
var _round_timer: SceneTreeTimer


func _ready() -> void:
	multiplayer.peer_connected.connect(func(id: int) -> void: print("peer %d connected" % id))
	multiplayer.peer_disconnected.connect(_on_peer_disconnected)
	multiplayer.connected_to_server.connect(func() -> void: hello.rpc_id(1, nickname))
	multiplayer.connection_failed.connect(func() -> void: status.emit("Connection failed."))
	multiplayer.server_disconnected.connect(func() -> void:
		roster.clear()
		status.emit("Host left.")
		roster_changed.emit())


func host() -> void:
	var peer := WebSocketMultiplayerPeer.new()
	var error := peer.create_server(PORT)
	if error != OK:
		status.emit("Can't host: %s" % error_string(error))
		return
	multiplayer.multiplayer_peer = peer
	roster[1] = nickname
	roster_changed.emit()
	status.emit("Hosting on port %d" % PORT)


func join(url: String) -> void:
	var peer := WebSocketMultiplayerPeer.new()
	var error := peer.create_client(url)
	if error != OK:
		status.emit("Bad address.")
		return
	multiplayer.multiplayer_peer = peer
	status.emit("Connecting…")


@rpc("any_peer", "reliable")
func hello(new_nickname: String) -> void:
	roster[multiplayer.get_remote_sender_id()] = new_nickname.left(16)
	set_roster.rpc(roster)
	roster_changed.emit()


@rpc("authority", "reliable")
func set_roster(new_roster: Dictionary) -> void:
	roster = new_roster
	roster_changed.emit()


## Host only, from the lobby's Start button.
func start_round() -> void:
	scores.clear()
	for id in roster:
		scores[id] = 0
	begin_round.rpc(COLORS.pick_random())
	_round_timer = get_tree().create_timer(ROUND_SECONDS)
	_round_timer.timeout.connect(func() -> void: end_round.rpc(scores))


@rpc("authority", "call_local", "reliable")
func begin_round(color: Color) -> void:
	current_color = color
	round_started.emit(color)


@rpc("any_peer", "call_local", "reliable")
func tap(color: Color) -> void:
	if not multiplayer.is_server():
		return
	var id := multiplayer.get_remote_sender_id()
	if id == 0:
		id = 1
	if color == current_color:
		scores[id] = scores.get(id, 0) + 1
		var next_color: Color = COLORS.pick_random()
		begin_round.rpc(next_color)


@rpc("authority", "call_local", "reliable")
func end_round(final_scores: Dictionary) -> void:
	round_finished.emit(final_scores)


func _on_peer_disconnected(id: int) -> void:
	roster.erase(id)
	if multiplayer.is_server():
		set_roster.rpc(roster)
	roster_changed.emit()
