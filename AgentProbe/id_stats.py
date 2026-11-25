# -*- coding: utf-8 -*-
"""统计 Terraria/ID 下每个 ID 类的条目数，以及 zh-Hans 本地化里可用的中文名分组。

输出：AgentProbe/out/id_stats.txt
"""
import glob
import json
import os
import re

ID_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
LOC_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\Localization\Content\zh-Hans"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

CONST_PATTERN = re.compile(r"public const (?:short|int|byte) (\w+) = ")
SOUND_PATTERN = re.compile(r"public static readonly LegacySoundStyle (\w+) = ")
READONLY_PATTERN = re.compile(r"public static readonly \w+(?:<[^>]+>)? (\w+) = ")


def collect_id_counts():
    counts = {}
    for path in glob.glob(os.path.join(ID_DIR, "*.cs")):
        base = os.path.basename(path)[:-3]
        base = base.replace(".TML", "")
        with open(path, encoding="utf-8") as handle:
            text = handle.read()
        n_const = len(CONST_PATTERN.findall(text))
        n_sound = len(SOUND_PATTERN.findall(text))
        n_readonly = len(READONLY_PATTERN.findall(text))
        entry = counts.setdefault(base, {"const": 0, "sound": 0, "readonly": 0})
        entry["const"] += n_const
        entry["sound"] += n_sound
        entry["readonly"] += n_readonly
    return counts


def collect_loc_groups():
    groups = {}
    failed = []
    for path in glob.glob(os.path.join(LOC_DIR, "*.json")):
        try:
            with open(path, encoding="utf-8-sig") as handle:
                data = json.load(handle)
        except Exception as error:
            failed.append(f"{os.path.basename(path)}\t{type(error).__name__}")
            continue
        for key, value in data.items():
            if isinstance(value, dict):
                groups.setdefault(key, 0)
                groups[key] += len(value)
    return groups, failed


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    counts = collect_id_counts()
    groups, failed = collect_loc_groups()

    lines = []
    lines.append("=== ID class entry counts (const / legacy sound / readonly) ===")
    for name in sorted(counts, key=lambda n: -(counts[n]["const"] + counts[n]["sound"] + counts[n]["readonly"])):
        entry = counts[name]
        total = entry["const"] + entry["sound"] + entry["readonly"]
        if total == 0:
            continue
        lines.append(f"{name}\tconst={entry['const']}\tsound={entry['sound']}\treadonly={entry['readonly']}")

    lines.append("")
    lines.append("=== zh-Hans localization groups ===")
    for key in sorted(groups, key=lambda k: -groups[k]):
        lines.append(f"{key}\t{groups[key]}")

    lines.append("")
    lines.append("=== unreadable json ===")
    for item in failed:
        lines.append(item)

    out_path = os.path.join(OUT_DIR, "id_stats.txt")
    with open(out_path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(lines))

    print(f"report = {out_path}")
    print(f"id classes = {len([c for c in counts.values() if c['const'] + c['sound'] + c['readonly'] > 0])}")
    print(f"loc groups = {len(groups)}")


if __name__ == "__main__":
    main()
