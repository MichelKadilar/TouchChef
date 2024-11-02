using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Le clip audio à jouer")]
    public AudioClip musicClip;

    [Tooltip("Volume de la musique (0 à 1)")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Tooltip("Fondu d'entrée en secondes")]
    public float fadeInDuration = 2f;

    private AudioSource audioSource;
    private float initialVolume;

    void Awake()
    {
        // Vérifier s'il y a déjà une instance de la musique
        BackgroundMusic[] musicObjects = FindObjectsOfType<BackgroundMusic>();
        if (musicObjects.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Garder la musique entre les scènes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Configurer l'AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true; // Active la lecture en boucle
        audioSource.volume = 0; // Commencer avec un volume à 0 pour le fade in
        initialVolume = musicVolume;

        // Démarrer la musique
        audioSource.Play();
        StartCoroutine(FadeIn());
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, initialVolume, elapsedTime / fadeInDuration);
            yield return null;
        }

        audioSource.volume = initialVolume;
    }

    // Méthode publique pour ajuster le volume
    public void SetVolume(float newVolume)
    {
        musicVolume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }

    // Méthode optionnelle pour faire un fade out
    public void FadeOut(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        audioSource.Stop();
    }
}