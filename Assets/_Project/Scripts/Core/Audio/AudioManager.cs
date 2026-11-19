using System.Collections.Generic;
using RelicAndCoin.Core.Events;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

namespace RelicAndCoin.Core.Audio
{
    /// <summary>
    /// Quản lý âm thanh game:
    /// - SFX: dùng ObjectPool<AudioSource> cố định, không Instantiate/Destroy trong gameplay loop (không rác GC).
    /// - BGM: hai AudioSource để crossfade mượt mà khi thay đổi bài nhạc nền.
    /// MonoBehaviour mỏng — chỉ điều khiển AudioSource, không chứa logic kinh tế/gameplay.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class AudioManager : MonoBehaviour
    {
        /// <summary>Tiện ích truy cập toàn cục (chỉ là accessor, không phải hub tin nhắn singleton).</summary>
        public static AudioManager Instance { get; private set; }

        [Header("Mixer (tùy chọn)")]
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;
        [SerializeField] private AudioMixerGroup _bgmMixerGroup;

        [Header("Pool SFX")]
        [SerializeField] [Min(1)] private int _sfxPoolSize = 16;
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 1f;

        [Header("BGM")]
        [SerializeField] [Range(0f, 1f)] private float _bgmVolume = 1f;
        [SerializeField] [Min(0f)] private float _crossfadeDuration = 1f;

        private readonly List<AudioSource> _stoppedBuffer = new();
        private readonly HashSet<AudioSource> _activeSfx = new();
        private ObjectPool<AudioSource> _sfxPool;

        private AudioSource _bgmCurrent;
        private AudioSource _bgmNext;
        private float _crossfadeTimer;
        private float _crossfadeTotal;

        private void Awake()
        {
            Instance = this;

            _bgmCurrent = gameObject.AddComponent<AudioSource>();
            _bgmNext = gameObject.AddComponent<AudioSource>();
            ConfigureSource(_bgmCurrent, _bgmMixerGroup, loop: true);
            ConfigureSource(_bgmNext, _bgmMixerGroup, loop: true);

            _sfxPool = new ObjectPool<AudioSource>(
                createFunc: CreateSfxSource,
                actionOnGet: null,
                actionOnRelease: ReleaseSfxSource,
                actionOnDestroy: DestroySfxSource,
                collectionCheck: false,
                maxSize: _sfxPoolSize
            );
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            UpdateBGM();

            // Trả các AudioSource SFX đã phát xong về pool (không new() trong Update).
            _stoppedBuffer.Clear();
            foreach (var src in _activeSfx)
                if (!src.isPlaying) _stoppedBuffer.Add(src);
            foreach (var src in _stoppedBuffer) _sfxPool.Release(src);
        }

        private AudioSource CreateSfxSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            ConfigureSource(source, _sfxMixerGroup, loop: false);
            source.playOnAwake = false;
            return source;
        }

        private static void ConfigureSource(AudioSource source, AudioMixerGroup mixerGroup, bool loop)
        {
            source.loop = loop;
            if (mixerGroup != null) source.outputAudioMixerGroup = mixerGroup;
        }

        private static void ReleaseSfxSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
        }

        private static void DestroySfxSource(AudioSource source) => Destroy(source);

        /// <summary>Phát một hiệu ứng âm thanh SFX (lấy từ pool).</summary>
        public void PlaySFX(SoundDataSO data)
        {
            PlaySFX(data, 1f);
        }

        /// <summary>Phát SFX với hệ số âm lượng bổ sung.</summary>
        public void PlaySFX(SoundDataSO data, float volumeScale)
        {
            if (data == null || data.Clip == null) return;

            // Pool cố định: nếu hết chỗ, bỏ qua phát để không spôn ra GC.
            if (_sfxPool.CountActive >= _sfxPoolSize) return;

            var source = _sfxPool.Get();
            source.clip = data.Clip;
            source.volume = _sfxVolume * data.Volume * volumeScale;
            source.pitch = data.Pitch;
            source.loop = data.Loop;
            _activeSfx.Add(source);
            source.Play();
        }

        /// <summary>Phát nhạc nền BGM, có tùy chọn crossfade mượt mà.</summary>
        public void PlayBGM(SoundDataSO data, bool crossfade = true)
        {
            if (data == null || data.Clip == null) return;

            if (!crossfade || _bgmNext.clip == data.Clip)
            {
                _bgmCurrent.clip = data.Clip;
                _bgmCurrent.volume = _bgmVolume;
                _bgmCurrent.Play();
                _bgmNext.Stop();
                _bgmNext.clip = null;
                return;
            }

            _bgmNext.clip = data.Clip;
            _bgmNext.volume = 0f;
            _bgmNext.Play();
            _crossfadeTimer = 0f;
            _crossfadeTotal = Mathf.Max(0.001f, _crossfadeDuration);
        }

        /// <summary>Dừng nhạc nền ngay lập tức.</summary>
        public void StopBGM()
        {
            _bgmCurrent.Stop();
            _bgmCurrent.clip = null;
            _bgmNext.Stop();
            _bgmNext.clip = null;
            _crossfadeTimer = 0f;
        }

        public void SetSFXVolume(float volume) => _sfxVolume = Mathf.Clamp01(volume);
        public void SetBGMVolume(float volume) => _bgmVolume = Mathf.Clamp01(volume);

        private void UpdateBGM()
        {
            if (_crossfadeTimer > 0f)
            {
                _crossfadeTimer += UnityEngine.Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(_crossfadeTimer / _crossfadeTotal);
                _bgmCurrent.volume = _bgmVolume * (1f - t);
                _bgmNext.volume = _bgmVolume * t;

                if (t >= 1f)
                {
                    _crossfadeTimer = 0f;
                    _bgmCurrent.Stop();
                    _bgmCurrent.clip = null;
                    Swap(ref _bgmCurrent, ref _bgmNext);
                }
            }
            else if (_bgmCurrent.isPlaying)
            {
                _bgmCurrent.volume = _bgmVolume;
            }
        }

        private static void Swap(ref AudioSource a, ref AudioSource b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }
    }
}
