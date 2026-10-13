"""Compare the vendored Edgegap OpenAPI snapshots in spec/ with the published ones.

  python scripts/check_spec.py            exit 1 and print a diff summary if they differ
  python scripts/check_spec.py --update   overwrite spec/ with the published specs

The skills' API facts (edgegap-deployments-api/references/api-reference.md) were written
from these snapshots. When the published spec changes, update the snapshot, re-read the
changed operations, fix the skills, and bump their metadata.version.
"""
import argparse
import difflib
import sys
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
BASE = "https://raw.githubusercontent.com/edgegap/openapi-specification/refs/heads/main/"
FILES = ["edgegap-v1-openapi.yaml", "edgegap-v2-openapi.yaml"]


def fetch(name: str) -> str:
    req = urllib.request.Request(BASE + name, headers={"User-Agent": "edgegap-agent-skills-spec-check"})
    with urllib.request.urlopen(req, timeout=30) as resp:
        return resp.read().decode("utf-8")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--update", action="store_true")
    args = parser.parse_args()

    changed = 0
    for name in FILES:
        local_path = ROOT / "spec" / name
        live = fetch(name).replace("\r\n", "\n")
        local = local_path.read_text(encoding="utf-8").replace("\r\n", "\n") if local_path.exists() else ""
        if live == local:
            print(f"OK: spec/{name} matches the published spec")
            continue
        changed += 1
        if args.update:
            local_path.write_text(live, encoding="utf-8", newline="\n")
            print(f"UPDATED: spec/{name}")
            continue
        diff = list(difflib.unified_diff(local.splitlines(), live.splitlines(),
                                         f"spec/{name} (snapshot)", f"{name} (published)", lineterm="", n=2))
        print(f"DRIFT: spec/{name} differs from the published spec ({len(diff)} diff lines)")
        print("\n".join(diff[:400]))
        if len(diff) > 400:
            print(f"... {len(diff) - 400} more lines")
    return 1 if changed and not args.update else 0


if __name__ == "__main__":
    sys.exit(main())
