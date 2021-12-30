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

    public bool neverDeleteMe;

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

    [Range(0, 9)] public int assetIndex;

    public EnemyAssets.BulletType _bulletIndex;
    private int bulletIndex;
    public int bulletSize;

    private Sprite regularSprite;
    private Sprite highlightSprite;

    public Transform[] engineNodes;

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

        public int assetIndex;

        public int bulletIndex;
        public int bulletSize;

        public EnemyData(Enemy enemy) {
            this.enemyName = enemy.enemyName;
            this.bulletPatternFiles = enemy.bulletPatternFiles;
            this.movementFiles = enemy.movementFiles;
            this.isFlying = enemy.isFlying;
            this.cycle = enemy.cycle;
            this.continous = enemy.continous;
            this.assetIndex = enemy.assetIndex;
            this.bulletIndex = enemy.bulletIndex;
            this.bulletSize = enemy.bulletSize;
        }

        public static Enemy Read(Enemy enemy, EnemyData data) {
            enemy.enemyName = data.enemyName;
            enemy.bulletPatternFiles = data.bulletPatternFiles;
            enemy.movementFiles = data.movementFiles;
            enemy.isFlying = data.isFlying;
            enemy.cycle = data.cycle;
            enemy.continous = data.continous;
            enemy.assetIndex = data.assetIndex;
            enemy.bulletIndex = data.bulletIndex;
            enemy.bulletSize = data.bulletSize;
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

    public void Set(Type type) {
        // spriteRenderer.sprite = GameRules.EnemySprites[spriteInd]
        this.type = type;

        regularSprite = EnemyAssets.Assets[assetIndex].regularSprite;
        highlightSprite = EnemyAssets.Assets[assetIndex].highlightSprite;
        _bulletIndex = (EnemyAssets.BulletType)bulletIndex;
        bullet = EnemyAssets.Bullets[bulletIndex];
        bullet.index = bulletSize;
        Color(); // Shouldn't need to do this in update.

        spriteRenderer.sprite = regularSprite;
        hasBeenOnScreen = false;

        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        engineNodes = new Transform[EnemyAssets.Assets[assetIndex].engineNodes.Length];
        for (int i = 0; i < EnemyAssets.Assets[assetIndex].engineNodes.Length; i++) {
            Transform newNode = Instantiate(EnemyAssets.Assets[assetIndex].engineNodes[i].gameObject).GetComponent<Transform>();
            newNode.parent = transform;
            newNode.localPosition = EnemyAssets.Assets[assetIndex].engineNodes[i].localPosition;
            engineNodes[i] = newNode;
        }
        //for (int i = 0; i < engineNodes.Length; i++) {
        //    GameRules.PlayAnimation(transform.position, EnemyAssets.EngineAnimation, true, engineNodes[i], true);
        //}
        // gameObject.SetActive(true);
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
        pattern.debugBullets = true;
        pattern.bulletBase = bullet;

        spriteRenderer = GetComponent<SpriteRenderer>();
        
        isInitialized = true;
    }

    public bool hasBeenOnScreen = false;

    // Update is called once per frame
    void Update() {

        if (GameRules.OnScreen(transform.position) && !hasBeenOnScreen) {
            hasBeenOnScreen = true;
        }

        //if (transform.position.y - GameRules.MainCamera.transform.position).magnitude > 50f) {
        //    if (!neverDeleteMe) {
        //        Destroy(gameObject);
        //    }
        //}

        if (!GameRules.OnScreen(transform.position) && hasBeenOnScreen) {
            if (!neverDeleteMe) {
                Destroy(gameObject);
            }
        }

        // 

        if (save) {
            bulletIndex = (int)_bulletIndex;
            EnemyData.Save(this);
            save = false;
        }

        if (load) {
            EnemyData.Load(this);
            load = false;
        }

        if (reset) {
            Set(type);
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
        Highlight();

        if (!isInitialized) {
            return;
        }

        RenderTrail();

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

    private void Highlight() {
        if (pattern != null) {
            if (pattern.isFiring) {
                spriteRenderer.sprite = highlightSprite;
                spriteRenderer.material.SetFloat("_Highlight", 1f);
            }
            else {
                spriteRenderer.sprite = regularSprite;
                spriteRenderer.material.SetFloat("_Highlight", 0f);
            }
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
                    if (!neverDeleteMe) {
                        Destroy(gameObject);
                    }
                }
            }
        }

    }

    void RenderTrail() {

        SimulationB();

    }

    public static float SegmentLength = 0.25f;
    public static int ConstraintDepth = 10;

    public static float TrailStartWidth = 2f/16f;
    public static float TrailEndWidth = 0f;

    public static float RopeLength = 5f;
    public float weight;
    private Vector3[][] ropeSegments; // The current positions of the segments.
    // protected Vector3[][] prevRopeSegments; // The previous positions of the segments.

    private bool initTrail;

    void SimulationB() {

        if (ropeSegments == null || ropeSegments.Length != engineNodes.Length) {
            ropeSegments = new Vector3[engineNodes.Length][];
            for (int n = 0; n < ropeSegments.Length; n++) {
                ropeSegments[n] = new Vector3[(int)(RopeLength / SegmentLength)];
            }
        }

        for (int n = 0; n < engineNodes.Length; n++) {

            ropeSegments[n][0] = engineNodes[n].position;
            for (int i = 1; i < ropeSegments[n].Length; i++) {
                ropeSegments[n][i] += GameRules.ScrollSpeed * Vector3.up * Time.deltaTime;
                // ropeSegments[i] += (Vector3)velocity / 20f * Time.deltaTime;
                if ((ropeSegments[n][i - 1] - ropeSegments[n][i]).magnitude > SegmentLength) {
                    ropeSegments[n][i] += (ropeSegments[n][i - 1] - ropeSegments[n][i]).normalized * Time.deltaTime * 5f;
                }
            }

            for (int i = 0; i < ConstraintDepth; i++) {
                Constraints(engineNodes[n].position, ropeSegments[n]);
            }

            LineRenderer trailRenderer = engineNodes[n].GetComponent<LineRenderer>();
            if (trailRenderer == null) {
                engineNodes[n].gameObject.AddComponent<LineRenderer>();
                trailRenderer = engineNodes[n].GetComponent<LineRenderer>();
                trailRenderer.materials = new Material[] { EnemyAssets.EnemyTrail };
            }

            trailRenderer.startWidth = TrailStartWidth;
            trailRenderer.endWidth = TrailEndWidth;
            trailRenderer.positionCount = ropeSegments[n].Length;
            trailRenderer.SetPositions(ropeSegments[n]);

            if (type == Type.BlueEnemy) {
                Color col = GameRules.Blue;
                col.a = 0.25f;
                trailRenderer.materials[0].SetColor("_Color", GameRules.Blue);
            }
            else {
                Color col = GameRules.Red;
                col.a = 0.25f;
                trailRenderer.materials[0].SetColor("_Color", GameRules.Red);
            }
        }
    }

    void Constraints(Vector3 origin, Vector3[] positions) {
        positions[0] = origin;
        for (int i = 1; i < positions.Length; i++) {
            // Get the distance and direction between the segments.
            float newDist = (positions[i - 1] - positions[i]).magnitude;
            Vector3 direction = (positions[i - 1] - positions[i]).normalized;

            // Get the error term.
            float error = newDist - SegmentLength;
            Vector3 errorVector = direction * error;

            // Adjust the segments by the error term.
            if (i != 1) {
                positions[i - 1] -= errorVector * 0.5f;
            }
            positions[i] += errorVector * 0.5f;
        }
        positions[0] = origin;
    }

}
