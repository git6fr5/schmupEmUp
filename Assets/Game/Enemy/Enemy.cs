// Libraries
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

// Definitions
using Type = GameRules.Type;
using PatternData = Pattern.PatternData;
using MovementData = Movement.MovementData;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Pattern))]
public class Enemy : MonoBehaviour {

    SpriteRenderer spriteRenderer;
    CircleCollider2D hitbox;

    public string enemyName;
    public bool save;
    public bool load;
    public bool reset;
    public bool init;
    [HideInInspector] public bool isInitialized = false;

    public Type type;
    public Bullet bullet;
 
    public string[] bulletPatternFiles;
    public PatternData[] bulletPatternData;

    public string[] movementFiles;
    public MovementData[] movementData;

    Pattern pattern;
    Movement movement;

    public bool isFlying;

    public int currPattern;
    public int currMovement;

    public bool cycle;
    public bool reverse;
    public bool continous;
    Vector3 origin;

    [HideInInspector] public bool flip;

    [System.Serializable]
    public class EnemyData {

        public static string path = "Enemies/";
        public static string filetype = ".enemy";

        public string enemyName;
        public string[] bulletPatternFiles;
        public string[] movementFiles;
        public bool isFlying;
        public bool cycle;
        public bool continous;

        public EnemyData(Enemy enemy) {
            this.enemyName = enemy.enemyName;
            this.bulletPatternFiles = enemy.bulletPatternFiles;
            this.movementFiles = enemy.movementFiles;
            this.isFlying = enemy.isFlying;
            this.cycle = enemy.cycle;
            this.continous = enemy.continous;
        }

        public static Enemy Read(Enemy enemy, EnemyData data) {
            enemy.enemyName = data.enemyName;
            enemy.bulletPatternFiles = data.bulletPatternFiles;
            enemy.movementFiles = data.movementFiles;
            enemy.isFlying = data.isFlying;
            enemy.cycle = data.cycle;
            enemy.continous = data.continous;
            enemy.currPattern = 0;
            enemy.currMovement = 0;
            enemy.init = false;
            enemy.reverse = false;
            return enemy;
        }

        public static void Save(Enemy enemy) {

            EnemyData data = new EnemyData(enemy);

            // Concatenate the path.
            string fullPath = GameRules.Path + path + enemy.enemyName + filetype;

            // Format the data.
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(fullPath, FileMode.Create);
            formatter.Serialize(fileStream, data);

            // Close the file.
            fileStream.Close();

        }

        public static EnemyData Load(string filename) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + filename + filetype;
            EnemyData data = null;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                data = (EnemyData)formatter.Deserialize(fileStream);

                // Close the file.
                fileStream.Close();

            }

            return data;

        }

        public static void Load(Enemy enemy) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + enemy.enemyName + filetype;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                EnemyData data = (EnemyData)formatter.Deserialize(fileStream);

                Read(enemy, data);

                // Close the file.
                fileStream.Close();

            }

            return;

        }

    }

    // Start is called before the first frame update
    void Start() {
        // Color();
        // Collider();
    }

    // Run this to initialize the enemy
    public void Init(Type type, Vector3 position) {

        this.type = type;
        transform.position = position;
        origin = position;
        pattern = GetComponent<Pattern>();
        movement = GetComponent<Movement>();

        movementData = new MovementData[movementFiles.Length];
        for (int i = 0; i < movementFiles.Length; i++) {

            MovementData newMovementData = MovementData.Load(movementFiles[i]);
            movementData[i] = newMovementData;
        }

        MovementData.Read(movement, movementData[currMovement]);
        movement.reverse = reverse;
        movement.flip = flip;
        movement.origin = origin;

        bulletPatternData = new PatternData[bulletPatternFiles.Length];
        for (int i = 0; i < bulletPatternFiles.Length; i++) {

            PatternData newPatternData = PatternData.Load(bulletPatternFiles[i]);
            bulletPatternData[i] = newPatternData;
        }

        pattern = PatternData.Read(pattern, bulletPatternData[currPattern]);
        pattern.loop = false;
        pattern.isFinished = false;
        pattern.bulletBase = bullet;
        pattern.debugBullets = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        // spriteRenderer.sprite = GameRules.EnemySprites[spriteInd]
        Color(); // Shouldn't need to do this in update.

        gameObject.SetActive(true);
        isInitialized = true;
    }

    // Update is called once per frame
    void Update() {

        if (save) {
            EnemyData.Save(this);
            save = false;
        }

        if (load) {
            EnemyData.Load(this);
            load = false;
        }

        if (reset) {
            transform.position = origin;
            reverse = false;
            currMovement = 0;
            currPattern = 0;
            init = true;
            reset = false;
        }

        if (init) {
            Init(type, transform.position);
            init = false;
        }

        Scroll();
        Collision();

        if (!isInitialized) {
            return;
        }

        if (pattern.isFinished) {
            currPattern = (currPattern + 1) % bulletPatternData.Length;
            PatternData.Read(pattern, bulletPatternData[currPattern]);
            pattern.isInitialized = false;
            pattern.loop = false;
            pattern.isFinished = false;
        }

        if (movement.ticks >= (movement.duration - 2 * Time.deltaTime)) {
            currMovement = (currMovement + 1) % movementData.Length;
            MovementData.Read(movement, movementData[currMovement]);

            if (cycle && currMovement == 0) {
                reverse = !reverse;
            }
            else if (!continous && currMovement == 0) {
                transform.position = origin;
            }

            movement.reverse = reverse;
            movement.flip = flip;
            movement.origin = transform.position;
        }

    }

    // The natural scrolling
    void Scroll() {
        if (isFlying) {
            origin += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
            transform.position += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
        }
    }

    // Color
    public void Color(Type type) {
        this.type = type;
        Color();
    }

    private void Color() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (type == Type.RedEnemy) {
            spriteRenderer.material = GameRules.RedMaterial;
        }
        else if (type == Type.BlueEnemy) {
            spriteRenderer.material = GameRules.BlueMaterial;
        }
    }

    public void Collider() {
        hitbox = GetComponent<CircleCollider2D>();
        hitbox.isTrigger = true;
    }

    void Collision() {

        float radius = 0.3f;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < collisions.Length; i++) {
            Bullet bullet = collisions[i].GetComponent<Bullet>();
            if (bullet != null) {

                int bulletType = (int)bullet.type - GameRules.ColorPaletteSize;
                int enemyType = (int)type;
                // There's a smarter way to do this I'm sure.
                if (bulletType == enemyType) {
                    Destroy(gameObject);
                }
            }
        }

    }

}
