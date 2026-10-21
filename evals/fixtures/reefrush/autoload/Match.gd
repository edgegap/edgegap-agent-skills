extends Node
# Matchmaking and the relayed match. The first player becomes the host.

signal match_ready(players)
signal error(message)

var bridge: NakamaMultiplayerBridge
var players := {}

func find_match() -> void:
	bridge = NakamaMultiplayerBridge.new(Online.nakama_socket)
	bridge.match_join_error.connect(func(e): emit_signal("error", e.message))
	bridge.match_joined.connect(_on_match_joined)
	multiplayer.multiplayer_peer = bridge.multiplayer_peer
	Online.nakama_socket.received_matchmaker_matched.connect(_on_matchmaker_matched)
	var ticket = await Online.nakama_socket.add_matchmaker_async("*", 2, 4)
	if ticket.is_exception():
		emit_signal("error", "Could not join matchmaking")

func _on_matchmaker_matched(matched: NakamaRTAPI.MatchmakerMatched) -> void:
	await bridge.join_matched_async(matched)

func _on_match_joined() -> void:
	# The host is whoever got peer id 1 from the bridge.
	players[multiplayer.get_unique_id()] = Online.nakama_session.username
	emit_signal("match_ready", players)

func is_host() -> bool:
	return multiplayer.is_server()
