using ShimmerRT.models;
using UnityEngine;

namespace Assets._Scripts.Shimmer
{
    public class ShimmerAdapter
    {
        public static void Move(Transform transform, Shimmer3DModel data)
        {
            if (data != null)
            {
                //xDirection = _myoXDirection;
                //transform.localRotation = new Quaternion(_myoQuaternion.Y, _myoQuaternion.Z, -_myoQuaternion.X, -_myoQuaternion.W);

                // TODO: fix cast to float?
                transform.localRotation = new Quaternion(
                    (float)data.Quaternion_0_CAL,
                    (float)data.Quaternion_1_CAL,
                    -(float)data.Quaternion_2_CAL,
                    -(float)data.Quaternion_3_CAL
                    );

            }

            var accelerometer = new Vector3(
                (float)data.Wide_Range_Accelerometer_Y_CAL,
                (float)data.Wide_Range_Accelerometer_Z_CAL,
                -(float)data.Wide_Range_Accelerometer_X_CAL);

            var gyroscope = new Vector3(
                (float)data.Gyroscope_Y_CAL,
                (float)data.Gyroscope_Z_CAL,
                (float)-data.Gyroscope_X_CAL
                );
        }
    }
}