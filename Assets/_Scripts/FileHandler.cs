using ShimmerRT.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets._Scripts
{
    class FileHandler
    {

        

        public static string BuildRowFromModel(Shimmer3DModel s)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(s.Timestamp_RAW); // convert this to a readable date
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
        
        public static void SaveModelToFile(string shimmerID, List<Shimmer3DModel> recordList)
        {
            List<string> fromPlayback = new List<string>();

            foreach (var item in recordList)
            {
                fromPlayback.Add(BuildRowFromModel(item));
            }       

            if (recordList == null)
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
                shimmerID + " " + DateTime.Now.ToString("dd-MM-yyyy HH-mm-ss") + ".csv",
                "csv");

            if (path.Length != 0)
            {

                if (recordList != null)
                {
                    //File.WriteAllBytes(path, pngData);
                    File.WriteAllLines(path, fromPlayback.ToArray());
                    Debug.Log("File Saved as: " + path);
                }
            }
        }

        public static Shimmer3DModel BuildModelFromRow(string csvLine)
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
        private static List<Shimmer3DModel> LoadModelFromFile(string path)
        {
            /*  
            The File.ReadAllLines reads all lines from the CSV file into a string array.
            The .Select(v => FromCsv(v)) uses Linq to build new shimmer model instead of for each
             */
            var loadFile = File.ReadAllLines(path).Select(row => BuildModelFromRow(row))
                                                .ToList();

            return loadFile;
        }

    }
}
