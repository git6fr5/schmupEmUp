using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRules : MonoBehaviour {

    public bool snake;
    public static bool Snake;

    public bool alternatingOrbs;
    public static bool AlternatingOrbs;

    public static float ParrallaxMax = 100f;
    public static float TunnelDepth = 100f;
    public static float BeamDepth = 60f;

    public static float OrbDepth = 30f;

    public static float PlatformShadow = 55f;
    public static float PlatformDepth = 50f;
    public static float PlatformDesigns = 49f;
    public static float PlatformOutline = 48f;
    public static float PlatformEnemies = 45f;

    public static float CliffShadowDepth = 95f;
    public static float CliffSideDepth = 22f;
    public static float CliffDepth = 20f;

    // public static string ParticleLayer = "Particles";
    public static float ParticleDepth = -7.5f;

    public static string Path = "Assets/Resources/";

    public static float MovementPrecision = 0.001f;
    public static float ScrollSpeed;

    public static Camera MainCamera;

    public static Material RedMaterial;
    public static Material BlueMaterial;

    public float scrollSpeed = 5f;
    public float maxScrollSpeed = 10f;
    public float scrollAcceleration = 0.1f;

    public Material redMaterial;
    public Material blueMaterial;

    public static GameObject PlayerObject;
    public static GameObject RestartObject;

    // Should probably just grab this from the camera instead of hardcoding it here.
    public static int ScreenPixelHeight = 320;
    public static int ScreenPixelWidth = 640;
    public static int PixelsPerUnit = 16;

    public float rotationRate = 5f;
    public float maxRotationRate = 15f;
    public float rotationAcceleration = 0.2f;
    public float currRotation;
    public RandomParams rotationChangeInterval;

    public Color blue;
    public Color red;
    public Color blueShade;
    public Color redShade;
    public static Color Blue;
    public static Color Red;
    public static Color BlueShade;
    public static Color RedShade;

    public static int Score;

    public AudioClip eatSound;
    public AudioClip fireSoundA;
    public AudioClip fireSoundB;

    public AudioClip hurtSound;
    public AudioClip lowHealthSound;
    public AudioClip loseSound;
    public AudioClip cliffSound;
    public AudioSource audioSource;

    public static AudioClip EatSound;
    public static AudioClip FireSound;
    public static AudioClip FireSoundB;
    public static AudioClip HurtSound;
    public static AudioClip LowHealthSound;
    public static AudioClip LoseSound;
    public static AudioClip CliffSound;

    public static void PlaySound(AudioClip sound, float volume = 0.5f) {
        if (Instance == null) { return; }
        if (sound == FireSound) {
            if (Random.Range(0f, 1f)> 0.5f) {
                sound = FireSoundB;
            }
        }
        AudioSource newAudio = Instantiate(Instance.audioSource.gameObject).GetComponent<AudioSource>();
        newAudio.transform.position = MainCamera.transform.position;
        newAudio.clip = sound;
        newAudio.Play();
        newAudio.volume = volume;
        Destroy(newAudio.gameObject, newAudio.clip.length * 9f / 10f);
    }

    public static int ColorPaletteSize = 2;
    public enum Type {
        //
        RedEnemy,
        BlueEnemy,
        //
        RedPlayer,
        BluePlayer,
        //
        Ethereal
    }

    public static GameRules Instance;

    void Start() {
        
        EatSound = eatSound;
        FireSound = fireSoundA;
        FireSoundB = fireSoundB;
        HurtSound = hurtSound;
        LowHealthSound = lowHealthSound;
        LoseSound = loseSound;
        CliffSound = cliffSound;

        Score = 0;

        ScrollSpeed = scrollSpeed;
        RedMaterial = redMaterial;
        BlueMaterial = blueMaterial;

        PlayerObject = ((Player)GameObject.FindObjectOfType(typeof(Player)))?.gameObject;
        RestartObject = ((RestartUI)GameObject.FindObjectOfType(typeof(RestartUI)))?.gameObject;

        MainCamera = Camera.main;

        Snake = snake;
        BlueShade = blueShade;
        RedShade = redShade;
        Blue = blue;
        Red = red;
        // StartCoroutine(IEGetRotation());

        AlternatingOrbs = alternatingOrbs;

        // Animations...
        ExplosionAnim = explosionAnim;
        EatAnim = eatAnim;
        RedFireAnimation = redFireAnimation;
        BlueFireAnimation = blueFireAnimation;

        // for easy access...
        Instance = this;
    }

    void Update() {
        if (scrollSpeed < maxScrollSpeed) {
            scrollSpeed += scrollAcceleration * Time.deltaTime;
        }
        else {
            scrollSpeed = maxScrollSpeed;
        }
        ScrollSpeed = scrollSpeed;

        // Restarting
        if (PlayerObject == null && RestartObject != null) {
            RestartObject.SetActive(true);
        }
        else {
            // RestartObject.SetActive(false);
        }

        rotationRate += rotationAcceleration * Time.deltaTime;
        MainCamera.transform.position += ScrollSpeed * Vector3.up * Time.deltaTime;
        MainCamera.transform.eulerAngles += Vector3.forward * currRotation * Time.deltaTime;
    }

    public static float GetParrallax(float z) {
        return Mathf.Pow(z / ParrallaxMax, 2f);
    }

    private IEnumerator IEGetRotation() {

        while (true) {
            bool rotate = Random.Range(0f, 1f) > 0.75f;
            currRotation = rotate ? Random.Range(-rotationRate, rotationRate) : 0f;
            yield return new WaitForSeconds(rotate ? rotationChangeInterval.min : rotationChangeInterval.max);
        }
        
    }

    [System.Serializable]
    public class RandomParams {
        public float min;
        public float max;

        public RandomParams(float min, float max) {
            this.min = min;
            this.max = max;
        }

        public float Get() {
            return Random.Range(min, max);
        }

        public float GetRound() {
            return Mathf.Round(Random.Range(min, max));
        }
    }

    [System.Serializable]
    public class SerializableVector3 {

        public float x;
        public float y;
        public float z;

        public SerializableVector3(Vector3 v) {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public Vector3 Deserialize() {
            return new Vector3(x, y, z);
        }
    }

    public static bool OnScreen(Vector3 position) {

        Vector2 screenPos = MainCamera.WorldToViewportPoint(position);
        return (screenPos.x < 1 && screenPos.x > 0 && screenPos.y < 1 && screenPos.y > 0);
    }

    // For easy access to animations...
    public static float FrameRate = 12f;

    public Sprite[] explosionAnim;
    public Sprite[] eatAnim;
    public Sprite[] redFireAnimation;
    public Sprite[] blueFireAnimation;

    public static Sprite[] ExplosionAnim;
    public static Sprite[] EatAnim;
    public static Sprite[] RedFireAnimation;
    public static Sprite[] BlueFireAnimation;

    public static void PlayAnimation(Vector3 position, Sprite[] animation, bool fixedScreenPosition = false, Transform follow = null, bool loop = false, Material material = null) {

        if (animation == null || animation.Length <= 0) {
            return;
        }
        print("Playing animation");
        Instance.StartCoroutine(Instance.IEPlayAnim(animation, position, FrameRate, fixedScreenPosition, follow, loop, material));

    }

    private IEnumerator IEPlayAnim(Sprite[] animation, Vector3 position, float frameRate, bool fixedScreenPosition, Transform follow, bool loop, Material material) {
        SpriteRenderer newAnimation = (new GameObject("New Animation", typeof(SpriteRenderer))).GetComponent<SpriteRenderer>();
        // newAnimation.sortingLayerName = ParticleLayer;
        if (material != null) {
            newAnimation.material = material;
        }
        position.z = ParticleDepth;
        newAnimation.transform.position = position;
        if (follow != null) {
            newAnimation.transform.parent = follow;
        }
        for (int i = 0; i < animation.Length; i++) {
            newAnimation.sprite = animation[i];
            if (FrameRate != 0f) {
                if (fixedScreenPosition) {
                    if (follow != null) {
                        Vector3 newPosition = follow.position;
                        newPosition.z = ParticleDepth;
                        newAnimation.transform.position = newPosition;
                    }
                    else {
                        newAnimation.transform.position += Vector3.up * (scrollSpeed + (scrollSpeed - scrollAcceleration * (1f / FrameRate))) * (1f / FrameRate) / 2f;
                    }
                }
                yield return new WaitForSeconds(1f / FrameRate);
            }
        }
        while (loop) {
            for (int i = 0; i < animation.Length; i++) {
                newAnimation.sprite = animation[i];
                if (FrameRate != 0f) {
                    if (fixedScreenPosition) {
                        if (follow != null) {
                            Vector3 newPosition = follow.position;
                            newPosition.z = ParticleDepth;
                            newAnimation.transform.position = newPosition;
                        }
                        else {
                            newAnimation.transform.position += Vector3.up * (scrollSpeed + (scrollSpeed - scrollAcceleration * (1f / FrameRate))) * (1f / FrameRate) / 2f;
                        }
                    }
                    yield return new WaitForSeconds(1f / FrameRate);
                }
            }
        }
        Destroy(newAnimation.gameObject);
    }


}
