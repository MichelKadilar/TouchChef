using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip itemSpawnSound;
    [SerializeField] [Range(0f, 1f)] private float spawnVolume = 0.7f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize audio source
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayItemSpawnSound()
    {
        if (itemSpawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(itemSpawnSound, spawnVolume);
        }
        else
        {
            Debug.LogWarning("Item spawn sound or audio source not set!");
        }
    }

    // Helper method to temporarily change volume
    public void SetSpawnVolume(float volume)
    {
        spawnVolume = Mathf.Clamp01(volume);
    }
}