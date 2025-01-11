using hakoniwa.sim;
using hakoniwa.sim.core;
using UnityEngine;
using UnityEngine.UI;

namespace hakoniwa.gui
{
    public class SimStart : MonoBehaviour
    {
        private Text myText;
        private Button myButton;

        private enum SimCommandStatus
        {
            Start,
            Stop,
            Reset,
            Resetting
        }

        private SimCommandStatus cmdStatus = SimCommandStatus.Start;

        private float resetStartTime = 0f; // リセット開始時間
        private float resetDuration = 1f; // リセットの表示時間（秒）

        void Start()
        {
            var obj = GameObject.Find("StartButton");
            myButton = obj.GetComponentInChildren<Button>();
            myText = obj.GetComponentInChildren<Text>();
            myButton.interactable = true;
            myText.text = "START";
        }

        void Update()
        {
            var simulator = HakoAsset.GetHakoControl();
            var state = simulator.GetState();

            // Disable button if the simulator is in an invalid state
            if (state != HakoSimState.Running && state != HakoSimState.Stopped)
            {
                myButton.interactable = false;
                return;
            }

            if (cmdStatus == SimCommandStatus.Resetting)
            {
                // リセットの経過時間をチェック
                if (Time.time - resetStartTime >= resetDuration)
                {
                    FinishReset(simulator);
                }
                return; // Resetting 中は他の状態遷移を防ぐ
            }

            // 通常状態でボタンを有効化
            myButton.interactable = true;

            // Update button text and state based on simulator state and command status
            switch (cmdStatus)
            {
                case SimCommandStatus.Start:
                    if (state == HakoSimState.Running)
                    {
                        myText.text = "STOP";
                        cmdStatus = SimCommandStatus.Stop;
                    }
                    break;

                case SimCommandStatus.Stop:
                    if (state == HakoSimState.Stopped)
                    {
                        myText.text = "RESET";
                        cmdStatus = SimCommandStatus.Reset;
                    }
                    break;
            }
        }

        public void OnButtonClick()
        {
            Debug.Log("Button clicked");
            var simulator = HakoAsset.GetHakoControl();

            switch (cmdStatus)
            {
                case SimCommandStatus.Start:
                    simulator.SimulationStart();
                    myButton.interactable = false;
                    break;

                case SimCommandStatus.Stop:
                    simulator.SimulationStop();
                    myButton.interactable = false;
                    break;

                case SimCommandStatus.Reset:
                    StartReset(simulator);
                    break;
            }
        }

        private void StartReset(IHakoControl simulator)
        {
            myText.text = "RESETTING"; // 表示をリセット中に変更
            cmdStatus = SimCommandStatus.Resetting;
            resetStartTime = Time.time; // リセット開始時間を記録
            myButton.interactable = false;
            simulator.SimulationReset(); // リセットを実行
        }

        private void FinishReset(IHakoControl simulator)
        {
            if (simulator.GetState() == HakoSimState.Stopped)
            {
                myText.text = "START"; // 表示を開始に戻す
                cmdStatus = SimCommandStatus.Start;
                myButton.interactable = true;
            }
        }
    }
}
