using ShimmerRT.models;
using UnityEngine;

namespace Assets._Scripts.Shimmer
{
    public class ShimmerAdapter
    {
        public static void Move(Transform transform, Shimmer3DModel model)
        {
            if (model != null)
            {
                // for myo this was: y, z, -x, -w
                transform.localRotation = new Quaternion(
                    (float)model.Quaternion_1_CAL,   // y
                    (float)model.Quaternion_2_CAL,   // z
                    -(float)model.Quaternion_0_CAL,   // -x
                    -(float)model.Quaternion_3_CAL  // -w
                    );
            }
            if (model != null)
            {
                // for myo this was: y, z, -x
                accelerometer = new Vector3(
                    (float)model.Low_Noise_Accelerometer_Y_CAL,    // y
                    (float)model.Low_Noise_Accelerometer_Z_CAL,    // z
                    -(float)model.Low_Noise_Accelerometer_X_CAL    // -x
                    );
            }


            if (model != null)
            {
                // for myo this was: y, z, -x
                gyroscope = new Vector3(
                    (float)model.Gyroscope_Y_CAL,    // y
                    (float)model.Gyroscope_Z_CAL,    //z
                    -(float)model.Gyroscope_X_CAL    // -x
                    );
            }
        }

        //public static void Move(Transform transform, Shimmer3DModel data)
        //{
        //    if (data != null)
        //    {
        //        //xDirection = _myoXDirection;
        //        //transform.localRotation = new Quaternion(_myoQuaternion.Y, _myoQuaternion.Z, -_myoQuaternion.X, -_myoQuaternion.W);

        //        // TODO: fix cast to float?
        //        transform.localRotation = new Quaternion(
        //            (float)data.Quaternion_0_CAL,
        //            (float)data.Quaternion_1_CAL,
        //            -(float)data.Quaternion_2_CAL,
        //            -(float)data.Quaternion_3_CAL
        //            );

        //    }

        //    var accelerometer = new Vector3(
        //        (float)data.Wide_Range_Accelerometer_Y_CAL,
        //        (float)data.Wide_Range_Accelerometer_Z_CAL,
        //        -(float)data.Wide_Range_Accelerometer_X_CAL);

        //    var gyroscope = new Vector3(
        //        (float)data.Gyroscope_Y_CAL,
        //        (float)data.Gyroscope_Z_CAL,
        //        (float)data.Gyroscope_X_CAL
        //        );
        //}
    }
}