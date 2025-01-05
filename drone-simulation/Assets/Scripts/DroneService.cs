using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Hakoniwa.DroneService
{
    public static class DroneServiceRC
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        private const string DllName = "hako_service_c"; // Windows
#else
        private const string DllName = "libhako_service_c"; // Ubuntu, Mac
#endif

        /*
         * Initialization and Control
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_init(string drone_config_dir_path);

        public static int Init(string droneConfigDirPath)
        {
            try
            {
                return drone_service_rc_init(droneConfigDirPath);
            }
            catch (DllNotFoundException e)
            {
                Debug.LogError($"DllNotFoundException: {e.Message}");
                return -1;
            }
            catch (EntryPointNotFoundException e)
            {
                Debug.LogError($"EntryPointNotFoundException: {e.Message}");
                return -1;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Exception: {e.Message}");
                return -1;
            }
        }


        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_start();

        public static int Start()
        {
            return drone_service_rc_start();
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_run();

        public static int Run()
        {
            return drone_service_rc_run();
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_stop();

        public static int Stop()
        {
            return drone_service_rc_stop();
        }

        /*
         * Stick Operations
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_vertical(int index, double value);

        public static int PutVertical(int index, double value)
        {
            return drone_service_rc_put_vertical(index, value);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_horizontal(int index, double value);

        public static int PutHorizontal(int index, double value)
        {
            return drone_service_rc_put_horizontal(index, value);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_heading(int index, double value);

        public static int PutHeading(int index, double value)
        {
            return drone_service_rc_put_heading(index, value);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_forward(int index, double value);

        public static int PutForward(int index, double value)
        {
            return drone_service_rc_put_forward(index, value);
        }

        /*
         * Button Operations
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_radio_control_button(int index, int value);

        public static int PutRadioControlButton(int index, int value)
        {
            return drone_service_rc_put_radio_control_button(index, value);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_magnet_control_button(int index, int value);

        public static int PutMagnetControlButton(int index, int value)
        {
            return drone_service_rc_put_magnet_control_button(index, value);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_camera_control_button(int index, int value);

        public static int PutCameraControlButton(int index, int value)
        {
            return drone_service_rc_put_camera_control_button(index, value);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_put_home_control_button(int index, int value);

        public static int PutHomeControlButton(int index, int value)
        {
            return drone_service_rc_put_home_control_button(index, value);
        }

        /*
         * Get Position and Attitude
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_get_position(int index, out double x, out double y, out double z);

        public static int GetPosition(int index, out double x, out double y, out double z)
        {
            return drone_service_rc_get_position(index, out x, out y, out z);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_get_attitude(int index, out double x, out double y, out double z);

        public static int GetAttitude(int index, out double x, out double y, out double z)
        {
            return drone_service_rc_get_attitude(index, out x, out y, out z);
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int drone_service_rc_get_controls(int index, out double c1, out double c2, out double c3, out double c4, out double c5, out double c6, out double c7, out double c8);

        public static int GetControls(int index, out double c1, out double c2, out double c3, out double c4, out double c5, out double c6, out double c7, out double c8)
        {
            return drone_service_rc_get_controls(index, out c1, out c2, out c3, out c4, out c5, out c6, out c7, out c8);
        }

        /*
         * Miscellaneous
         */
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern ulong drone_service_rc_get_time_usec(int index);

        public static ulong GetTimeUsec(int index)
        {
            return drone_service_rc_get_time_usec(index);
        }
    }
}
