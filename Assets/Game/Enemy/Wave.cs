// Libraries
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

// Definitions
using Type = GameRules.Type;
using EnemyData = Enemy.EnemyData;
using SerializableVector3 = GameRules.SerializableVector3;

public class Wave : MonoBehaviour {

    public string waveName;
    public bool save;
    public bool load;
    public bool flip;
    public bool reset;
    public bool pause;
    public Enemy enemyBase;

    public string[] enemyNames;
    public Type type;
    public int[] counts;
    public float[] delays;
    public bool isCompleted;

    public List<Enemy> spawns = new List<Enemy>();
    public Vector3 origin;
    public Vector3 targetPoint;
    public float initializeDelay;
    public float waveSpeed;

    private Coroutine spawnRoutine = null;
    public bool isCleared;

    [System.Serializable]
    public class WaveData {

        public static string path = "Waves/";
        public static string filetype = ".wave";

        public string waveName;
        public string[] enemyNames;
        public int[] counts;
        public float[] delays;
        public SerializableVector3 origin;
        public SerializableVector3 targetPoint;
        public float initializeDelay;
        public float waveSpeed;

        public WaveData(Wave wave) {
            this.waveName = wave.waveName;
            this.enemyNames = wave.enemyNames;
            this.counts = wave.counts;
            this.delays = wave.delays;
            this.origin = new SerializableVector3(wave.origin);
            this.targetPoint = new SerializableVector3(wave.targetPoint);
            this.initializeDelay = wave.initializeDelay;
            this.waveSpeed = wave.waveSpeed;
        }

        public static Wave Read(Wave wave, WaveData data) {
            wave.waveName = data.waveName;
            wave.enemyNames = data.enemyNames;
            wave.counts = data.counts;
            wave.delays = data.delays;
            wave.targetPoint = data.targetPoint.Deserialize();
            wave.initializeDelay = data.initializeDelay;
            wave.waveSpeed = data.waveSpeed;
            wave.origin = data.origin.Deserialize();
            wave.transform.position = data.origin.Deserialize();
            return wave;
        }

        public static void Save(Wave wave) {

            WaveData data = new WaveData(wave);

            // Concatenate the path.
            string fullPath = GameRules.Path + path + wave.waveName + filetype;

            // Format the data.
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(fullPath, FileMode.Create);
            formatter.Serialize(fileStream, data);

            // Close the file.
            fileStream.Close();

        }

        public static WaveData Load(string filename) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + filename + filetype;
            WaveData data = null;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                data = (WaveData)formatter.Deserialize(fileStream);

                // Close the file.
                fileStream.Close();

            }

            return data;

        }

        public static void Load(Wave wave) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + wave.waveName + filetype;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                WaveData data = (WaveData)formatter.Deserialize(fileStream);

                Read(wave, data);

                // Close the file.
                fileStream.Close();

            }

            return;

        }

    }

    // Start is called before the first frame update
    void Start() {
        origin = transform.position;
    }

    // Update is called once per frame
    void Update() {

        if (save) {
            WaveData.Save(this);
            save = false;
        }

        if (load) {
            WaveData.Load(this);
            if (flip) {
                Vector3 newPosition = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
                Vector3 deltaTarget = (newPosition - transform.position);// - new Vector3(-2f * targetPoint.x, 0f, 0f);
                // targetPoint += deltaTarget;
                transform.position = newPosition;
                origin = newPosition;
            }
            load = false;
        }

        if (isCleared) {
            // reset = true;
        }

        if (reset) {

            int[] tempCounts = new int[enemyNames.Length];
            for (int i = 0; i < enemyNames.Length; i++) {
                if (i < counts.Length) {
                    tempCounts[i] = counts[i];
                }
                else {
                    tempCounts[i] = 1;
                }
            }
            counts = tempCounts;

            float[] tempDelays = new float[enemyNames.Length];
            for (int i = 0; i < enemyNames.Length; i++) {
                if (i < delays.Length) {
                    tempDelays[i] = delays[i];
                }
                else {
                    tempDelays[i] = 0f;
                }
            }
            delays = tempDelays;

            for (int i = 0; i < spawns.Count; i++) {
                if (spawns[i] != null) {
                    Destroy(spawns[i].gameObject);
                }
            }

            spawns = new List<Enemy>();
            if (spawnRoutine != null) {
                StopCoroutine(spawnRoutine);
            }
            spawnRoutine = StartCoroutine(IESpawn());

            isCleared = false;
            reset = false;
            return;
        }

        for (int i = 0; i < spawns.Count; i++) {
            if (spawns[i] != null && !spawns[i].isInitialized) {
                spawns[i].transform.position += (targetPoint - spawns[i].transform.position).normalized * waveSpeed * Time.deltaTime;
            }
        }

        bool tempCleared = true;
        for (int i = 0; i < spawns.Count; i++) {
            if (spawns[i] != null) {
                tempCleared = false;
            }
        }

        if (isCompleted && tempCleared) {
            isCleared = true;
        }

        Vector3 v = (targetPoint - transform.position).normalized * waveSpeed * initializeDelay;
        Debug.DrawLine(transform.position, transform.position + v, Color.red, Time.deltaTime, false);

        Scroll();
    }

    // The natural scrolling
    void Scroll() {
        origin += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        targetPoint += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
    }

    // Spawn the enemy
    private IEnumerator IESpawn() {

        isCompleted = false;

        for (int i = 0; i < enemyNames.Length; i++) {
            // Get the data.
            EnemyData newEnemyData = EnemyData.Load(enemyNames[i]);
            for (int j = 0; j < counts[i]; j++) {

                while (pause) {
                    yield return null;
                }

                // Create the enemy.
                Enemy newEnemy = Instantiate(enemyBase.gameObject).GetComponent<Enemy>();
                // Put the data into the enemy
                EnemyData.Read(newEnemy, newEnemyData);
                spawns.Add(newEnemy);

                // Start the initialization process
                newEnemy.gameObject.SetActive(true);
                newEnemy.transform.position = transform.position;
                newEnemy.GetComponent<Movement>().duration = initializeDelay + Time.deltaTime;
                newEnemy.Color(type);
                StartCoroutine(IEInitializeEnemy(newEnemy, initializeDelay));

                yield return new WaitForSeconds(delays[i]);
            }
        }

        isCompleted = true;
    }

    // Initialize the enemy
    private IEnumerator IEInitializeEnemy(Enemy enemy, float delay) {
        yield return new WaitForSeconds(delay);
        if (flip) {
            enemy.flip = true;
        }
        enemy.Init(type, enemy.transform.position);
        yield return null;
    }

}
