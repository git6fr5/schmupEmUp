using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAssets : MonoBehaviour {

    [System.Serializable]
    public class AssetData {

        public Sprite regularSprite;
        public Sprite highlightSprite;

    }

    public AssetData[] assets;
    public static AssetData[] Assets;

    public Bullet[] bullets;
    public static Bullet[] Bullets;

    void Start() {
        Assets = assets;
        Bullets = bullets;
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
