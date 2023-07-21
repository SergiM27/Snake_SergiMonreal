using UnityEngine;


[System.Serializable]
public class AudioFile 
{
    public string Name;
    public AudioClip Clip;

    [Range(0, 1)]
    public float Volume;
}
