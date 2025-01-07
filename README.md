# tutorial-hakoniwa-unity-drone

## 概要

本リポジトリでは、最新の箱庭ドローンコア機能を利用したUnityアプリケーションのサンプルコードを提供します。

## 動作環境

- Unity6.0.0
  - Android Build Support
- [Meta XR Core SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-core-sdk-269169)
- Meta Quest Link
- [Meta Quest Developer Hub](https://developers.meta.com/horizon/documentation/unity/ts-odh-getting-started)
- Hakoniwa Drone Core 1.0.0

## セットアップ手順

1. Unity Hubを起動し、Unity 6.0.0をインストールします。
2. 本リポジトリをクローンします。
3. Unity Hubでプロジェクトを開きます。
4. UnityのBuild Settingsを調整
  4.1. Platformの切り替え:
   - 「File」 > 「Build Settings」で「Android」を選択し、「Switch Platform」をクリック。
   - Player Settings:
      - Minimum API Levelを「Android 10.0 (API Level 29)」以上に設定。
      - Target API Levelを「自動」または最新バージョンに設定。
      - Scripting Backendを「IL2CPP」に設定。


## 参考サイト

- [Unity + Meta Quest開発メモ](https://tech.framesynthesis.co.jp/unity/metaquest/)
- [Quest3のアプリ開発でパススルー機能を使う方法](https://qiita.com/mofurune/items/fc9dd73f3adb29dd3934)

