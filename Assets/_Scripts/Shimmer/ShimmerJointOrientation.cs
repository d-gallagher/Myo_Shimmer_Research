﻿using UnityEngine;

using LockingPolicy = Thalmic.Myo.LockingPolicy;
using UnlockType = Thalmic.Myo.UnlockType;
using ShimmerRT.models;
using UnityEngine.UI;
using System.Collections.Generic;

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

    void Start()
    {
        // get the script from the ShimmerDevice object
        shimmerFeed = shimmerDevice.GetComponent<ShimmerFeedManager>();
        ResetTransform();
        btnSnapshot.onClick.AddListener(PrintSnaps);
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

        float dX = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_X_CAL - s.Low_Noise_Accelerometer_X_CAL));
        float dY = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_Y_CAL - s.Low_Noise_Accelerometer_Y_CAL));
        float dZ = Mathf.Abs((float)(lastShimmerModel.Low_Noise_Accelerometer_Z_CAL - s.Low_Noise_Accelerometer_Z_CAL));

        //if ((x != nx && abs(x - nx) > threshold) || (y != ny && abs(y - ny) > threshold) || (z != nz && abs(z - nz) > threshold))
        //{
        //    onAwake(x, y, z);
        //}

        if (dX > isMovingThreshold || dY > isMovingThreshold || dZ > isMovingThreshold)
        {
            Vector3 accel = new Vector3(dX, dY, dZ);
            Vector3 gyro = new Vector3((float)s.Gyroscope_X_CAL, (float)s.Gyroscope_Y_CAL, (float)s.Gyroscope_Z_CAL);
            IsMoving(accel, gyro);
            return true;
        }
     

        return false;
    }

    //Add snapshots of model accel and rotation to list
    private void IsMoving(Vector3 accel, Vector3 gyro)
    {
        //add string key then value
        snapshots.Add("Ac: " + Time.time, accel);
        snapshots.Add("Gy: " + Time.time, gyro);
    }

    private void PrintSnaps()
    {
        Debug.Log("Snapshot Dict Size: "+snapshots.Count);
        foreach (KeyValuePair<string, Vector3> kvp in snapshots)
        {
            Debug.Log(string.Format("PrintSnap: Key = {0}, Value = {1}", kvp.Key, kvp.Value));
        }

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
