using System.Collections.Generic;
using UnityEngine;
using ShimmerInterfaceTest;
using ShimmerRT.models;
using ShimmerAPI;
using UnityEngine.UI;
using Assets._Scripts.Shimmer;
using System;
using ShimmerRT;
using Quaternion = UnityEngine.Quaternion;
using System.Collections;

public class ShimmerFeedManager : MonoBehaviour, IFeedable
{

    #region Fields and Properties
    private string comPort;
    private ShimmerController sc;

    #region UI Elements
    public InputField inputComPort;
    public Button btnConnect;
    public Button btnStream;
    public Button btnStop;
    public Text txtOutput;
    #endregion

    // Myo's current accelerometer reading, representing the acceleration due to force on the Myo armband in units of
    // g (roughly 9.8 m/s^2) and following Unity coordinate system conventions.
    public Vector3 accelerometer;

    // Myo's current gyroscope reading, representing the angular velocity about each of Myo's axes in degrees/second
    // following Unity coordinate system conventions.
    public Vector3 gyroscope;

    // True if and only if this Myo armband has paired successfully, at which point it will provide data and a
    // connection with it will be maintained when possible.
    public bool isPaired
    {
        get { return sc != null && sc.ShimmerDevice.IsConnected(); }
    }

    //private XDirection _myoXDirection = XDirection.Unknown;
    private Thalmic.Myo.Quaternion _myoQuaternion = null;
    private Thalmic.Myo.Vector3 _myoAccelerometer = null;
    private Thalmic.Myo.Vector3 _myoGyroscope = null;

    // the object to which movement and rotation will be applied
    //public GameObject player;
    #endregion

    #region Unity methods
    void Start()
    {
        // COM Port that will be used to connect to Shimmer IMU
        // TODO: allow this to be user selected from UI?

        // Add UI Button click handlers
        btnConnect.onClick.AddListener(Connect);
        btnStream.onClick.AddListener(StartStreaming);
        btnStop.onClick.AddListener(Disconnect);
    }

    //void Update()
    //{
    //    Shimmer3DModel s; // variable to store 'frame' data
    //                      // check that Shimmer is connected
    //    if (sc != null && sc.ShimmerDevice.IsConnected())
    //    {

    //    }
    //    //else
    //    //{     //TODO: Leaving this for now
    //    //    player.transform.SetPositionAndRotation(new Vector3(0, 0, 0), UnityEngine.Quaternion.Euler(0, 0, 0));
    //    //}
    //}

    void UpdateTransform(Shimmer3DModel s)
    {
        if (isPaired)
        {
            //armSynced = _myoArmSynced;
            //arm = _myoArm;
            //xDirection = _myoXDirection;
            //if (_myoQuaternion != null)
            //{
            //transform.localRotation = new Quaternion(_myoQuaternion.Y, _myoQuaternion.Z, -_myoQuaternion.X, -_myoQuaternion.W);
            transform.localRotation = new Quaternion(
                (float)s.Quaternion_1_CAL,
                (float)s.Quaternion_2_CAL,
                -(float)s.Quaternion_0_CAL,
                -(float)s.Quaternion_3_CAL);
            //}
            //if (_myoAccelerometer != null)
            //{
            //accelerometer = new Vector3(_myoAccelerometer.Y, _myoAccelerometer.Z, -_myoAccelerometer.X);
            accelerometer = new Vector3(
                (float)s.Low_Noise_Accelerometer_Y_CAL,
                (float)s.Low_Noise_Accelerometer_Z_CAL,
                -(float)s.Low_Noise_Accelerometer_X_CAL);
            //}
            //if (_myoGyroscope != null)
            //{
            //gyroscope = new Vector3(_myoGyroscope.Y, _myoGyroscope.Z, -_myoGyroscope.X);
            gyroscope = new Vector3(
                (float)s.Gyroscope_Y_CAL,
                (float)s.Gyroscope_Z_CAL,
                -(float)s.Gyroscope_X_CAL);
            //}
            //pose = _myoPose;
            //unlocked = _myoUnlocked;
        }
    }
    #endregion

    #region Get data
    // this method is called for each row of data
    public void UpdateFeed(List<double> data)
    {
        Shimmer3DModel s;
        if (data.Count > 0)
        {
            s = Shimmer3DModel.GetModelFromArray(data.ToArray());
            UpdateTransform(s);
        }
    }
    #endregion

    #region Shimmer device methods
    public void Connect()
    {

        Debug.Log("CONNECT AND STREAM CLICKED");
        this.comPort = inputComPort.text;
        Debug.Log("USING " + this.comPort);

        txtOutput.text = "Connecting...";
        sc = new ShimmerController(this);
        txtOutput.text += "\nTrying to connect on " + this.comPort;
        sc.Connect(this.comPort);

        ////// wait until connection
        ////// TODO: fix this - will cause hang if cannot connect to Shimmer
        //do
        //{
        //    //System.Threading.Thread.Sleep(100);
        //} while (!sc.ShimmerDevice.IsConnected());

        //print("CONNNNNECCCETTTTECETTEEEEEDDDD!!!!");

    }

    public void StartStreaming()
    {
        if (!isPaired)
        {
            Debug.Log("NOT PAIRED!");
            return;
        }

        Debug.Log("PAIRED!");
        sc.ShimmerDevice.WriteBaudRate(230000);

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