---
applyTo: "Tmod_Ref/**"
description: "查阅本地资料库 Tmod_Ref（tModLoader Wiki 克隆 + ID 对照表）时的只读规则；该目录禁止任何修改。"
---

# `Tmod_Ref/` 只读参考库

`Tmod_Ref/` 是**离线参考资料库**，不属于模组内容：

- `tModLoader.wiki/` —— tModLoader 官方 Wiki 的 **git 克隆**（内含 `.git/`）
- `ID/*.txt` —— 物品 / 射弹 ID 中文对照表（对应版本 1.4.4.9）

## 必须遵守

- **只读**：禁止创建、修改、重命名、删除该目录下任何文件（含 `.md`、`.txt`、`.git/`）。
- 禁止对其执行 `git add` / `git commit` / `git pull` 等改变其状态的命令。
- 需要补充结论时，写进本项目自己的笔记（如 `.github/` 下的说明文件、或临时文件），**不要回写 `Tmod_Ref/`**。
- 若发现其内容与当前 TML 版本不符（例如 ID 数值差异、页面已过时），**先指出并向用户确认**，再决定是否采用。

## 使用方式

- 引用时写清来源，例：`Tmod_Ref/tModLoader.wiki/Basic-Netcode.md:47`、
  `Tmod_Ref/ID/泰拉瑞亚射弹ID1449.txt` 中 ID 307（`BlueFlare`）。
- 查找顺序：`Tmod_Ref/` → 本地 TML 源码 → 联网，不要凭记忆猜页面名。
- ✅ 它已被排除在提交与打包之外：`build.txt` 里有 `buildIgnore = Tmod_Ref/*`，`.gitignore` 也忽略了 `Tmod_Ref/`。
  若发现这两处配置被改动（或 `Tmod_Ref/` 出现在 `git status` 里），**主动提醒我恢复**。