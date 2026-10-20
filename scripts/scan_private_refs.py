"""Fail if the repository contains things that must not be public.

Run: python scripts/scan_private_refs.py   (exit code 1 on any hit)

Rules (each hit prints `path:line: rule: excerpt`):
  personal-path   a user's home folder: C:\\Users\\<name>, /Users/<name>, /home/<name>
  personal-email  any email address except the generic mailboxes in ALLOWED_EMAILS
  matchmaker-url  a real matchmaker or deployment host (<id>.edgegap.net) instead of a placeholder
  registry-user   a real Edgegap registry robot account (robot$<org>+...)
  secret-hex      a 32-character hex string, the shape of deployment delete/context tokens
  uuid            a UUID, the shape of Edgegap API and matchmaker tokens
  token-literal   a long literal value assigned to a token/password/secret field

This script is skipped. Known-safe examples are listed
in ALLOWED with the reason.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
SKIP_DIRS = {".git", "node_modules", "__pycache__"}
SKIP_FILES = {"scripts/scan_private_refs.py"}
TEXT_SUFFIXES = {".md", ".json", ".cs", ".txt", ".asset", ".yml", ".yaml", ".py", ".sh", ""}

ALLOWED_EMAILS = {"noreply@anthropic.com", "info@edgegap.com", "support@edgegap.com"}

# (substring, reason): hits containing the substring are ignored.
ALLOWED = [
    ("f68e011bfb01.pr.edgegap.net", "example deployment URL from the Edgegap docs"),
    ("5fa53fa00a57.pr.edgegap.net", "example deployment URL from the Edgegap docs"),
    ("<request_id>.pr.edgegap.net", "placeholder"),
    ("om-k2x9fj3q1a.edgegap.net", "fictional matchmaker in the broken-kartclash eval fixture"),
    ("om-a1b2c3d4e5.edgegap.net", "fictional matchmaker in a FishNet routing eval query"),
    ("om-p7q2w9z4x1.edgegap.net", "fictional matchmaker in the broken-orbitbrawl eval fixture"),
    ("om-rr7k2pq9xd.edgegap.net", "fictional matchmaker in the broken-rocketrumble eval fixture"),
    ("VITE_MM_TOKEN=3c9b7e1d-fake-4a6b-9c1d-matchmaker000", "fake matchmaker token in the broken-rocketrumble eval fixture"),
    ('Token = "PASTE_MATCHMAKER_AUTH_TOKEN', "placeholder in the broken-kartclash eval fixture"),
    ("<id>.edgegap.net", "placeholder"),
    ("dGhlIHNhbXBsZSBub25jZQ==", "RFC 6455 sample WebSocket key"),
]

RULES = [
    ("personal-path", re.compile(r"(?i)(?:[a-z]:\\+users\\+(?!public\b|<)[a-z0-9._-]+|/Users/(?!Shared\b|<)[A-Za-z0-9._-]+|/home/(?!server\b|ubuntu\b|node\b|user\b|<)[a-z0-9._-]+)")),
    ("personal-email", re.compile(r"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b")),
    ("matchmaker-url", re.compile(r"\b[a-z0-9][a-z0-9-]{5,}(?:\.pr)?\.edgegap\.net\b")),
    ("registry-user", re.compile(r"robot\$(?!<)[a-z0-9][a-z0-9-]{3,}\+")),
    ("secret-hex", re.compile(r"\b[0-9a-f]{32}\b")),
    ("token-literal", re.compile(r"(?i)(?:token|password|secret)[\"']?\s*[:=]\s*[\"']?(?![<$])[A-Za-z0-9_\-]{20,}")),
    ("uuid", re.compile(r"\b[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}\b")),
]


def scan() -> list[str]:
    hits = []
    for path in sorted(ROOT.rglob("*")):
        rel = path.relative_to(ROOT).as_posix()
        if not path.is_file() or rel in SKIP_FILES or SKIP_DIRS & set(path.relative_to(ROOT).parts):
            continue
        if path.suffix not in TEXT_SUFFIXES:
            continue
        for n, line in enumerate(path.read_text(encoding="utf-8", errors="ignore").splitlines(), 1):
            for rule, pattern in RULES:
                for m in pattern.finditer(line):
                    found = m.group(0)
                    if rule == "personal-email" and found.lower() in ALLOWED_EMAILS:
                        continue
                    if rule == "secret-hex" and "guid:" in line:
                        continue  # Unity asset GUIDs share the shape
                    if rule == "uuid" and found.startswith("xxxxxxxx"):
                        continue
                    if any(found in safe for safe, _ in ALLOWED):
                        continue
                    hits.append(f"{rel}:{n}: {rule}: {found[:60]}")
    return hits


if __name__ == "__main__":
    found = scan()
    if found:
        print("\n".join(found))
        print(f"\n{len(found)} hit(s). Replace with placeholders, or add a reasoned entry to ALLOWED.")
        sys.exit(1)
    print("No private references found.")
