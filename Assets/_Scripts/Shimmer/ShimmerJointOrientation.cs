using UnityEngine;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using UnlockType = Thalmic.Myo.UnlockType;
using ShimmerRT.models;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Linq;

// Orient the object to match that of the Myo armband.
// Compensate for initial yaw (orientation about the gravity vector) and roll (orientation about
// the wearer's arm) by allowing the user to set a reference orientation.
// Making the fingers spread pose or pressing the 'r' key resets the reference orientation.
public class ShimmerJointOrientation : MonoBehaviour
{
    // Myo game object to connect with.
    // This object must have a ThalmicMyo script attached.
    public GameObject shimmerDevice = null;

    // A rotation that compensates for the Myo armband's orientation parallel to the ground, i.e. yaw.
    // Once set, the direction the Myo armband is facing becomes "forward" within the program.
    // Set by making the fingers spread pose or pressing "r".
    private Quaternion _antiYaw = Quaternion.identity;

    // A reference angle representing how the armband is rotated about the wearer's arm, i.e. roll.
    // Set by making the fingers spread pose or pressing "r".
    private float _referenceRoll = 0.0f;
    private ShimmerFeedManager shimmerFeed;
    private Vector3 accelerometer;
    private Vector3 gyroscope;

    private Shimmer3DModel lastShimmerModel = null;

    public Text txtImpact;

    public float impactThreshold = 1.0f;
    public float isMovingThreshold = 1.0f;
    public Button btnSnapshot;
    //name value for accel + gyro from running rugby guy
    Dictionary<string, Vector3> snapshots = new Dictionary<string, Vector3>();
    List<string> playback = new List<string>();
    List<Shimmer3DModel> loadFile = new List<Shimmer3DModel>();

    void Start()
    {
        // get the script from the ShimmerDevice object
        shimmerFeed = shimmerDevice.GetComponent<ShimmerFeedManager>();
        ResetTransform();
        btnSnapshot.onClick.AddListener(SaveFile);
    }

    private void Update()
    {
        // if data is available, use it
        if (shimmerFeed.Queue.Count > 0)
        {
            var s = shimmerFeed.Queue.Dequeue();
            // see if there was an 'impact' between this data and the last received data
            if (lastShimmerModel != null)
            {
                Debug.Log("Checking Impact");
                if (CheckImpact(s))
                {
                    txtImpact.text = "--IMPACT--\n" + Time.time;
                }

                if (CheckMoving(s))
                {
                    txtImpact.text = "--IsMoving--\n" + "LN Acc X: " + accelerometer.x + "\nLN Acc Y: " + accelerometer.y + "\nLN Acc Z: " + accelerometer.z;

                }
            }
            UpdateTransform(s);
            lastShimmerModel = s;
        }
    }


    #region == ImpactCheck ==

