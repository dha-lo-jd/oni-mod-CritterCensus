# Changelog

Critter Census の更新履歴。バージョンは `CritterCensus/mod_info.yaml` の `version`。新しい版を上に書く。
各版の「English / 日本語」のブロックは、そのまま Steam Workshop の変更ノートに貼れる。

## 1.0.3 — 2026-11-13（Workshop 更新待ち）

English
- Fixed an IndexOutOfRangeException thrown on every key press (the census button has no hotkey).
- Fixed the mod's patches being applied twice.
- New tool icon for the census button.

日本語
- キーを押すたびに IndexOutOfRangeException が出る問題を修正しました（調査ボタンにはホットキーがありません）。
- Mod のパッチが二重に適用されていた問題を修正しました。
- 調査ボタンに専用のアイコンを追加しました。

内部: 1.0.2 は Workshop に出していない（Harmony の二重適用の修正）。1.0.3 に含めて公開する。

## 1.0.1 — 2026-11-10（初回公開）

English
- First release: the census tool, the Stable (Creature Pen) click in the Rooms overlay, the full-screen list (stage, species, age, happiness, fertility, fullness, fur growth, temperature, vitality, ailments) with filters, sorting and row click to jump, and the Japanese translation.

日本語
- 初回公開: 調査ツール、部屋オーバーレイでの飼育小屋のクリック、全画面の一覧（成長段階、種、年齢、幸福度、繁殖率、満腹度、毛皮の成長率、体温、生命力、状態異常）、絞り込み・並べ替え・行クリックで移動、日本語訳。

内部: 1.0.0 は開発中の版で、Workshop には出していない。
