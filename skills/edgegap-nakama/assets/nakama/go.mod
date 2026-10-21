// ADAPT: the module path. Versions: nakama-common must be the exact version
// that the Nakama release (and its pluginbuilder image) was built with;
// v1.46.0 matches Nakama 3.39.0. nakama-edgegap is pinned by commit
// pseudo-version because its tags ("0.4.0") have no "v" prefix, so Go can't
// use them: `go get github.com/edgegap/nakama-edgegap@20d849e`.
module example.com/mygame/nakama

go 1.26.3

require (
	github.com/edgegap/nakama-edgegap v0.0.0-20260618130934-20d849e39d46
	github.com/heroiclabs/nakama-common v1.46.0
)

require google.golang.org/protobuf v1.36.11 // indirect
