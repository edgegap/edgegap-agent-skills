#!/usr/bin/env node
// End-to-end test through the Edgegap matchmaker, the path the browser clients take:
// tickets -> HOST_ASSIGNED -> wss to the deployment -> full match -> the server stops itself.
//
//   MM_URL=https://<id>.edgegap.net MM_TOKEN=<matchmaker auth token> node tools/e2e.mjs <profile> [players]
//
// Uses only the matchmaker's client token, never the Edgegap API token. Edit makeInput() so the
// headless players actually play your game (moving toward the goal, shooting...), or the match may
// only end on time.

import { playSession } from "./headless-player.mjs";

const MM = (process.env.MM_URL ?? "").replace(/\/+$/, "");
const TOKEN = process.env.MM_TOKEN;
const [profile, playersArg] = process.argv.slice(2);
const PLAYERS = Number(playersArg ?? 2);
if (!MM || !TOKEN || !profile) {
  console.error("Usage: MM_URL=... MM_TOKEN=... node tools/e2e.mjs <profile> [players]");
  process.exit(1);
}

// Per-game test input. state = latest {phase, players, s}, me = {id, team}, info = welcome.info.
const makeInput = (state, me, info) => ({ mx: Math.sin(Date.now() / 700), mz: Math.cos(Date.now() / 900) });

async function mm(method, path, body) {
  for (let attempt = 0; ; attempt++) {
    let res;
    try {
      res = await fetch(MM + path, {
        method,
        headers: { Authorization: TOKEN, ...(body ? { "Content-Type": "application/json" } : {}) },
        body: body ? JSON.stringify(body) : undefined,
        signal: AbortSignal.timeout(15000),
      });
    } catch (err) {
      if (attempt < 4) {
        await new Promise((r) => setTimeout(r, 2000)); // transient network error
        continue;
      }
      throw err;
    }
    if (res.status === 429 && attempt < 8) {
      await new Promise((r) => setTimeout(r, 1000 * 2 ** Math.min(attempt, 3) + Math.random() * 500)); // rate limited
      continue;
    }
    if (!res.ok) throw new Error(`${method} ${path}: HTTP ${res.status} ${await res.text()}`);
    return res.json();
  }
}

async function queue(name) {
  const t0 = Date.now();
  const ticket = await mm("POST", "/tickets", { profile, attributes: {} });
  for (;;) {
    await new Promise((r) => setTimeout(r, 2000));
    const t = await mm("GET", `/tickets/${ticket.id}`);
    if (t.status === "HOST_ASSIGNED") {
      const a = t.assignment;
      console.log(`${name}: HOST_ASSIGNED after ${((Date.now() - t0) / 1000).toFixed(0)}s -> ${a.fqdn}`);
      return { ticketId: ticket.id, fqdn: a.fqdn, port: a.ports.gameport.external };
    }
    if (t.status === "CANCELLED") throw new Error(`${name}: ticket expired`);
  }
}

const problems = [];
const names = Array.from({ length: PLAYERS }, (_, i) => `e2e-${i + 1}`);
const seats = await Promise.all(names.map((n, i) => new Promise((r) => setTimeout(r, i * 300)).then(() => queue(n))));
if (new Set(seats.map((s) => s.fqdn)).size !== 1) problems.push("players were split across deployments (stale tickets from an earlier run? restart the matchmaker)");

const reports = await Promise.all(
  seats.map((s, i) => playSession({ url: `wss://${s.fqdn}:${s.port}`, name: names[i], ticket: s.ticketId, makeInput, maxSeconds: 600 }))
);
for (const r of reports) {
  if (r.error) problems.push(`${r.name}: ${r.error}`);
  if (r.match?.players !== PLAYERS) problems.push(`${r.name}: server saw ${JSON.stringify(r.match)} (expected ${PLAYERS} tickets)`);
  for (const p of ["countdown", "playing", "over"]) if (!r.phases.includes(p)) problems.push(`${r.name}: never saw ${p}`);
}
console.log(`result: ${reports[0].result?.winner}; teams: ${reports.map((r) => r.team).join(", ")}`);

// The deployment should stop itself shortly after the results screen.
const t0 = Date.now();
let stopped = false;
while (Date.now() - t0 < 90000 && !stopped) {
  try {
    await fetch(`https://${seats[0].fqdn}:${seats[0].port}/`, { signal: AbortSignal.timeout(4000) });
    await new Promise((r) => setTimeout(r, 3000));
  } catch {
    stopped = true;
  }
}
if (stopped) console.log(`deployment stopped itself ${((Date.now() - t0) / 1000).toFixed(0)}s after the match`);
else problems.push("deployment still answering 90s after the match");

console.log(problems.length ? `FAIL\n - ${problems.join("\n - ")}` : "PASS");
process.exit(problems.length ? 1 : 0);
