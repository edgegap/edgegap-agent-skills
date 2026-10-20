// Starhaul Nakama module (TypeScript runtime).

const matchmakerMatched: nkruntime.MatchmakerMatchedFunction = function (ctx, logger, nk, matches) {
  const matchId = nk.matchCreate('salvage_match', { players: matches.length });
  logger.info('Created salvage match %s for %d players', matchId, matches.length);
  return matchId;
};

let InitModule: nkruntime.InitModule = function (ctx, logger, nk, initializer) {
  nk.leaderboardCreate('salvage_best', false, nkruntime.SortOrder.DESCENDING, nkruntime.Operator.BEST);
  initializer.registerMatch('salvage_match', salvageMatch);
  initializer.registerMatchmakerMatched(matchmakerMatched);
  logger.info('Starhaul module loaded');
};

// Relay-only match: forwards position packets between players.
const salvageMatch: nkruntime.MatchHandler<{ presences: { [id: string]: nkruntime.Presence } }> = {
  matchInit: (ctx, logger, nk, params) => ({ state: { presences: {} }, tickRate: 10, label: '' }),
  matchJoinAttempt: (ctx, logger, nk, dispatcher, tick, state, presence) => ({ state, accept: true }),
  matchJoin: (ctx, logger, nk, dispatcher, tick, state, presences) => {
    presences.forEach((p) => (state.presences[p.userId] = p));
    return { state };
  },
  matchLeave: (ctx, logger, nk, dispatcher, tick, state, presences) => {
    presences.forEach((p) => delete state.presences[p.userId]);
    return Object.keys(state.presences).length === 0 ? null : { state };
  },
  matchLoop: (ctx, logger, nk, dispatcher, tick, state, messages) => {
    messages.forEach((m) => dispatcher.broadcastMessage(m.opCode, m.data, null, m.sender));
    return { state };
  },
  matchTerminate: (ctx, logger, nk, dispatcher, tick, state) => ({ state }),
  matchSignal: (ctx, logger, nk, dispatcher, tick, state) => ({ state }),
};
