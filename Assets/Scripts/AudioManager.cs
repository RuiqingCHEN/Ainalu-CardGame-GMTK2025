using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource; // 背景音乐
    [SerializeField] private AudioSource sfxSource;   // 音效
    
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    
    [Header("Card Sound Effects")]
    [SerializeField] private AudioClip cardDrawSound;        // 抽牌声音
    [SerializeField] private AudioClip cardPlaceSound;       // 放置卡牌声音
    [SerializeField] private AudioClip cardSelectSound;     // 选中卡牌声音 (鼠标按下)
    [SerializeField] private AudioClip cardErrorSound;      // 放错卡槽声音
    [SerializeField] private AudioClip cardDestroySound;    // 右侧卡牌销毁声音
    [SerializeField] private AudioClip cardRecycleSound;    // 左侧卡牌回收声音
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    protected override void Awake()
    {
        base.Awake();
        
        // 确保有AudioSource组件
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("Music Source");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX Source");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }
        
        // 设置音乐源属性
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        
        // 设置音效源属性
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }
    
    private void Start()
    {
        PlayBackgroundMusic();
    }
    
    #region Background Music
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }
    
    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
    #endregion
    
    #region Sound Effects
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }
    
    private void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, volumeScale * sfxVolume);
        }
    }
    
    // 卡牌相关音效
    public void PlayCardDraw()
    {
        PlaySFX(cardDrawSound);
    }
    
    public void PlayCardPlace()
    {
        PlaySFX(cardPlaceSound);
    }
    
    public void PlayCardSelect()
    {
        PlaySFX(cardSelectSound);
    }
    
    public void PlayCardError()
    {
        PlaySFX(cardErrorSound);
    }
    
    public void PlayCardDestroy()
    {
        PlaySFX(cardDestroySound);
    }
    
    public void PlayCardRecycle()
    {
        PlaySFX(cardRecycleSound);
    }
    #endregion
    
    #region Utility Methods
    // 静音/取消静音
    public void ToggleMute()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicSource.mute;
        }
        if (sfxSource != null)
        {
            sfxSource.mute = !sfxSource.mute;
        }
    }
    
    // 检查是否静音
    public bool IsMuted()
    {
        return (musicSource != null && musicSource.mute) || 
               (sfxSource != null && sfxSource.mute);
    }
    #endregion
}