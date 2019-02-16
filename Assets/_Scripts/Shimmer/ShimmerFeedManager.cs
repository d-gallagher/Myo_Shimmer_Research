using System.Collections.Generic;
using UnityEngine;
using ShimmerInterfaceTest;
using ShimmerRT.models;
using ShimmerAPI;
using UnityEngine.UI;

namespace Assets._Scripts.Shimmer
{

    public class ShimmerFeedManager : MonoBehaviour, IFeedable
    {
        #region Fields and Properties
        private string comPort;
        private ShimmerController sc;
        //public Stack<Shimmer3DModel> shimmerModels;
        private Queue<Shimmer3DModel> _queue;

        #region UI Elements
        public Button btnStart;
        public Button btnStop;
        public Text txtOutput;
        #endregion

        public GameObject player;
        #endregion

        #region Unity methods
        void Start()
        {
            // COM Port that will be used to connect to Shimmer IMU
            // TODO: allow this to be user selected from UI?
            this.comPort = "COM4";
            // initialise data structure
            this._queue = new Queue<Shimmer3DModel>();
            // Add UI Button click handlers
            btnStart.onClick.AddListener(ConnectAndStream);
            btnStop.onClick.AddListener(Disconnect);
        }

        void Update()
        {
            Shimmer3DModel s; // variable to store 'frame' data
                              // check that Shimmer is connected
            if (sc != null && sc.ShimmerDevice.IsConnected())
            {
                // only get data if it exists
                if (_queue.Count > 0)
                {
                    s = _queue.Dequeue(); // get the next item in the queue
                    ShimmerAdapter.Move(player.transform, s);
                    Moving(s); // translate the GameObject
                    Rotating(s); // rotate the GameObject
                }
            }
            //else
            //{     //TODO: Leaving this for now
            //    player.transform.SetPositionAndRotation(new Vector3(0, 0, 0), UnityEngine.Quaternion.Euler(0, 0, 0));
            //}
        }
        #endregion

        #region GameObject manipulation
        /// <summary>
        /// Move the GameObject transform by values in supplied model
        /// 
        /// TODO: get and apply movement to transform
        /// </summary>
        /// <param name="shimmerModel">data model</param>
        public void Moving(Shimmer3DModel shimmerModel) { }

        /// <summary>
        /// Rotate the GameObject transform by values in supplied model
        /// 
        /// TODO: get and apply rotation to GameObject
        /// </summary>
        /// <param name="shimmerModel"></param>
        public void Rotating(Shimmer3DModel shimmerModel) { }
        #endregion

        #region Get data
        // this method is called for each row of data
        public void UpdateFeed(List<double> data)
        {
            Shimmer3DModel s = null;
            if (data.Count > 0)
            {
                s = Shimmer3DModel.GetModelFromArray(data.ToArray());
                if (s != null) { _queue.Enqueue(s); }
            }
        }
        #endregion

        #region Shimmer device methods
        public void ConnectAndStream()
        {
            txtOutput.text = "Connecting...";
            sc = new ShimmerController(this);
            txtOutput.text += "\nTrying to connect on " + this.comPort;
            sc.Connect(comPort);

            // wait until connection
            // TODO: fix this - will cause hang if cannot connect to Shimmer
            do
            {
                System.Threading.Thread.Sleep(100);
            } while (!sc.ShimmerDevice.IsConnected());

            txtOutput.text += "\nConnected";
            // set options, start streaming
            sc.ShimmerDevice.Set3DOrientation(true);
            txtOutput.text += "\nStarting stream...";
            sc.StartStream();
        }

        public void Disconnect()
        {
            print("Stopping stream...");
            txtOutput.text += "\nStopping stream";
            sc.StopStream(); // stop the stream
            sc.ShimmerDevice.Disconnect(); // disconnect from the Shimmer
            sc = null; // set controller to null

            txtOutput.text += "\nStream Stopped";
            txtOutput.text += "\nDisconnected";
        }
        #endregion
    }
}