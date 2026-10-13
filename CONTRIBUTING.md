# Contributing

Thanks for helping. The most valuable contributions are **end-to-end reports**: what you tried (netcode, transport, Unity version, platform), where the skill went wrong, and the logs.

## Editing the skills

- Each `skills/<name>/SKILL.md` is a workflow. Keep it under ~500 lines and put details in `references/`.
- The safety rules for acting on an Edgegap account live once, in `skills/edgegap/references/agent-rules.md`. Link to it instead of restating them.
- API facts come from Edgegap's published OpenAPI spec (snapshot in `spec/`). If the live API disagrees, the live API wins: fix the skill and say so in the changelog.
- Every skill has `metadata.version` in its frontmatter. Bump it when you change the skill (patch: wording and fixes; minor: new guidance; major: changed workflow), and bump `Pack version:` in `skills/edgegap/SKILL.md` with each release.
- The frontmatter `description` decides when agents load the skill. It must stay **under 1024 characters**, because longer descriptions get cut off or ignored. After changing it, re-run the trigger eval (below).
- Explain *why* in instructions rather than adding MUSTs. Agents follow reasons better.
- Never commit tokens, registry credentials, matchmaker URLs, or anything specific to a real account.

## Checks

```bash
python scripts/validate.py
python scripts/scan_private_refs.py
python scripts/check_spec.py
```

`validate.py` checks skill frontmatter (name, description length, `metadata.version`), relative links and skill names across skills, every JSON file, and the plugin manifests. `scan_private_refs.py` fails on anything that must not be public: personal paths and emails, real matchmaker or deployment hosts, registry robot accounts, token-shaped strings. CI runs both on every pull request. `check_spec.py` compares `spec/` with the published Edgegap OpenAPI spec; a weekly workflow runs it and opens an issue on drift (`--update` refreshes the snapshot).

If you change the C# templates or adapters, compile them in a real Unity project (Unity 6 LTS, SDK `https://github.com/edgegap/edgegap-unity-sdk.git#3.5.5`) with the netcode you touched, and build a Linux Dedicated Server once:

```bash
<Unity> -batchmode -quit -projectPath <project> -executeMethod EdgegapServerBuild.Build -logFile -
```

## Evals

- `evals/evals.json`: task prompts with assertions, run with and without the skill.
- `evals/trigger-eval.json`: 20 prompts (10 should trigger, 10 near-misses) to measure `edgegap-unity` triggering.
- `evals/routing-eval.json`: 20 prompts with the skill that should load (or none), to check the pack's descriptions don't steal each other's prompts.
- `evals/guardrails.json`: safety scenarios run with the MCP connected, in a test organization with no live game. An entry passes only if every `expect` holds and no `forbid` occurs. Record date and pass/total in the changelog; never commit transcripts, tokens or IDs.
- `evals/fixtures/ngo-arena`: a minimal Netcode for GameObjects host/client project (scripts only) for the NGO eval. The Mirror WebGL eval uses https://github.com/edgegap/mirror-webgl.

When measuring triggering with skill-creator's `run_eval.py`, use `--num-workers 1`. Parallel workers share one `.claude/commands` folder, so each run sees several copies of the skill and the trigger rate is understated.

## Releases

Bump `version` in `.claude-plugin/plugin.json` and `.claude-plugin/marketplace.json`, and add an entry to `CHANGELOG.md`.
