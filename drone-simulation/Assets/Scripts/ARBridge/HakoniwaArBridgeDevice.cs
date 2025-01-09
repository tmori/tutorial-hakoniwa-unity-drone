using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hakoniwa.ar.bridge
{

    public class HakoniwaArBridgeDevice: IHakoniwaArBridge
    {
        private readonly int heartbeatTimeoutSeconds = 5;
        private DateTime lastHeartbeatTime = DateTime.Now; // 初期値として現在時刻を設定
        private IHakoniwaArBridgePlayer player;
        private UdpComm udp_service;
        private HakoniwaArBridgeStateManager state_manager;
        private bool isStartedWebSocket = false;
        private string serverUri;
        private HeartBeatRequestData latestHeartbeatData = null;
        private static readonly object lockObject = new object(); // スレッドセーフのためのロックオブジェクト

        private static HakoniwaArBridgeDevice instance;
        public static HakoniwaArBridgeDevice Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = new HakoniwaArBridgeDevice();
                    }
                    return instance;
                }
            }
        }

        public HakoniwaArBridgeDevice()
        {
            state_manager = new HakoniwaArBridgeStateManager();
            udp_service = new UdpComm();
        }

        public bool Register(IHakoniwaArBridgePlayer player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (this.player != null)
            {
                Console.WriteLine("A player is already registered.");
                return false;
            }

            this.player = player;
            Console.WriteLine("Player registered successfully.");
            return true;
        }

        private void HeartBeatCheck()
        {
            var packet = udp_service.GetLatestPacket("heartbeat_request");

            if (packet != null && packet.Data != null && packet.Data.ContainsKey("ip_address"))
            {
                // ハートビートを受信したため、タイムスタンプを更新
                lastHeartbeatTime = DateTime.Now;
                latestHeartbeatData = HeartBeatRequest.FromBasePacket(packet);

                var ipAddress = packet.Data["ip_address"] as string;
                serverUri = $"ws://{ipAddress}:8765";
                if (isStartedWebSocket == false)
                {
                    try
                    {
                        udp_service.SetSendPort(latestHeartbeatData.ServerUdpPort);
                        player.StartService(serverUri);
                        isStartedWebSocket = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error starting player service: {ex.Message}");
                    }
                }

                // 現在の状態を文字列として取得し、HeartBeatResponseに設定
                var reply = new HeartBeatResponse(state_manager.GetState().ToString());
                udp_service.SendPacket(reply);
                //Console.WriteLine($"Heartbeat response sent with state: {state_manager.GetState().ToString()} to {serverUri}");
            }
            else
            {
                if (packet != null) {
                    Console.WriteLine("Invalid or missing IP address in heartbeat_request packet.");
                }
            }
            if (latestHeartbeatData != null)
            {
                // タイムアウトチェック: 最後のハートビートから5秒以上経過している場合
                if ((DateTime.Now - lastHeartbeatTime).TotalSeconds > heartbeatTimeoutSeconds)
                {
                    //Console.WriteLine("Heartbeat timeout detected. Switching to POSITIONING state.");
                    ResetEvent();
                }
            }
        }
        void RunPositioning()
        {
            HakoVector3 pos = new HakoVector3();
            HakoVector3 rot = new HakoVector3();
            if (latestHeartbeatData != null)
            {
                pos.X = (float)latestHeartbeatData.SavedPosition.Position["x"];
                pos.Y = (float)latestHeartbeatData.SavedPosition.Position["y"];
                pos.Z = (float)latestHeartbeatData.SavedPosition.Position["z"];

                rot.X = (float)latestHeartbeatData.SavedPosition.Orientation["x"];
                rot.Y = (float)latestHeartbeatData.SavedPosition.Orientation["y"];
                rot.Z = (float)latestHeartbeatData.SavedPosition.Orientation["z"];
            }
            player.UpdatePosition(pos, rot);
            //Console.WriteLine("Position and orientation data have been updated.");
        }

        private void ResetEvent()
        {
            if (state_manager.GetState() == BridgeState.PLAYING) {
                player.ResetPostion();
                player.StopService();
                isStartedWebSocket = false;
            }
            else {
                //nothing to do
            }
            udp_service.ClearBuffers();
            state_manager.EventReset();
        }
        public void Run()
        {
            HeartBeatCheck();
            if (state_manager.GetState() == BridgeState.POSITIONING) {
                RunPositioning();
            }
            player.UpdateAvatars();
        }

        public bool Start()
        {
            if (player == null)
            {
                Console.WriteLine("No player registered. Cannot start service.");
                return false;
            }
            udp_service.Start();
            Console.WriteLine("Hakoniwa AR Bridge service starting...");
            return true;
        }

        public bool Stop()
        {
            if (player == null)
            {
                Console.WriteLine("No player registered. Nothing to stop.");
                return false;
            }

            udp_service.Stop();
            Console.WriteLine("Hakoniwa AR Bridge service stopping...");
            return player.StopService();
        }
        public BridgeState GetState()
        {
            return state_manager.GetState();
        }

        public void DevicePlayStartEvent()
        {
            //TODO send udp packet
            state_manager.EventPlayStart();
        }
        public void DeviceResetEvent()
        {
            //TODO send udp packet
            ResetEvent();
        }
    }
}
