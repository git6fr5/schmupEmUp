using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class Movement : MonoBehaviour {

    public enum PathType {
        Linear
    }

    public bool save;
    public bool load;
    public string movementName;

    public Vector3 origin;

    public List<Vector3> points;
    public List<PathType> paths;
    public List<Vector3> nodes;

    [System.Serializable]
    public class MovementData {

        public static string path = "Movements/";
        public static string filetype = ".movement";

        public MovementData(Movement movement) {

        }

        public static Movement Read(Movement movement, MovementData data) {
            return movement;
        }

        public static void Save(Movement movement) {

            MovementData data = new MovementData(movement);

            // Concatenate the path.
            string fullPath = GameRules.Path + path + movement.movementName + filetype;

            // Format the data.
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(fullPath, FileMode.Create);
            formatter.Serialize(fileStream, data);

            // Close the file.
            fileStream.Close();

        }

        public static MovementData Load(string filename) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + filename + filetype;
            MovementData data = null;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                data = (MovementData)formatter.Deserialize(fileStream);

                // Close the file.
                fileStream.Close();

            }

            return data;

        }

        public static void Load(Movement movement) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + movement.movementName + filetype;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                MovementData data = (MovementData)formatter.Deserialize(fileStream);

                Read(movement, data);

                // Close the file.
                fileStream.Close();

            }

            return;

        }

    }

    private void Start() {
        origin = transform.position;
        points.Add(transform.position);
    }

    private void Update() {
        if (save) {
            MovementData.Save(this);
            save = false;
        }

        if (load) {
            MovementData.Load(this);
            load = false;
        }
        
        points = new List<Vector3>();
        for (int i = 1; i < nodes.Count; i++) {
            if (paths[i-1] == PathType.Linear) {
                print("Hello");
                AddLinearPath(nodes[i - 1], nodes[i]);
            }

        }

        for (int i = 1; i < points.Count; i++) {
            Debug.DrawLine(points[i-1], points[i], Color.yellow, Time.deltaTime, false);
        }
    }

    private void AddLinearPath(Vector3 start, Vector3 end) {

        int count = (int)Mathf.Floor((end - start).magnitude * GameRules.UnitPrecision);
        print(count);
        Vector3 increment = (end - start) / count;

        for (int i = 0; i < count; i++) {
            points.Add(start + i * increment);
        }

    }

    private void AddSinusoidalPath(Vector3 start, Vector3 end) {

        int count = (int)Mathf.Floor((end - start).magnitude * GameRules.UnitPrecision);
        print(count);
        Vector3 increment = (end - start) / count;

        for (int i = 0; i < count; i++) {
            points.Add(start + i * increment);
        }

    }

}
