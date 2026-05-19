using UnityEngine;
using UnityEngine.Audio; // 🔥 Necesario para controlar el AudioMixer

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("Global Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private AudioSource playerMovementSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // No se destruye al cambiar de nivel
            ConfigurarFuentesGlobales();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Añade esto dentro de tu AudioManager.cs

    /// <summary>
    /// Hace un fade out de la música actual en un tiempo determinado.
    /// </summary>
    public void FadeOutMusic(float duration)
    {
        // Usamos el propio AudioManager para correr la corrutina y que no se destruya al cambiar de escena
        StartCoroutine(FadeOutMusicCoroutine(duration));
    }

    private System.Collections.IEnumerator FadeOutMusicCoroutine(float duration)
    {
        // Asumiendo que guardas la referencia al AudioSource de la música en una variable llamada 'musicSource'
        // Si tu canal de música se llama diferente (ej: musicChannel), cámbialo aquí.
        if (musicSource == null || !musicSource.isPlaying) yield break;

        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Va reduciendo el volumen de manera lineal a lo largo del tiempo
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
            yield return null; // Espera al siguiente frame
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        // Restablecemos el volumen original del componente para la próxima pista que se reproduzca en el juego
        musicSource.volume = startVolume;
    }
    private void ConfigurarFuentesGlobales()
    {
        if (playerMovementSource == null) playerMovementSource = gameObject.AddComponent<AudioSource>();

        // 🔥 AQUÍ ESTÁ LA CLAVE: Se enruta al canal de SFX, NO al de música
        playerMovementSource.outputAudioMixerGroup = sfxGroup;
        playerMovementSource.loop = true; // Forzamos a que repita el clip continuamente
        // Si no creaste los AudioSource en el objeto, los creamos dinámicamente
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        // Enrutamos las fuentes a sus respectivos canales del Mixer
        musicSource.outputAudioMixerGroup = musicGroup;
        sfxSource.outputAudioMixerGroup = sfxGroup;

        musicSource.loop = true; // La música de ambiente siempre va en loop
    }

    // --- REPRODUCCIÓN DE MÚSICA ---
    public void PlayMusic(AudioClip musicClip, float volume = 1f)
    {
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.clip = musicClip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // --- REPRODUCCIÓN DE SFX 2D (Globales: UI, Inventario, Recoger ítems) ---
    public void PlaySFX2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        // PlayOneShot permite reproducir múltiples sonidos superpuestos en la misma fuente sin cortarse
        sfxSource.PlayOneShot(clip, volume);
    }

    public void ControlarPasosLoop(AudioClip clip, bool reproducir, float volumen)
    {
        // Si el jugador se detiene, apagamos el reproductor de inmediato
        if (!reproducir || clip == null)
        {
            if (playerMovementSource.isPlaying) playerMovementSource.Stop();
            return;
        }

        // Si el clip cambió (de caminar a correr o viceversa), hacemos el cambio
        if (playerMovementSource.clip != clip)
        {
            playerMovementSource.clip = clip;
            playerMovementSource.volume = volumen;
            playerMovementSource.Play();
        }
        // Si es el mismo clip pero por alguna razón se había pausado/detenido, le damos Play
        else if (!playerMovementSource.isPlaying)
        {
            playerMovementSource.Play();
        }
    }

    // --- OPTIMIZACIÓN MÁXIMA PARA OBJETOS/EFECTOS EN EL MUNDO (3D) ---
    // Crea un sonido en una posición del mapa, se enruta al SFX y el objeto se autodestruye al terminar
    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempAudioObj = new GameObject("TempAudio_3D");
        tempAudioObj.transform.position = position;

        AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;

        // Configuración Espacial 3D
        tempSource.spatialBlend = 1.0f; // 1.0 significa totalmente 3D
        tempSource.outputAudioMixerGroup = sfxGroup; // Lo mandamos al canal SFX
        tempSource.minDistance = 2f;
        tempSource.maxDistance = 15f;
        tempSource.rolloffMode = AudioRolloffMode.Linear;

        tempSource.Play();

        // Destruye el objeto automáticamente cuando el clip termine de sonar
        Destroy(tempAudioObj, clip.length);
    }

    public AudioSource PlaySFXLoop(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return null;

        // Creamos un GameObject hijo del AudioManager para que no ensucie la jerarquía
        GameObject loopObj = new GameObject($"LoopSFX_{clip.name}");
        loopObj.transform.SetParent(this.transform);

        AudioSource newSource = loopObj.AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.volume = volume;
        newSource.loop = true; // 🔥 Activamos el loop
        newSource.outputAudioMixerGroup = sfxGroup; // Enrutado al canal de SFX

        newSource.Play();

        return newSource; // Devolvemos el componente para que el Warning lo controle
    }
}
