# これは何？

箱庭ドローンの共有シミュレーションのセットアップ手順です。

共有シミュレーションは、複数の箱庭ドローンを同じ空間で飛ばすことができる機能です。
箱庭ドローンを２台のQUEST3にインストールして、互いのドローンを認識して、共有シミュレーションを行えるようになります。


# サポート環境

- Windows 11
- Python 3.12

# セットアップ手順

まずは、本リポジトリをクローンします。

```bash
git clone --recursive https://github.com/tmori/tutorial-hakoniwa-unity-drone.git
```

## 構成要素

本システムの構成は以下の通りです。

- QUEST3：２台
- PC(Windows)：１台
  - 箱庭Webサーバー
  - 箱庭ARブリッジ
  - 共有シミュレーション環境(Unityアプリケーション)

QUEST3とPCのIPアドレスは、同じネットワークに接続している必要があります。
ここでは、それぞれのIPアドレスを以下のように表記します。

- QUEST3-1: <IP_QUEST3_1>
- QUEST3-2: <IP_QUEST3_2>
- PC: <IP_PC>

## 設定ファイル

以下のファイルを編集して、IPアドレスを設定します。

- hakoniwa-ar-bridge/asset_lib/config
  - node.json
    - bridge_ip: <IP_PC>
    - web_ip: <IP_PC>
  - ar1_config.json
    - ar_ip: <IP_QUEST3_1>
  - ar2_config.json
    - ar_ip: <IP_QUEST3_2>

## インストール

以下から、QUEST3用のapkファイルをダウンロードしてください。

https://github.com/tmori/tutorial-hakoniwa-unity-drone/releases

- model1.apk
  - 1台目のQUEST3にインストールするapkファイル
- model2.apk
  - 2台目のQUEST3にインストールするapkファイル

共有シミュレーション環境として、以下のファイルをダウンロード＆解凍してください。

- ShareSimulation.zip

箱庭コア機能のPythonライブラリをダウンロードしてください。

- hakopy.pyd

箱庭コア機能が利用するMMAPファイルおよび設定ファイルをダウンロードしてください。

- mmap.zip
- cpp_core_config.json

Webサーバー用のPythonライブラリをインストールするため、
以下のリポジトリのディレクトリに移動します。

```bash
cd tutorial-hakoniwa-unity-drone/hakoniwa-webserver
```

Pythonライブラリをインストールします。

```bash
pip install -r requirements.txt
```

## 箱庭コア機能のセットアップ

1. Zドライブに、RAMDISKを作成してください。
2. Zドライブ直下に、mmap.zipを展開してください。
   - mmapディレクトリが作成され、以下のファイルが存在するはずです。
     - flock.bin
     - mmap-0xff.bin
     - mmap-0x100.bin
3. 任意のディレクトリ(ここでは、"E:¥hako" とします)に、cpp_core_config.jsonを配置してください。
4. 任意のディレクトリ(ここでは、"E:¥hako" とします)に、hakopy.pydを配置してください。
4. 環境変数を設定してください。
   - HAKO_CONFIG_PATH: "E:¥hako¥cpp_core_config.json"
   - PYTHONPATH: "E:¥hako"


# 共有シミュレーション実行手順

## 箱庭Webサーバーの起動

Powershellで、hakoniwa-webserverに移動します。

まず、以下のコマンドを実行して、箱庭コンダクタを起動します。

```powershell
python -m server.conductor --delta_time_usec 20000 --max_delay_time_usec 100000
```


次に、以下のコマンで、箱庭Webサーバーを起動します。

```powershell
python -m server.main --asset_name WebServer --config_path .\config\custom.json --delta_time_usec 20000
```

## 箱庭ARブリッジの起動

Powershellで、hakoniwa-ar-bridgeに移動します。

以下のコマンドを実行して、箱庭ARブリッジを起動します。

```powershell
python -m asset_lib.main
```

## 共有シミュレーションの実行

ShareSimulation内にある drone-simulation.exeをダブルクリックして、共有シミュレーション環境を起動します。

## QUEST3の起動

QUEST3を起動して、インストールしたapkファイルを起動します。


