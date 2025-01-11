using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using hakoniwa.sim.core;

namespace hakoniwa.gui
{
    public class SimTime : MonoBehaviour
    {
        public GameObject myObject;
        Text simTimeText;
        // Start is called before the first frame update
        void Start()
        {
            simTimeText = myObject.GetComponent<Text>();
        }

        // Update is called once per frame
        void Update()
        {
            long simtime = HakoAsset.GetHakoControl().GetWorldTime();
            double t = ((double)simtime) / 1000000.0f;
            if (simtime <= 1)
            {
                simTimeText.text = "0.000";
            }
            else
            {
                long tl = (long)(t * 1000);
                t = (double)tl / 1000;
                simTimeText.text = t.ToString();
            }
        }
    }
}
