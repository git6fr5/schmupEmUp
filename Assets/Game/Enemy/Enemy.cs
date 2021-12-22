// Libraries
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Definitions
using PatternData = Pattern.PatternData;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Pattern))]
public class Enemy : MonoBehaviour {

    public Bullet bullet;

    public string[] bulletPatternFiles;
    public PatternData[] bulletPatternData;

    public string[] movementFiles;
    // public MovementData[] movementData;

    Pattern pattern;
    Movement movement;

    // Start is called before the first frame update
    void Start() {

        pattern = GetComponent<Pattern>();
        movement = GetComponent<Movement>();

        //movementData = new MovementData[movementFiles.Length];
        //for (int i = 0; i < movementFiles,Length; i++) {

        //    MovementData newMovementData = MovementData.Load(movementFiles[i]);
        //    movementData[i] = newMovementData;
        //}

        //MovementData.Read(movement, movementData[0]);

        bulletPatternData = new PatternData[bulletPatternFiles.Length];
        for (int i = 0; i < bulletPatternFiles.Length; i++) {

            PatternData newPatternData = PatternData.Load(bulletPatternFiles[i]);
            bulletPatternData[i] = newPatternData;
        }

        pattern = PatternData.Read(pattern, bulletPatternData[0]);
        pattern.bulletBase = bullet;
        pattern.debugBullets = true;
    }

    // Update is called once per frame
    void Update() {

    }

}
