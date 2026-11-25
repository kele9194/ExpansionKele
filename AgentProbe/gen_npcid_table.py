# -*- coding: utf-8 -*-
"""从本地 TML 源码生成 NPCID 表，并与 Tmod_Ref 里现有的表逐条对比。

数据源：
  ID/NPCID.cs + ID/NPCID.TML.cs   -> 常量名 + 数值
  Localization/Content/zh-Hans/NPCs.json -> 中文名（NPCName 段）

输出（写到 AgentProbe/out/，不碰 Tmod_Ref/）：
  NPCID_generated.txt   值 \t 内部名 \t 中文名
  NPCID_compare.txt     比对报告
"""
import json
import os
import re

SRC_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
LOC_PATH = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\Localization\Content\zh-Hans\NPCs.json"
USER_TABLE = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\ID\NPCID表.txt"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

CONST_PATTERN = re.compile(r"public const (?:short|int|byte) (\w+) = (-?\d+);")
NAME_PATTERN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*")


def read_constants():
    """读出所有常量名与数值，按常量名去重。"""
    result = {}
    for file_name in ("NPCID.cs", "NPCID.TML.cs"):
        path = os.path.join(SRC_DIR, file_name)
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as handle:
            text = handle.read()
        for name, value in CONST_PATTERN.findall(text):
            result.setdefault(name, int(value))
    return result


def read_localized_names():
    with open(LOC_PATH, encoding="utf-8") as handle:
        data = json.load(handle)
    return data.get("NPCName", {})


def read_user_table():
    """解析用户表：以最后一列（内部名）为键，取第一列为数值。

    该表的中文名里含换行，所以按「整行能否解析成 \t 分隔且末列是合法标识符」来判断。
    """
    entries = {}
    unparsed = 0
    with open(USER_TABLE, encoding="utf-8", errors="replace") as handle:
        for line in handle:
            parts = line.rstrip("\n").split("\t")
            if len(parts) < 2:
                unparsed += 1
                continue
            name = parts[-1].strip()
            if not NAME_PATTERN.fullmatch(name):
                unparsed += 1
                continue
            try:
                value = int(parts[0].strip())
            except ValueError:
                unparsed += 1
                continue
            entries[name] = value
    return entries, unparsed


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    constants = read_constants()
    localized = read_localized_names()
    user_table, unparsed = read_user_table()

    generated_path = os.path.join(OUT_DIR, "NPCID_generated.txt")
    with open(generated_path, "w", encoding="utf-8") as handle:
        for name in sorted(constants, key=lambda n: (constants[n], n)):
            handle.write(f"{constants[name]}\t{name}\t{localized.get(name, '')}\n")

    only_source = sorted(set(constants) - set(user_table))
    only_user = sorted(set(user_table) - set(constants))
    mismatch = sorted(n for n in set(constants) & set(user_table) if constants[n] != user_table[n])
    missing_cn = sorted(n for n in constants if not localized.get(n))

    report = []
    report.append(f"source unique constants : {len(constants)}")
    report.append(f"user table parsed rows  : {len(user_table)}  (unparsed lines: {unparsed})")
    report.append(f"constants without CN    : {len(missing_cn)}")
    report.append(f"only in source          : {len(only_source)}")
    report.append(f"only in user table      : {len(only_user)}")
    report.append(f"value mismatch          : {len(mismatch)}")
    report.append("")

    report.append("=== value mismatch ===")
    for name in mismatch[:80]:
        report.append(f"{name}\tsource={constants[name]}\tuser={user_table[name]}")
    report.append("")

    report.append("=== only in source ===")
    for name in only_source[:200]:
        report.append(f"{name}\t{constants[name]}\t{localized.get(name, '')}")
    report.append("")

    report.append("=== only in user table ===")
    for name in only_user[:200]:
        report.append(f"{name}\t{user_table[name]}")
    report.append("")

    report.append("=== constants without CN name ===")
    for name in missing_cn[:200]:
        report.append(f"{name}\t{constants[name]}")

    report_path = os.path.join(OUT_DIR, "NPCID_compare.txt")
    with open(report_path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(report))

    print(f"generated = {generated_path}")
    print(f"report    = {report_path}")
    print(f"src={len(constants)} user={len(user_table)} only_src={len(only_source)} "
          f"only_user={len(only_user)} mismatch={len(mismatch)} no_cn={len(missing_cn)}")


if __name__ == "__main__":
    main()