    private bool CheckImpact(Shimmer3DModel s)
    {

        float dX = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_X_CAL - s.Low_Noise_Accelerometer_X_CAL));
        float dY = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_Y_CAL - s.Low_Noise_Accelerometer_Y_CAL));
        float dZ = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_Z_CAL - s.Low_Noise_Accelerometer_Z_CAL));

        Debug.Log(dX);
        Debug.Log(dY);
        Debug.Log(dZ);

        if (dX > impactThreshold)
        {
            return true;
        }
        if (dY > impactThreshold)
        {
            return true;
        }
        if (dZ > impactThreshold)
        {
            return true;
        }
        return false;
    }
    #endregion

    #region == Movement Snapshots ==
    private bool CheckMoving(Shimmer3DModel s)
    {
        //if movement in any direction is above a set threshold, capture data and add to list
        float dX = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_X_CAL - s.Low_Noise_Accelerometer_X_CAL));
        float dY = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_Y_CAL - s.Low_Noise_Accelerometer_Y_CAL));
        float dZ = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_Z_CAL - s.Low_Noise_Accelerometer_Z_CAL));

        if (dX > isMovingThreshold || dY > isMovingThreshold || dZ > isMovingThreshold)
        {
            Vector3 accel = new Vector3(dX, dY, dZ);
            Vector3 gyro = new Vector3((float)s.Gyroscope_X_CAL, (float)s.Gyroscope_Y_CAL, (float)s.Gyroscope_Z_CAL);
            IsMoving(s);
            return true;
        }

        return false;
    }

    //Add snapshots of model accel and rotation to list
    private void IsMoving(Shimmer3DModel s)
    {
        ////add string key then value
        //snapshots.Add("Ac: " + Time.time, accel);
        //snapshots.Add("Gy: " + Time.time, gyro);
         playback.Add(BuildRowFromModel(s));
    }

    #region == BuildRows ==
    public static string BuildRowFromModel(Shimmer3DModel s)
    {
        StringBuilder sb = new StringBuilder();
        
        sb.Append(s.Timestamp_RAW);
        sb.Append("," + s.Timestamp_CAL);

        sb.Append("," + s.Low_Noise_Accelerometer_X_RAW);
        sb.Append("," + s.Low_Noise_Accelerometer_X_CAL);
        sb.Append("," + s.Low_Noise_Accelerometer_Y_RAW);
        sb.Append("," + s.Low_Noise_Accelerometer_Y_CAL);
        sb.Append("," + s.Low_Noise_Accelerometer_Z_RAW);
        sb.Append("," + s.Low_Noise_Accelerometer_Z_CAL);

        sb.Append("," + s.Wide_Range_Accelerometer_X_RAW);
        sb.Append("," + s.Wide_Range_Accelerometer_X_CAL);
        sb.Append("," + s.Wide_Range_Accelerometer_Y_RAW);
        sb.Append("," + s.Wide_Range_Accelerometer_Y_CAL);
        sb.Append("," + s.Wide_Range_Accelerometer_Z_RAW);
        sb.Append("," + s.Wide_Range_Accelerometer_Z_CAL);

        sb.Append("," + s.Gyroscope_X_RAW);
        sb.Append("," + s.Gyroscope_X_CAL);
        sb.Append("," + s.Gyroscope_Y_RAW);
        sb.Append("," + s.Gyroscope_Y_CAL);
        sb.Append("," + s.Gyroscope_Z_RAW);
        sb.Append("," + s.Gyroscope_Z_CAL);

        sb.Append("," + s.Magnetometer_X_RAW);
        sb.Append("," + s.Magnetometer_X_CAL);
        sb.Append("," + s.Magnetometer_Y_RAW);
        sb.Append("," + s.Magnetometer_Y_CAL);
        sb.Append("," + s.Magnetometer_Z_RAW);
        sb.Append("," + s.Magnetometer_Z_CAL);


        sb.Append("," + s.Pressure_RAW);
        sb.Append("," + s.Pressure_CAL);
        sb.Append("," + s.Temperature_RAW);
        sb.Append("," + s.Temperature_CAL);

        sb.Append("," + s.Axis_Angle_A_CAL);
        sb.Append("," + s.Axis_Angle_X_CAL);
        sb.Append("," + s.Axis_Angle_Y_CAL);
        sb.Append("," + s.Axis_Angle_Z_CAL);

        sb.Append("," + s.Quaternion_0_CAL);
        sb.Append("," + s.Quaternion_1_CAL);
        sb.Append("," + s.Quaternion_2_CAL);
        sb.Append("," + s.Quaternion_3_CAL);

        return sb.ToString();
    
    }
    #endregion
    #endregion

    #region == Save to File ==
    private void SaveFile()
    {

        if (playback == null)
        {
            EditorUtility.DisplayDialog(
                "Select File",
                "Select Location first!",
                "Ok");
            return;
        }

        var path = EditorUtility.SaveFilePanel(
            "Save File",
            "",
            "TestSave" + " " + DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss") + ".csv",
            "csv");

        if (path.Length != 0)
        {
                        
            if (playback != null)
            {
                //File.WriteAllBytes(path, pngData);
                File.WriteAllLines(path, playback.ToArray());
                Debug.Log("File Saved as: " + path);
            }
        }
    }
    
    public static Shimmer3DModel FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        Shimmer3DModel loadedModel = new Shimmer3DModel(
            Convert.ToDouble(values[0]), 
            Convert.ToDouble(values[1]), 
            Convert.ToDouble(values[2]), 
            Convert.ToDouble(values[3]),
            Convert.ToDouble(values[4]),
            Convert.ToDouble(values[5]),
            Convert.ToDouble(values[6]),
            Convert.ToDouble(values[7]),
            Convert.ToDouble(values[8]), 
            Convert.ToDouble(values[9]),
            Convert.ToDouble(values[10]),
            Convert.ToDouble(values[11]),
            Convert.ToDouble(values[12]),
            Convert.ToDouble(values[13]),
            Convert.ToDouble(values[14]),
            Convert.ToDouble(values[15]),
            Convert.ToDouble(values[16]),
            Convert.ToDouble(values[17]),
            Convert.ToDouble(values[18]),
            Convert.ToDouble(values[19]),
            Convert.ToDouble(values[20]),
            Convert.ToDouble(values[21]),
            Convert.ToDouble(values[22]),
            Convert.ToDouble(values[23]),
            Convert.ToDouble(values[24]),
            Convert.ToDouble(values[25]),
            Convert.ToDouble(values[26]),
            Convert.ToDouble(values[27]),
            Convert.ToDouble(values[28]),
            Convert.ToDouble(values[29]),
            Convert.ToDouble(values[30]),
            Convert.ToDouble(values[31]),
            Convert.ToDouble(values[32]),
            Convert.ToDouble(values[33]),
            Convert.ToDouble(values[34]),
            Convert.ToDouble(values[35]),
            Convert.ToDouble(values[36]),
            Convert.ToDouble(values[37]));
       
        return loadedModel;
    }
    //build list from file path.. posibly better to load direct from file w/streams?
    private void ReadFile(string path)
    {
        /*
        The File.ReadAllLines reads all lines from the CSV file into a string array.
        The .Select(v => FromCsv(v)) uses Linq to build new shimmer model instead of for each
         */
        loadFile = File.ReadAllLines(path).Select(row => FromCsv(row))
                                           .ToList();
    }

    

    #endregion

    void ResetTransform()
    {
        // Current zero roll vector and roll value.
        Vector3 zeroRoll = computeZeroRollVector(shimmerDevice.transform.forward);
        float roll = rollFromZero(zeroRoll, shimmerDevice.transform.forward, shimmerDevice.transform.up);

        // The relative roll is simply how much the current roll has changed relative to the reference roll.
        // adjustAngle simply keeps the resultant value within -180 to 180 degrees.
        float relativeRoll = normalizeAngle(roll - _referenceRoll);

        // antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
        Quaternion antiRoll = Quaternion.AngleAxis(relativeRoll, shimmerDevice.transform.forward);

        // Here the anti-roll and yaw rotations are applied to the myo Armband's forward direction to yield
        // the orientation of the joint.
        transform.rotation = _antiYaw * antiRoll * Quaternion.LookRotation(shimmerDevice.transform.forward);

        // Mirror the rotation around the XZ plane in Unity's coordinate system (XY plane in Myo's coordinate
        // system). This makes the rotation reflect the arm's orientation, rather than that of the Myo armband.
        transform.rotation = new Quaternion(transform.localRotation.x,
                                            -transform.localRotation.y,
                                            transform.localRotation.z,
                                            -transform.localRotation.w);
    }

    private void UpdateTransform(Shimmer3DModel s)
    {

        transform.localRotation = new Quaternion(
            -(float)s.Quaternion_2_CAL,
            -(float)s.Quaternion_0_CAL,
            (float)s.Quaternion_1_CAL,
            -(float)s.Quaternion_3_CAL);


        accelerometer = new Vector3(
            (float)s.Low_Noise_Accelerometer_X_CAL,
            (float)s.Low_Noise_Accelerometer_Y_CAL,
            (float)s.Low_Noise_Accelerometer_Z_CAL);
        //Debug.Log("LN Acc X: " + accelerometer.x + "LN Acc Y: " + accelerometer.y + "LN Acc Z: " + accelerometer.z);

        gyroscope = new Vector3(
            (float)s.Gyroscope_Y_CAL,
            (float)s.Gyroscope_Z_CAL,
            -(float)s.Gyroscope_X_CAL);
    }

    #region == Myo Code ==
    // Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
    // As the armband is rotated about the forward axis this value will change, regardless of which way the
    // forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
    float rollFromZero(Vector3 zeroRoll, Vector3 forward, Vector3 up)
    {
        // The cosine of the angle between the up vector and the zero roll vector. Since both are
        // orthogonal to the forward vector, this tells us how far the Myo has been turned around the
        // forward axis relative to the zero roll vector, but we need to determine separately whether the
        // Myo has been rolled clockwise or counterclockwise.
        float cosine = Vector3.Dot(up, zeroRoll);

        // To determine the sign of the roll, we take the cross product of the up vector and the zero
        // roll vector. This cross product will either be the same or opposite direction as the forward
        // vector depending on whether up is clockwise or counter-clockwise from zero roll.
        // Thus the sign of the dot product of forward and it yields the sign of our roll value.
        Vector3 cp = Vector3.Cross(up, zeroRoll);
        float directionCosine = Vector3.Dot(forward, cp);
        float sign = directionCosine < 0.0f ? 1.0f : -1.0f;

        // Return the angle of roll (in degrees) from the cosine and the sign.
        return sign * Mathf.Rad2Deg * Mathf.Acos(cosine);
    }

    // Compute a vector that points perpendicular to the forward direction,
    // minimizing angular distance from world up (positive Y axis).
    // This represents the direction of no rotation about its forward axis.
    Vector3 computeZeroRollVector(Vector3 forward)
    {
        Vector3 antigravity = Vector3.up;
        Vector3 m = Vector3.Cross(shimmerDevice.transform.forward, antigravity);
        Vector3 roll = Vector3.Cross(m, shimmerDevice.transform.forward);

        return roll.normalized;
    }

    // Adjust the provided angle to be within a -180 to 180.
    float normalizeAngle(float angle)
    {
        if (angle > 180.0f)
        {
            return angle - 360.0f;
        }
        if (angle < -180.0f)
        {
            return angle + 360.0f;
        }
        return angle;
    }

    // Extend the unlock if ThalmcHub's locking policy is standard, and notifies the given myo that a user action was
    // recognized.
    void ExtendUnlockAndNotifyUserAction(ThalmicMyo myo)
    {
        ThalmicHub hub = ThalmicHub.instance;

        if (hub.lockingPolicy == LockingPolicy.Standard)
        {
            myo.Unlock(UnlockType.Timed);
        }

        myo.NotifyUserAction();
    }
    #endregion
}
