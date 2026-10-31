#!/usr/bin/env python3
"""Check local Markdown file links and GitHub-style heading anchors.

Standard-library only. Does not make network requests or modify files.
"""
from __future__ import annotations

import re
import sys
import unicodedata
from pathlib import Path
from urllib.parse import unquote, urlsplit

ROOT = Path(__file__).resolve().parents[1]
EXCLUDED = {".git", "bin", "obj", "dist", "node_modules", ".venv"}
INLINE_LINK = re.compile(r"!?\[[^\]]*\]\(([^)]+)\)")
HEADING = re.compile(r"^#{1,6}\s+(.+?)\s*$")
CODE_FENCE = re.compile(r"^\s*(?:`{3,}|~{3,})")
HTML_TAG = re.compile(r"<[^>]+>")
BACKTICKS = re.compile(r"`")
WHITESPACE = re.compile(r"\s")


def github_slug(value: str) -> str:
    value = HTML_TAG.sub("", BACKTICKS.sub("", value.strip().rstrip("#").strip()))
    value = value.lower()
    value = "".join(
        c for c in value
        if c in {"-", "_", " "} or
        unicodedata.category(c)[0] in {"L", "N", "M"} or
        c.isspace()
    )
    return WHITESPACE.sub("-", value)


def anchors(text: str) -> set[str]:
    seen: dict[str, int] = {}
    found: set[str] = set()
    for line in text.splitlines():
        match = HEADING.match(line)
        if not match:
            continue
        slug = github_slug(match.group(1))
        num = seen.get(slug, 0)
        seen[slug] = num + 1
        found.add(slug if num == 0 else f"{slug}-{num}")
    return found


def main() -> int:
    docs = [
        p for p in ROOT.rglob("*.md")
        if not any(part in EXCLUDED for part in p.relative_to(ROOT).parts)
    ]
    source = {p.resolve(): p.read_text(encoding="utf-8-sig") for p in docs}
    heading_map = {p: anchors(text) for p, text in source.items()}
    failures: list[str] = []
    checked = 0
    checked_anchors = 0

    for document, text in sorted(source.items()):
        fenced = False
        for number, line in enumerate(text.splitlines(), start=1):
            if CODE_FENCE.match(line):
                fenced = not fenced
                continue
            if fenced:
                continue

            for match in INLINE_LINK.finditer(line):
                url = match.group(1).strip().split(maxsplit=1)[0].strip("<>")
                if not url:
                    continue
                parts = urlsplit(url)
                if parts.scheme or parts.netloc or url.startswith("//"):
                    continue

                checked += 1
                target = ((document.parent / unquote(parts.path)).resolve()
                          if parts.path else document)
                if ROOT not in target.parents and target != ROOT:
                    failures.append(
                        f"{document.relative_to(ROOT)}:{number}: link leaves repository: {url}"
                    )
                    continue
                if not target.is_file():
                    failures.append(
                        f"{document.relative_to(ROOT)}:{number}: missing file: {url}"
                    )
                    continue
                if parts.fragment and target.suffix.lower() == ".md":
                    checked_anchors += 1
                    fragment = unquote(parts.fragment)
                    if fragment not in heading_map.get(target, set()):
                        failures.append(
                            f"{document.relative_to(ROOT)}:{number}: missing anchor: {url}"
                        )

    for error in failures:
        print("FAIL:", error)
    print(f"Documents: {len(source)}; local links: {checked}; "
          f"Markdown anchors: {checked_anchors}; failures: {len(failures)}")
    if failures:
        return 1
    print("PASS: local Markdown paths and heading anchors")
    return 0


if __name__ == "__main__":
    sys.exit(main())
