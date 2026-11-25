# -*- coding: utf-8 -*-
"""从本地 TML 源码生成 BuffID 表，并与 Tmod_Ref 里的表对比。

数据源：
  ID/BuffID.cs + ID/BuffID.TML.cs   -> 常量名 + 数值
  Localization/Content/zh-Hans/Game.json -> BuffName 段（中文名）

输出（AgentProbe/out/）：
  BuffID_generated.txt   值 \t 内部名 \t 中文名
  BuffID_compare.txt     比对报告
"""
import json
import os
import re

ID_DIR = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\ID"
LOC_PATH = r"e:\dev\tml260830\tModLoader-2026.08.3.0\src\tModLoader\Terraria\Localization\Content\zh-Hans\Game.json"
USER_TABLE = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\Tmod_Ref\ID\buffID表.txt"
OUT_DIR = r"d:\OneDrive\文档\My Games\Terraria\tModLoader\ModSources\ExpansionKele\AgentProbe\out"

CONST_PATTERN = re.compile(r"public const (?:short|int|byte) (\w+) = (-?\d+);")
# 用户表：数值(+tab) \n 中文名 \n 中文名 \n 内部名 \t 类型
USER_PATTERN = re.compile(r"^(\d+)[ \t]*\n[^\n]*\n[^\n]*\n([A-Za-z_][A-Za-z0-9_]*)\t", re.MULTILINE)


def read_constants():
    result = {}
    for file_name in ("BuffID.cs", "BuffID.TML.cs"):
        path = os.path.join(ID_DIR, file_name)
        if not os.path.exists(path):
            continue
        with open(path, encoding="utf-8") as handle:
            text = handle.read()
        for name, value in CONST_PATTERN.findall(text):
            result.setdefault(name, int(value))
    return result


def read_localized_names():
    with open(LOC_PATH, encoding="utf-8-sig") as handle:
        data = json.load(handle)
    return data.get("BuffName", {})


def read_text_auto(path):
    """按 BOM 自动选择编码：UTF-16 / UTF-8-BOM / UTF-8。"""
    raw = open(path, "rb").read()
    if raw[:2] in (b"\xff\xfe", b"\xfe\xff"):
        return raw.decode("utf-16")
    if raw[:3] == b"\xef\xbb\xbf":
        return raw.decode("utf-8-sig")
    return raw.decode("utf-8", errors="replace")


def read_user_table():
    text = read_text_auto(USER_TABLE).replace("\r\n", "\n").replace("\r", "\n")
    return {name: int(value) for value, name in USER_PATTERN.findall(text)}


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    constants = read_constants()
    localized = read_localized_names()
    user_table = read_user_table()

    generated_path = os.path.join(OUT_DIR, "BuffID_generated.txt")
    with open(generated_path, "w", encoding="utf-8") as handle:
        for name in sorted(constants, key=lambda n: (constants[n], n)):
            handle.write(f"{constants[name]}\t{name}\t{localized.get(name, '')}\n")

    only_source = sorted(set(constants) - set(user_table))
    only_user = sorted(set(user_table) - set(constants))
    mismatch = sorted(n for n in set(constants) & set(user_table) if constants[n] != user_table[n])
    missing_cn = sorted(n for n in constants if not localized.get(n))

    report = []
    report.append(f"source unique constants : {len(constants)}")
    report.append(f"user table parsed rows  : {len(user_table)}")
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

    report_path = os.path.join(OUT_DIR, "BuffID_compare.txt")
    with open(report_path, "w", encoding="utf-8") as handle:
        handle.write("\n".join(report))

    print(f"generated = {generated_path}")
    print(f"report    = {report_path}")
    print(f"src={len(constants)} user={len(user_table)} only_src={len(only_source)} "
          f"only_user={len(only_user)} mismatch={len(mismatch)} no_cn={len(missing_cn)}")


if __name__ == "__main__":
    main()
