# Troubleshooting: symptom → cause → fix

Every row below happened in a real run. Engine and compiler problems are in `engine-setup.md` §4, matchmaker ones in `matchmaker.md` §9.

## Engine, project, build

| Symptom | Cause | Fix |
|---|---|---|
| `git clone … UnrealEngine.git`: Repository not found | GitHub account not in the EpicGames org. | Link accounts and accept the org invite (engine-setup §1). |
| `could not create leading directories of 'C:/UE5'` | Command ran in WSL. | Run it in PowerShell or Git Bash on Windows. |
| Editor target fails: `modifies the values of properties … bStrictConformanceMode` | Old `BuildSettingsVersion` in targets. | Upgrade all targets (server-ready §2). |
| `action paths are longer than 260 characters` | Deep path on Windows. | Move the project to a short path. |
| Game launch exits: `Couldn't launch ShaderCompileWorker.exe` | Not built. | Build the `ShaderCompileWorker` target. |
| Editor/game launch sits on `Launching UnrealBuildTool… QueryTargets` | Another UBT run (packaging) holds the mutex. | Wait for it, or don't launch during packaging. |
| `-server` launch: map not found / wrong map | `ServerDefaultMap` unset, or the map isn't cooked. | Set it, add `+MapsToCook`. |

## Docker and image

| Symptom | Cause | Fix |
|---|---|---|
| `docker`: cannot find `dockerDesktopLinuxEngine` pipe | Docker Desktop not running. | Start it, wait for `docker info`. |
| Container exits at once, log mentions root | Running as root. | Non-root `USER` (the provided Dockerfile). |
| `docker logs` / Edgegap logs stop right after LoadMap | stdout buffered. | `-stdout -FullStdOutLogOutput` in `CMD`. |
| `docker stop` takes 10 s (killed) | A shell script is PID 1 and swallows SIGTERM. | `CMD` runs the server binary directly, exec form. |
| Image is 2 GB+ | `.debug`/`.sym` files copied. | Exclude them in `.dockerignore`. |
| Git Bash: `exec: "C:/Program Files/Git/app/…": no such file` | MSYS path conversion. | `MSYS_NO_PATHCONV=1 docker run …`. |
| Push fails 401 | Registry login expired or wrong account. | Developer re-runs `docker login registry.edgegap.com`. |

## Edgegap deployment

| Symptom | Cause | Fix |
|---|---|---|
| Deploy: "Unable to pull image" | No registry credentials on the version. | Developer adds them in the dashboard; check `registry_credentials_set`. |
| Deployed server seems to run old code after a push | Likely a re-used image tag that the host had already pulled. | New tag + new app version for every build. |
| Server log on a matchmaker deployment lacks `Started by the Edgegap matchmaker` | Old image (re-used tag), or the drop-in not compiled into the server. | Check with a fresh tag; run the image locally with `-e MM_MATCH_ID=test`. |
| Matchmaker deployment never stops | Self-stop inactive (see above), or a player still connected. | Logs show `Stopping deployment` when it works; `edgegap_stop_deployment` meanwhile. |

## Client connection

| Symptom | Cause | Fix |
|---|---|---|
| `Couldn't resolve host name` (host `tickets`) | Unquoted ini URL. | Quote `MatchmakerUrl`. |
| `assigned to …` then back to the menu | Connection failed (port, protocol, server crashed). | Direct deploy + `open ip:port`; check the version port is UDP 7777 named like `PortName`. |
| Overlay doesn't show | `bEnabled=False`, `bShowDefaultUI=False`, or the map isn't standalone. | Check the ini section name uses the module name. |
| Closing clients: server stops only ~75 s later | Killed clients don't say goodbye; the server waits for the connection timeout (~60 s), then the 15 s grace. | Expected. A clean disconnect stops it in ~15 s. |
