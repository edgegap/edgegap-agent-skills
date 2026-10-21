extends Node
# Nakama sign-in and matchmaking.

var client: NakamaClient
var session: NakamaSession
var socket: NakamaSocket

signal server_found(ip, port)
signal error(message)

func _ready() -> void:
	client = Nakama.create_client("defaultkey", "127.0.0.1", 7350, "http")

func sign_in(email: String, password: String) -> void:
	session = await client.authenticate_email_async(email, password, null, true)
	socket = Nakama.create_socket_from(client)
	await socket.connect_async(session)
	socket.received_notification.connect(_on_notification)

func find_match() -> void:
	await socket.add_matchmaker_async("*", 2, 4)

func _on_notification(n) -> void:
	var info = JSON.parse_string(n.content)
	match n.code:
		111: emit_signal("server_found", info["IpAddress"], int(info["Port"]))
		_: emit_signal("error", "Unable to start a game server")

func on_server_connected() -> void:
	EdgegapServer.rpc_id(1, "hello", session.username)

# Battle.gd calls this on the winner's client.
func submit_win() -> void:
	await client.write_leaderboard_record_async(session, "tidewars_wins", 1)
