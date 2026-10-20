package main

import (
	"context"
	"database/sql"

	"github.com/heroiclabs/nakama-common/runtime"
)

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	// Wins leaderboard; clients write their own record when they win.
	if err := nk.LeaderboardCreate(ctx, "reefrush_wins", false, "desc", "incr", "", nil, true); err != nil {
		return err
	}
	logger.Info("Reef Rush module loaded")
	return nil
}
