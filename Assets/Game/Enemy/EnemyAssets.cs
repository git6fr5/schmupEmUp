using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAssets : MonoBehaviour {

    [System.Serializable]
    public class AssetData {

        public Sprite regularSprite;
        public Sprite highlightSprite;
        public Transform[] engineNodes;
        public Vector3[] trailNodes;

    }

    public Material enemyTrail;
    public static Material EnemyTrail;

    public AssetData[] assets;
    public static AssetData[] Assets;

    public Bullet[] bullets;
    public static Bullet[] Bullets;

    public Sprite[] engineAnimation;
    public static Sprite[] EngineAnimation;

    void Start() {
        EnemyTrail = enemyTrail;
        Assets = assets;
        Bullets = bullets;
        EngineAnimation = engineAnimation;
    }

    public enum BulletType {
        Circle,
        Diamond,
        Oval,
        Fire,
        Ring,
        Beam
    }

}
