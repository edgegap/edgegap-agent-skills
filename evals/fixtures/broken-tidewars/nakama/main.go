package main

import (
	"context"
	"database/sql"
	"encoding/json"
	"time"

	"github.com/edgegap/nakama-edgegap/pkg/fleetmanager"
	"github.com/heroiclabs/nakama-common/runtime"
)

const (
	notificationConnectionInfo = 111
	notificationCreateTimeout  = 112
	notificationCreateFailed   = 113
)

type instanceCreateReply struct {
	Message string `json:"message"`
	Ok      bool   `json:"ok"`
}

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	initStart := time.Now()

	efm, err := fleetmanager.NewEdgegapFleetManager(ctx, logger, db, nk, initializer)
	if err != nil {
		return err
	}
	if err = initializer.RegisterFleetManager(efm); err != nil {
		return err
	}

	// From the plugin README.
	if err := initializer.RegisterAfterAuthenticateCustom(fleetmanager.OnAuthenticateUpdateCustom); err != nil {
		return err
	}

	if err = initializer.RegisterMatchmakerMatched(OnMatchmakerMatched); err != nil {
		return err
	}

	if err = nk.LeaderboardCreate(ctx, "tidewars_wins", false, "desc", "incr", "", nil, true); err != nil {
		return err
	}

	logger.Info("Tidewars module loaded in '%s'", time.Since(initStart).String())
	return nil
}

func OnMatchmakerMatched(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, entries []runtime.MatchmakerEntry) (string, error) {
	userIds := make([]string, 0, len(entries))
	for _, entry := range entries {
		userIds = append(userIds, entry.GetPresence().GetUserId())
	}

	callback := func(status runtime.FmCreateStatus, instanceInfo *runtime.InstanceInfo, sessionInfo []*runtime.SessionInfo, metadata map[string]any, createErr error) {
		switch status {
		case runtime.CreateSuccess:
			content := map[string]interface{}{
				"IpAddress":  instanceInfo.ConnectionInfo.IpAddress,
				"DnsName":    instanceInfo.ConnectionInfo.DnsName,
				"Port":       instanceInfo.ConnectionInfo.Port,
				"InstanceId": instanceInfo.Id,
			}
			for _, userId := range userIds {
				_ = nk.NotificationSend(ctx, userId, "connection-info", content, notificationConnectionInfo, "", false)
			}
		case runtime.CreateTimeout:
			for _, userId := range userIds {
				_ = nk.NotificationSend(ctx, userId, "create-timeout", map[string]interface{}{}, notificationCreateTimeout, "", false)
			}
		default:
			logger.WithField("error", createErr.Error()).Error("Failed to create Edgegap instance")
			for _, userId := range userIds {
				_ = nk.NotificationSend(ctx, userId, "create-failed", map[string]interface{}{}, notificationCreateFailed, "", false)
			}
		}
	}

	_, err := nk.GetFleetManager().Create(ctx, len(entries), userIds, nil, nil, callback)
	if err != nil {
		return "", err
	}

	reply, _ := json.Marshal(instanceCreateReply{Message: "Instance Created", Ok: true})
	return string(reply), nil
}
