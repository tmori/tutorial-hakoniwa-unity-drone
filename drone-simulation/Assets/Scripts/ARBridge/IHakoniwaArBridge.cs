using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hakoniwa.ar.bridge
{
    public struct HakoVector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public HakoVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
    public interface IHakoniwaArBridgePlayer
    {
        Task<bool> StartService(string serverUri);
        bool StopService();
        /*
         * position, rotationは、LocalとDeviceとで意味が変わる
         * Local  : 最新のベース位置情報
         * Device : always zeros
         */
        void UpdatePosition(HakoVector3 position, HakoVector3 rotation);
        /*
         * Device側で保持している最新のベース位置情報を取得する
         */
        void GetBasePosition(out HakoVector3 position, out HakoVector3 rotation);

        /*
         * Deviceの場合のみ、コールバックされる
         */
        void SetBasePosition(HakoVector3 position, HakoVector3 rotation);
        void setPositioningSpeed(float rotation, float move);
    }
    public enum BridgeState
    {
        POSITIONING,
        PLAYING
    }
    public interface IHakoniwaArBridge
    {
        bool Register(IHakoniwaArBridgePlayer player);
        bool Start();
        void Run();
        BridgeState GetState();
        bool Stop();

        /*
         * Device向けの Event API
         */
         void DevicePlayStartEvent();
         void DeviceResetEvent();
    }
}
