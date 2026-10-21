extends Node
# Nakama connection. Defaults point at the local docker compose Nakama.

var nakama_server_key := "defaultkey"
var nakama_host := "127.0.0.1"
var nakama_port := 7350
var nakama_scheme := "http"

var nakama_client: NakamaClient
var nakama_session: NakamaSession
var nakama_socket: NakamaSocket

func _ready() -> void:
	nakama_client = Nakama.create_client(nakama_server_key, nakama_host, nakama_port, nakama_scheme)

func sign_in() -> bool:
	var device_id := OS.get_unique_id()
	nakama_session = await nakama_client.authenticate_device_async(device_id, null, true)
	if nakama_session.is_exception():
		push_error("Sign-in failed: %s" % nakama_session)
		return false
	nakama_socket = Nakama.create_socket_from(nakama_client)
	var result = await nakama_socket.connect_async(nakama_session)
	return not result.is_exception()
