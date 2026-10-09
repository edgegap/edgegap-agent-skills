# Contributing

Thanks for helping. The most valuable contributions are **end-to-end reports**: what you tried (netcode, transport, Unity version, platform), where the skill went wrong, and the logs.

## Editing the skill

- `skills/edgegap-unity/SKILL.md` is the workflow. Keep it under ~500 lines and put details in `references/`.
- The frontmatter `description` decides when agents load the skill. It must stay **under 1024 characters**, because longer descriptions get cut off or ignored. After changing it, re-run the trigger eval (below).
- Explain *why* in instructions rather than adding MUSTs. Agents follow reasons better.
- Never commit tokens, registry credentials, matchmaker URLs, or anything specific to a real account.

## Checks

```bash
python scripts/validate.py
```

The script validates the skill frontmatter (name, description length), every JSON file, and the plugin manifests. CI runs it on every pull request.

If you change the C# templates or adapters, compile them in a real Unity project (Unity 6 LTS, SDK `https://github.com/edgegap/edgegap-unity-sdk.git#3.5.5`) with the netcode you touched, and build a Linux Dedicated Server once:

```bash
<Unity> -batchmode -quit -projectPath <project> -executeMethod EdgegapServerBuild.Build -logFile -
```

## Evals

- `evals/evals.json`: task prompts with assertions, run with and without the skill.
- `evals/trigger-eval.json`: 20 prompts (10 should trigger, 10 near-misses) to measure description triggering.
- `evals/fixtures/ngo-arena`: a minimal Netcode for GameObjects host/client project (scripts only) for the NGO eval. The Mirror WebGL eval uses https://github.com/edgegap/mirror-webgl.

When measuring triggering with skill-creator's `run_eval.py`, use `--num-workers 1`. Parallel workers share one `.claude/commands` folder, so each run sees several copies of the skill and the trigger rate is understated.

## Releases

Bump `version` in `.claude-plugin/plugin.json` and `.claude-plugin/marketplace.json`, and add an entry to `CHANGELOG.md`.
