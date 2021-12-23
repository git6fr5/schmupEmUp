using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

using BulletType = GameRules.Type;

public class Pattern : MonoBehaviour {

    // public Nodes node[];

    // Pattern => Shots => Individual Bullets

    [Space(5), Header("Debug")]
    public Bullet bulletBase;
    [Range(0f, 1f)] public float debugLineDuration;
    public bool debugLines;
    public bool debugBullets;

    [Space(5), Header("Pattern")]
    public string patternName;
    public float patternInterval; // Time in between the pattern
    public bool save;
    public bool load;

    [Space(5), Header("Shots")]
    public float shotCount;
    public float shotInterval; // Time in between the shots
    public float shotInitAngle; // The initial angle the first shot fires at
    public float shotSpread; // The total amount of spread that the shots cover

    [Space(5), Header("Bullets")]
    public float bulletCount; // The amount of bullets per shot
    public float bulletInterval; // The time between each bullet in the shot
    public float bulletInitAngle; // The initial angle the first bullet fires at (relative to the shot angle
    public float bulletSpread; // The total amount of spread that the bullets cover

    public float bulletSpeed;
    public float bulletAccelerationAngle; // Relative to the direction that the bullet is travelling in.
    public float bulletAccelerationMagnitude;

    [System.Serializable]
    public class PatternData {

        public static string path = "Patterns/";
        public static string filetype = ".pattern";

        public string patternName;
        public float patternInterval;
        public float shotCount;
        public float shotInterval;
        public float shotInitAngle;
        public float shotSpread;
        public float bulletCount;
        public float bulletInterval;
        public float bulletInitAngle;
        public float bulletSpread;
        public float bulletSpeed;
        public float bulletAccelerationAngle;
        public float bulletAccelerationMagnitude;

        public PatternData(Pattern pattern) {
            this.patternName = pattern.patternName;
            this.patternInterval = pattern.patternInterval;
            this.shotCount = pattern.shotCount;
            this.shotInterval = pattern.shotInterval;
            this.shotInitAngle = pattern.shotInitAngle;
            this.shotSpread = pattern.shotSpread;
            this.bulletCount = pattern.bulletCount;
            this.bulletInterval = pattern.bulletInterval;
            this.bulletInitAngle = pattern.bulletInitAngle;
            this.bulletSpread = pattern.bulletSpread;
            this.bulletSpeed = pattern.bulletSpeed;
            this.bulletAccelerationAngle = pattern.bulletAccelerationAngle;
            this.bulletAccelerationMagnitude = pattern.bulletAccelerationMagnitude;
        }

        public static void Save(Pattern pattern) {

            PatternData data = new PatternData(pattern);

            // Concatenate the path.
            string fullPath = GameRules.Path + path + pattern.patternName + filetype;

            // Format the data.
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(fullPath, FileMode.Create);
            formatter.Serialize(fileStream, data);

            // Close the file.
            fileStream.Close();

        }

        public static PatternData Load(string filename) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + filename + filetype;
            PatternData data = null;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                data = (PatternData)formatter.Deserialize(fileStream);

                // Close the file.
                fileStream.Close();

            }

            return data;

        }

        public static void Load(Pattern pattern) {

            // Concatenate the path.
            string fullPath = GameRules.Path + path + pattern.patternName + filetype;

            if (File.Exists(fullPath)) {

                // Read the data.
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(fullPath, FileMode.Open);
                PatternData data = (PatternData)formatter.Deserialize(fileStream);

                Read(pattern, data);

                // Close the file.
                fileStream.Close();

            }

            return;

        }

        public static Pattern Read(Pattern pattern, PatternData data) {
            pattern.patternName = data.patternName;
            pattern.patternInterval = data.patternInterval;
            pattern.shotCount = data.shotCount;
            pattern.shotInterval = data.shotInterval;
            pattern.shotInitAngle = data.shotInitAngle;
            pattern.shotSpread = data.shotSpread;
            pattern.bulletCount = data.bulletCount;
            pattern.bulletInterval = data.bulletInterval;
            pattern.bulletInitAngle = data.bulletInitAngle;
            pattern.bulletSpread = data.bulletSpread;
            pattern.bulletSpeed = data.bulletSpeed;
            pattern.bulletAccelerationAngle = data.bulletAccelerationAngle;
            pattern.bulletAccelerationMagnitude = data.bulletAccelerationMagnitude;

            return pattern;
        }

    }

    private void Start() {
        StartCoroutine(IEPattern());
    }

    private void Update() {
        if (save) {
            PatternData.Save(this);
            save = false;
        }

        if (load) {
            PatternData.Load(this);
            load = false;
        }

    }

    private IEnumerator IEPattern() {

        yield return new WaitForSeconds(patternInterval);

        for (int shotNumber = 0; shotNumber < shotCount; shotNumber++) {
            // Fire shot.

            // Get the angle.
            float shotAngle = shotInitAngle;
            if (shotCount > 1) {
                shotAngle += shotNumber * (shotSpread / (shotCount-1));
            }

            for (int bulletNumber = 0; bulletNumber < bulletCount; bulletNumber++) {

                float bulletAngle = shotAngle + bulletInitAngle;
                if (bulletCount > 1) {
                    bulletAngle += bulletNumber * (bulletSpread / (bulletCount-1));
                }
                Vector3 bulletVelocity = bulletSpeed * (Quaternion.Euler(0f, 0f, bulletAngle) * Vector3.right);
                Vector2 bulletAcceleration = bulletAccelerationMagnitude * (Quaternion.Euler(0f, 0f, bulletAccelerationAngle) * bulletVelocity.normalized);

                if (debugLines) {
                    Debug.DrawLine(transform.position, transform.position + bulletVelocity, Color.yellow, debugLineDuration, false);
                }
                if (debugBullets) {
                    Enemy enemy = GetComponent<Enemy>();
                    BulletType type = enemy != null ? enemy.type : BulletType.RedEnemy;
                    Bullet newBullet = Instantiate(bulletBase.gameObject).GetComponent<Bullet>();
                    newBullet.Init(transform.position, type, bulletVelocity, bulletAcceleration);
                }
                if (bulletInterval > 0f) {
                    yield return new WaitForSeconds(bulletInterval);
                }

            }
            // Wait before firing next shot.
            if (shotInterval > 0f) {
                yield return new WaitForSeconds(shotInterval);
            }
        }

        StartCoroutine(IEPattern());
    }

}
