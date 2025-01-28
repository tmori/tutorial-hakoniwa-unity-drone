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
python -m server.conductor  --delta_time_usec 20000 --max_delay_time_usec 100000
```

実行例：
```
PS E:\project\temp\hakoniwa-webserver> python -m server.conductor  --delta_time_usec 20000 --max_delay_time_usec 100000
INFO: hako_conductor thread start
```


次に、以下のコマンで、箱庭Webサーバーを起動します。

```powershell
python -m server.main --asset_name WebServer --config_path .\config\custom.json --delta_time_usec 20000
```

実行例：
```
PS E:\project\temp\hakoniwa-webserver> python -m server.main --asset_name WebServer --config_path .\config\custom.json --delta_time_usec 20000
INFO: start http server
INFO: start websocket server
run webserver
INFO: Success for external initialization.
pdu writer: Player1_head
pdu create: Player1_head 0 72
INFO: Player1_head create_lchannel: logical_id=0 real_id=0 size=72
pdu writer: Player1_left_hand
pdu create: Player1_left_hand 0 72
INFO: Player1_left_hand create_lchannel: logical_id=0 real_id=1 size=72
pdu writer: Player1_right_hand
pdu create: Player1_right_hand 0 72
INFO: Player1_right_hand create_lchannel: logical_id=0 real_id=2 size=72
set event loop on asyncio
pdu writer: Player2_head
Starting WebSocket server...
pdu create: Player2_head 0 72
INFO: Player2_head create_lchannel: logical_id=0 real_id=3 size=72
pdu writer: Player2_left_hand
pdu create: Player2_left_hand 0 72
INFO: Player2_left_hand create_lchannel: logical_id=0 real_id=4 size=72
pdu writer: Player2_right_hand
pdu create: Player2_right_hand 0 72
INFO: Player2_right_hand create_lchannel: logical_id=0 real_id=5 size=72
WebSocket server started on ws://0.0.0.0:8765
pdu writer: Drone1
pdu create: Drone1 1 72
INFO: Drone1 create_lchannel: logical_id=1 real_id=6 size=72
pdu writer: Drone1
pdu create: Drone1 0 112
INFO: Drone1 create_lchannel: logical_id=0 real_id=7 size=112
pdu writer: Drone2
pdu create: Drone2 1 72
INFO: Drone2 create_lchannel: logical_id=1 real_id=8 size=72
pdu writer: Drone2
pdu create: Drone2 0 112
INFO: Drone2 create_lchannel: logical_id=0 real_id=9 size=112
======== Running on http://localhost:8080 ========
(Press CTRL+C to quit)
Starting HTTP server on port 8000...
LOADED: PDU DATA
WARNING: on_simulation_step_async() took longer than delta_time_usec: 35.52 ms
```

## 箱庭ARブリッジの起動

Powershellで、hakoniwa-ar-bridgeに移動します。

以下のコマンドを実行して、箱庭ARブリッジを起動します。

```powershell
 python -m asset_lib.main
```

実行例：

```
PS E:\project\temp\hakoniwa-ar-bridge> python -m asset_lib.main
node_dir: asset_lib/config
node: {'bridge_ip': '192.168.2.105', 'web_ip': '192.168.2.105', 'ar_port': 38528, 'nodes': [{'type': 'device', 'path': 'ar1_config.json'}, {'type': 'device', 'path': 'ar2_config.json'}]}
node: {'type': 'device', 'path': 'ar1_config.json'}
config_path: asset_lib/config\ar1_config.json
Config: {'ar_ip': '192.168.2.111', 'server_udp_port': 48528, 'player': {'type': 'dji', 'name': 'Drone1'}, 'avatars': [{'type': 'dji', 'name': 'Drone2'}, {'type': 'head', 'name': 'Player1_head'}, {'type': 'left_hand', 'name': 'Player1_left_hand'}, {'type': 'right_hand', 'name': 'Player1_right_hand'}, {'type': 'head', 'name': 'Player2_head'}, {'type': 'left_hand', 'name': 'Player2_left_hand'}, {'type': 'right_hand', 'name': 'Player2_right_hand'}], 'positioning_speed': {'rotation': 20.0, 'move': 0.2}, 'position': [0.0471993163228035, -0.17764319479465485, -0.0029343680944293737], 'rotation': [0.0, 1.9902009963989258, 0.0]}
node: {'type': 'device', 'path': 'ar2_config.json'}
config_path: asset_lib/config\ar2_config.json
Config: {'ar_ip': '192.168.2.113', 'server_udp_port': 48529, 'player': {'type': 'dji', 'name': 'Drone2'}, 'avatars': [{'type': 'dji', 'name': 'Drone1'}, {'type': 'head', 'name': 'Player2_head'}, {'type': 'left_hand', 'name': 'Player2_left_hand'}, {'type': 'right_hand', 'name': 'Player2_right_hand'}, {'type': 'head', 'name': 'Player1_head'}, {'type': 'left_hand', 'name': 'Player1_left_hand'}, {'type': 'right_hand', 'name': 'Player1_right_hand'}], 'positioning_speed': {'rotation': 20.0, 'move': 0.2}, 'position': [-0.06008348986506462, 0.7474485039710999, -0.9891206622123718], 'rotation': [0.0, 1.6716727018356323, 0.0]}
Starting SyncManager service.
SyncManager service started.
Starting SyncManager service.
Drone1 : Heartbeat timeout: assuming AR device is disconnected.
SyncManager service started.
Hakoniwa AR Bridge started.
Drone2 : Heartbeat timeout: assuming AR device is disconnected.
Drone1 : Heartbeat timeout: assuming AR device is disconnected.
```

## 共有シミュレーションの実行

ShareSimulation内にある drone-simulation.exeをダブルクリックして、共有シミュレーション環境を起動します。

起動したら、STARTボタンを押下してください。

![image](https://github.com/user-attachments/assets/ba699b9a-4dce-4fb5-92c3-7abc90abd1c8)


## QUEST3の起動

QUEST3を起動して、インストールしたapkファイルを起動します。


# QUEST3でのドローン操縦方法

## 位置合わせモード

アプリ起動後は、位置合わせモードになります。

左スティックと右スティックを駆使して、初期位置を調整してください。

決まったら、左スティックの X ボタンを押下すると、ドローン操縦モードに移行します。

ドローン操縦モードから位置合わせモードに移行するには、同様に Y ボタンを押下してください。

## ドローン操縦モード

右スティックの A ボタンを押下すると、ドローンがホバリング状態なります。

あとは、左スティックと右スティックを駆使して、ドローンを操縦してください。

モード２で動きます。



