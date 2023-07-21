using System.Collections;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [SerializeField] public AudioFile[] audioFiles;
    public AudioSource m_Sfx;
    public AudioSource m_SfxPitchVariation;
    [Range(0, 1)] public float OverallVolume_SFX;
    [Range(0, 1)] public float OverallVolume_PitchVariation;


    private void Awake()
    {

        //Make the AudioManager a singleton.
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetVolume();
    }
    public void SetVolume()
    {
        m_Sfx.volume = instance.OverallVolume_SFX;
        m_SfxPitchVariation.volume = instance.OverallVolume_PitchVariation;
    }

    public void PlaySFX(string audioName)
    {
        var file = GetFileByName(audioName);

        if (file != null)
        {
            if (file.Clip != null)
            {
                m_Sfx.clip = file.Clip;
                m_Sfx.volume = instance.OverallVolume_SFX;
                m_Sfx.Play();
            }
            else Debug.LogError("This AudioFile does not have any AudioClip: " + audioName);
        }
        else Debug.LogError("Trying to play a sound that not exist: " + audioName);
    }

    public void PlaySFXPitchVariation(string audioName)
    {
        var file = GetFileByName(audioName);

        if (file != null)
        {
            if (file.Clip != null)
            {
                m_SfxPitchVariation.clip = file.Clip;
                m_SfxPitchVariation.volume = instance.OverallVolume_PitchVariation;
                m_SfxPitchVariation.pitch = 1 + Random.Range(-0.3f, 0.3f);
                m_SfxPitchVariation.Play();
            }
            else Debug.LogError("This AudioFile does not have any AudioClip: " + audioName);
        }
        else Debug.LogError("Trying to play a sound that not exist: " + audioName);
    }


    private AudioFile GetFileByName(string soundName)
    {
        var file = audioFiles.First(x => x.Name == soundName);
        if (file != null)
        {
            return file;
        }
        else Debug.LogError("Trying to play a sound that not exist: " + soundName);

        return null;

    }
}
