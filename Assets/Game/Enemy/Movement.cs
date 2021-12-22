using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class Movement : MonoBehaviour {

    public bool save;
    public bool load;
    public string movementName;

    public bool reset;
    public Vector3 origin;
    public float duration;
    public float ticks = 0f;

    public float horizontalSpeed;
    public float sinHorizontalSpeed;
    public float horizontalPeriod;
    [Range(0f, 1f)] public float horizontalPeriodOffset;
    public float horizontalAcceleration;

    public float verticalSpeed;
    public float sinVerticalSpeed;
    public float verticalPeriod;
    [Range(0f, 1f)] public float verticalPeriodOffset;
    public float verticalAcceleration;

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
        
        if (reset) {
            transform.position = origin;
            ticks = 0f;
            reset = false;
        }

        Move();
    }

    // Movement mechanics
    private void Move() {
        ticks += Time.deltaTime;
        if (ticks > duration) {
            reset = true;
        }

        Vector3 acceleration = new Vector3(horizontalAcceleration * ticks, verticalAcceleration * ticks, 0f);
        
        float horizontal = horizontalSpeed;
        if (horizontalPeriod != 0) {
            horizontal += sinHorizontalSpeed * Mathf.Sin(2 * Mathf.PI * (ticks / horizontalPeriod  + horizontalPeriodOffset) );
        }

        float vertical = verticalSpeed;
        if (verticalPeriod != 0) {
            vertical += sinVerticalSpeed * Mathf.Sin(2 * Mathf.PI * (ticks / verticalPeriod + verticalPeriodOffset) );
        }
            

        Vector3 velocity = new Vector3(horizontal, vertical, 0f) + acceleration;
        transform.position += velocity * Time.deltaTime;
    }

}
