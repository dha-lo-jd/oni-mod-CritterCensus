# CLAUDE.md

ONI mod（Census シリーズ。兄弟は `../PlantCensus`）。Build: `dotnet build` (Debug copies to `ModInstallPath` from `Local.Build.props`). 共通事項は HQ（親フォルダ）の `CLAUDE.md`。

- `PlantCensus` はこの Mod を写して作った。`Ui/`（`UiFactory`、`UiStyle`、`ModalScreens`）と `Selection/` は同じ内容で、名前空間だけが違う。直すときは兄弟側にも同じ問題がないか見る。
- ツールボタンのアイコンは `Ui/ToolIcon.cs`。`CritterCensus/ModAssets/tool_icon.png` を読み、`Assets.Sprites` に自分の名前で登録する。ファイルがなければ捕獲ツールのアイコン名を返す。Debug ビルドは `ModAssets/*.png` を Mod フォルダへコピーする。**Release ビルドの配布フォルダ（`release/`）を作る設定がまだない**ので、Workshop へ上げるときはこの画像を含めること。
- アイコンの元画像は HQ の `_Incoming/CritterCensus/2026-11-13-icon-tool-3/`（候補 2 案。採用は案 1 の赤い枠）。加工は `tools/asset.py`（HQ の `ASSETS.md`）。

## 実機検証
ONI 実機で検証するには Gravitas.TestHarness を使う。テストは `[HarnessTest]` を付けた static メソッドとして書く（`CritterCensus/HarnessTests.cs`、`#if DEBUG && HARNESS`。`HARNESS` は `Local.Build.props` に `HarnessDllPath` があるときだけ定義される）。
実行: `python C:/Users/dha_1/RiderProjects/GravitasTestHarness/Gravitas.TestProbe/probe.py run --project . --tests '{"tags":["crittercensus"]}'`
ゲームを起動し本番の mods.json を書き換えるので、実行前にユーザーに確認する。詳細は harness repo の `README.md` と `protocol.md`。

テストが見ているもの: パッチの二重適用、ツールの登録位置、アイコンの読み込み、生き物の収集（ハッチ 1 匹）。
見ていないもの: 一覧画面の見た目と操作、ドラッグ操作、飼育小屋のクリック、卵・幼体の表示。
