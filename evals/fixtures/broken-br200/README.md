# BR200 on Edgegap (eval copy)

Photon's BR200 Battle Royale sample 2.1.0, unchanged, in Unity 6000.3.11f1 on Windows, Fusion 2.1.1 build 2177. We added our own server start script, Dockerfile and test-client launcher (`edgegap/`, `tools/`).

This copy has no BR200 or Photon Fusion files: they are licensed, so `Assets/TPSBR/` and `Assets/Photon/Fusion/` only hold notes and the two Fusion settings files, trimmed. `ProjectSettings/ProjectSettings.asset` is trimmed to the keys that matter here, and `Packages/manifest.json` lists only the main packages. The Fusion App ID was removed from this copy; the real project has one.

See EDGEGAP-NOTES.md and logs/.
