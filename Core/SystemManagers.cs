using UnityEngine;

namespace Game.Core.Systems
{
    /// <summary>
    /// Zone System Manager - Manages zone loading, generation, and transitions.
    /// Integrates with ZoneSceneManager for scene management.
    /// </summary>
    public class ZoneSystemManager : MonoBehaviour
    {
        [Header("Zone Management")]
        [SerializeField] private ZoneSceneManager _sceneManager;
        
        private void Awake()
        {
            _sceneManager = GetComponent<ZoneSceneManager>();
            if (_sceneManager == null)
            {
                _sceneManager = gameObject.AddComponent<ZoneSceneManager>();
            }
        }
        
        public async Awaitable<UnityEngine.SceneManagement.Scene> LoadZone(string zoneName, ZoneConfig config = null)
        {
            return await _sceneManager.LoadZone(zoneName, config);
        }
        
        public async Awaitable UnloadZone(string zoneName)
        {
            await _sceneManager.UnloadZone(zoneName);
        }
        
        public async Awaitable<UnityEngine.SceneManagement.Scene> GenerateZone(ZoneConfig config)
        {
            return await _sceneManager.GenerateAndSaveZone(config);
        }
        
        public string GetCurrentZone()
        {
            return _sceneManager.GetCurrentZone();
        }
        
        public async Awaitable ShutdownAsync()
        {
            if (_sceneManager != null)
            {
                await _sceneManager.ShutdownAsync();
            }
        }
    }
    

    /// <summary>
    /// Audio System Manager - Manages all audio playback.
    /// Handles music, sound effects, and spatial audio.
    /// </summary>
    public class AudioSystemManager : MonoBehaviour
    {
        [Header("Audio Configuration")]
        [SerializeField] private float _masterVolume = 1.0f;
        [SerializeField] private float _musicVolume = 0.7f;
        [SerializeField] private float _sfxVolume = 1.0f;
        
        private AudioSource _musicSource;
        
        private void Awake()
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.volume = _musicVolume;
            
            Debug.Log("[AudioSystemManager] Initialized");
        }
        
        public void PlayMusic(AudioClip clip)
        {
            if (_musicSource != null && clip != null)
            {
                _musicSource.clip = clip;
                _musicSource.Play();
            }
        }
        
        public void PlaySFX(AudioClip clip, Vector3 position)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, position, _sfxVolume * _masterVolume);
            }
        }
        
        public void StopMusic()
        {
            if (_musicSource != null)
            {
                _musicSource.Stop();
            }
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
        }
        
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
            {
                _musicSource.volume = _musicVolume * _masterVolume;
            }
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public void Shutdown()
        {
            StopMusic();
            Debug.Log("[AudioSystemManager] Shutting down...");
        }
    }
}