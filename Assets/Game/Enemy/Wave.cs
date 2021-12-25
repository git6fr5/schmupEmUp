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
    [Space(5), Header("Editing")]
    public bool editing;

    [Space(5), Header("IO")]
    public bool save;
    public bool load;
    public bool flip;
    public bool reset;
    public bool pause;
    public Enemy enemyBase;

    [Space(5), Header("Properties")]
    public string enemyName;
    public Type type;
    public int spawnsPerWave;
    public int spawnSpread;
    public int waves;
    public float waveDelay;
    public bool isCompleted;

    public bool isInitialized;
    public bool isFlipped;

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
        public string enemyName;

        public int spawnsPerWave;
        public int spawnSpread;
        public int waves;
        public float waveDelay;

        public SerializableVector3 origin;
        public SerializableVector3 targetPoint;
        public float initializeDelay;
        public float waveSpeed;

        public WaveData(Wave wave) {
            this.waveName = wave.waveName;
            this.enemyName = wave.enemyName;

            this.spawnsPerWave = wave.spawnsPerWave;
            this.spawnSpread = wave.spawnSpread;
            this.waves = wave.waves;
            this.waveDelay = wave.waveDelay;

            this.origin = new SerializableVector3(wave.origin);
            this.targetPoint = new SerializableVector3(wave.targetPoint);
            this.initializeDelay = wave.initializeDelay;
            this.waveSpeed = wave.waveSpeed;
        }

        public static Wave Read(Wave wave, WaveData data) {
            wave.waveName = data.waveName;
            wave.enemyName = data.enemyName;

            wave.spawnsPerWave = data.spawnsPerWave;
            wave.spawnSpread = data.spawnSpread;
            wave.waves = data.waves;
            wave.waveDelay = data.waveDelay;

            wave.origin = data.origin.Deserialize();
            wave.targetPoint = data.targetPoint.Deserialize();
            wave.initializeDelay = data.initializeDelay;
            wave.waveSpeed = data.waveSpeed;

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
        // origin = transform.position;
        isInitialized = false;
        isFlipped = false;
    }

    // Update is called once per frame
    void Update() {

        if (editing) {
            transform.position = origin;
            if (Input.GetMouseButton(0)) {
                origin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                origin -= origin.z * Vector3.forward;
            }
            if (Input.GetMouseButton(1)) {
                targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
                targetPoint -= targetPoint.z * Vector3.forward;
            }
        }

        if (save) {
            WaveData.Save(this);
            save = false;
        }

        if (load) {
            WaveData.Load(this);
            // Because we actually only care about this delta.
            if (!isInitialized) {
                transform.position += origin;
            }
            if (flip) {
                targetPoint = new Vector3(-targetPoint.x, targetPoint.y, targetPoint.z);
                if (!isFlipped) {
                    Vector3 newPosition = new Vector3(-transform.position.x, transform.position.y, transform.position.z);
                    transform.position = newPosition;
                    origin = newPosition;
                    isFlipped = true;
                }
            }
            else if (!flip) {
                isFlipped = false;
            }
            load = false;
            isInitialized = true;
        }

        if (isCleared) {
            // reset = true;
        }

        if (reset) {
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
                spawns[i].transform.position += targetPoint.normalized * waveSpeed * Time.deltaTime;
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

        Vector3 v = targetPoint.normalized * waveSpeed * initializeDelay;
        Debug.DrawLine(transform.position, transform.position + v, Color.red, Time.deltaTime, false);
        Vector3 n = (Quaternion.Euler(0f, 0f, 90f) * v).normalized * spawnSpread / 2f;
        Debug.DrawLine(transform.position - n, transform.position + n, Color.red, Time.deltaTime, false);

        Scroll();
    }

    // The natural scrolling
    void Scroll() {
        origin += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
    }

    // Spawn the enemy
    private IEnumerator IESpawn() {

        isCompleted = false;

        // Get the data.
        EnemyData newEnemyData = EnemyData.Load(enemyName);
        for (int i = 0; i < waves; i++) {

            while (pause) {
                yield return null;
            }

            for (int j = 0; j < spawnsPerWave; j++) {
                // Create the enemy.
                Enemy newEnemy = Instantiate(enemyBase.gameObject).GetComponent<Enemy>();
                // Put the data into the enemy
                EnemyData.Read(newEnemy, newEnemyData);
                spawns.Add(newEnemy);

                float midPoint = (((float)spawnsPerWave - 1f) / 2f);
                float _j = (float)j;
                float invDenom = 0f;
                if (spawnsPerWave > 1f) {
                    invDenom = 1 / ((float)spawnsPerWave - 1f);
                }
                float offset = (_j - midPoint) * invDenom;
                Vector3 n = (Quaternion.Euler(0f, 0f, 90f) * targetPoint.normalized) * spawnSpread / 2f;

                // Start the initialization process
                newEnemy.gameObject.SetActive(true);
                newEnemy.transform.position = transform.position + n * offset;
                newEnemy.GetComponent<Movement>().duration = initializeDelay + Time.deltaTime;
                newEnemy.Color(type);
                StartCoroutine(IEInitializeEnemy(newEnemy, initializeDelay));
            }
                    
            yield return new WaitForSeconds(waveDelay);
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
