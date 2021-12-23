using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class Movement : MonoBehaviour {

    [Space(5), Header("Movement")]
    public string movementName;
    public bool save;
    public bool load;
    public bool reset;
    public Vector3 origin;
    [Range(0.05f, 20f)] public float duration;
    public float ticks = 0f;
    public bool reverse;
    public bool flip;

    [Space(5), Header("Linear")]
    public float horizontalSpeed;
    public float verticalSpeed;

    [Space(5), Header("Sinusoidal")]
    public bool circle;
    public float horizontalAmplitude;
    public float verticalAmplitude;
    public float horizontalPeriod;
    public float verticalPeriod;
    [Range(0f, 1f)] public float horizontalPeriodOffset;
    [Range(0f, 1f)] public float verticalPeriodOffset;

    [Space(5), Header("Acceleration")]
    public float horizontalAcceleration;
    public float verticalAcceleration;

    [System.Serializable]
    public class MovementData {

        public static string path = "Movements/";
        public static string filetype = ".movement";

        public string movementName;
        public float duration;
        public float horizontalSpeed;
        public float verticalSpeed;
        public float horizontalAmplitude;
        public float verticalAmplitude;
        public float horizontalPeriod;
        public float verticalPeriod;
        public float horizontalPeriodOffset;
        public float verticalPeriodOffset;
        public float horizontalAcceleration;
        public float verticalAcceleration;

        public MovementData(Movement movement) {
            this.movementName = movement.movementName;
            this.duration = movement.duration;
            this.horizontalSpeed = movement.horizontalSpeed;
            this.verticalSpeed = movement.verticalSpeed;
            this.horizontalAmplitude = movement.horizontalAmplitude;
            this.verticalAmplitude = movement.verticalAmplitude;
            this.horizontalPeriod = movement.horizontalPeriod;
            this.verticalPeriod = movement.verticalPeriod;
            this.horizontalPeriodOffset = movement.horizontalPeriodOffset;
            this.verticalPeriodOffset = movement.verticalPeriodOffset;
            this.horizontalAcceleration = movement.horizontalAcceleration;
            this.verticalAcceleration = movement.horizontalAcceleration;
        }

        public static Movement Read(Movement movement, MovementData data) {
            movement.movementName = data.movementName;
            movement.duration = data.duration;
            movement.horizontalSpeed = data.horizontalSpeed;
            movement.verticalSpeed = data.verticalSpeed;
            movement.horizontalAmplitude = data.horizontalAmplitude;
            movement.verticalAmplitude = data.verticalAmplitude;
            movement.horizontalPeriod = data.horizontalPeriod;
            movement.verticalPeriod = data.verticalPeriod;
            movement.horizontalPeriodOffset = data.horizontalPeriodOffset;
            movement.verticalPeriodOffset = data.verticalPeriodOffset;
            movement.horizontalAcceleration = data.horizontalAcceleration;
            movement.verticalAcceleration = data.horizontalAcceleration;
            movement.ticks = 0f;
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

        if (circle) {
            float amp = 5;
            if (horizontalAmplitude != 0 || verticalAmplitude != 0) {
                amp = Mathf.Abs(horizontalAmplitude) > Mathf.Abs(verticalAmplitude) ? horizontalAmplitude : verticalAmplitude;
            }

            float period = 1;
            if (horizontalPeriod != 0 || verticalPeriod != 0) {
                period = Mathf.Abs(horizontalPeriod) > Mathf.Abs(verticalPeriod) ? horizontalPeriod : verticalPeriod;
            }
            horizontalAmplitude = amp; verticalAmplitude = amp;
            horizontalPeriod = period; verticalPeriod = period;
            duration = period;
            horizontalPeriodOffset = 0f;
            verticalPeriodOffset = 0.25f;
            circle = false;
        }

        Move();
        Predict();
    }

    // Movement mechanics
    private void Move() {
        ticks += Time.deltaTime;
        if (ticks > duration) {
            reset = true;
        }

        float time = reverse ? -ticks : ticks;

        Vector3 acceleration = new Vector3(horizontalAcceleration * time, verticalAcceleration * time, 0f);

        float horizontal = reverse ? -horizontalSpeed : horizontalSpeed;
        if (horizontalPeriod != 0) {
            horizontal += horizontalAmplitude * Mathf.Sin(2 * Mathf.PI * (time / horizontalPeriod + horizontalPeriodOffset));
        }
        if (flip) { horizontal *= -1; }

            float vertical = reverse ? -verticalSpeed : verticalSpeed;
        if (verticalPeriod != 0) {
            vertical += verticalAmplitude * Mathf.Sin(2 * Mathf.PI * (time / verticalPeriod + verticalPeriodOffset));
        }


        Vector3 velocity = new Vector3(horizontal, vertical, 0f) + acceleration;
        transform.position += velocity * Time.deltaTime;
    }

    private void Predict() {

        int count = (int)Mathf.Floor(duration / Time.deltaTime);
        List<Vector3> points = new List<Vector3>();
        points.Add(origin);

        for (int i = 1; i < count; i++) {

            float time = i * Time.deltaTime;
            Vector3 newPoint = PredictStep(points[i - 1], time, Time.deltaTime);
            points.Add(newPoint);
        }

        for (int i = 1; i < points.Count; i++) {
            Debug.DrawLine(points[i - 1], points[i], Color.yellow, Time.deltaTime, false);
        }

    }

    private Vector3 PredictStep(Vector3 start, float time, float deltaTime) {

        time = reverse ? -time : time;

        Vector3 acceleration = new Vector3(horizontalAcceleration * time, verticalAcceleration * time, 0f);

        float horizontal = reverse ? -horizontalSpeed : horizontalSpeed;
        if (horizontalPeriod != 0) {
            horizontal += horizontalAmplitude * Mathf.Sin(2 * Mathf.PI * (time / horizontalPeriod + horizontalPeriodOffset));
        }
        if (flip) { horizontal *= -1; }

        float vertical = reverse ? -verticalSpeed : verticalSpeed;
        if (verticalPeriod != 0) {
            vertical += verticalAmplitude * Mathf.Sin(2 * Mathf.PI * (time / verticalPeriod + verticalPeriodOffset));
        }

        Vector3 velocity = new Vector3(horizontal, vertical, 0f) + acceleration;
        return start + velocity * deltaTime;
    }

}
