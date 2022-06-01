using UnityEngine;
using System.Collections.Generic;
using Scorpio.Config;
using Scorpio.Timer;

public class SoundManager : Singleton<SoundManager> {
	private const string SOUND_VOLUME = "__SoundVolume";		//音效音量
	private const string MUSIC_VOLUME = "__MusicVolume";		//背景音乐音量
	private const string SOUND_ENABLE = "__SoundEnable";		//音效开启
	private const string MUSIC_ENABLE = "__MusicEnable";		//背景音乐开启
	private static GameObject DefaultListener = null;			//默认的Audio Listener
	private static AudioSource DefaultSoundSource = null;		//播放音效的对象
	public class BackgroundMusic {
		public int Index { get; private set; }				//背景音乐索引
		public float Volume { get; private set; } = 1;		//背景音乐声音
		public AudioSource Music { get; private set; }		//背景音乐资源
		public BackgroundMusic(int index, AudioClip audioClip) {
			this.Index = index;
			this.Music = new GameObject("BackgroundMusic - " + index).AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(this.Music.gameObject);
            Reset(audioClip);
		}
		public void Reset(AudioClip audioClip) {
			this.Music.clip = audioClip;
			if (audioClip != null) {
				this.Music.rolloffMode = AudioRolloffMode.Linear;
				this.Music.loop = true;
				UpdateVolume();
			} else {
				this.Music.mute = true;
			}
		}
		public void SetVolume(float volume) {
			Volume = Mathf.Clamp01(volume);
			UpdateVolume();
		}
		public void UpdateVolume() {
			var volume = SoundManager.Instance.MusicVolume * Volume;
			if (SoundManager.Instance.MusicEnable && volume > 0) {
				this.Music.volume = volume;
                this.Music.mute = false;
                if (!this.Music.isPlaying)
					this.Music.Play();
			} else {
				this.Music.mute = true;
			}
		}
		public void Shutdown() {
			GameObject.Destroy (Music.gameObject);
			Music = null;
		}
	}
	private bool mSoundEnable = true;
	public bool SoundEnable {
		get { return mSoundEnable; }
		set { mSoundEnable = value; LocalGlobalConfig.SetBool(SOUND_ENABLE, mSoundEnable); }
	}
	private float mSoundVolume;
	public float SoundVolume {
		get { return mSoundVolume;  }
		set { mSoundVolume = value; LocalGlobalConfig.SetFloat(SOUND_VOLUME, mSoundVolume); }
	}
	private bool mMusicEnable = true;
	public bool MusicEnable {
		get { return mMusicEnable; }
		set { mMusicEnable = value; LocalGlobalConfig.SetBool(MUSIC_ENABLE, mMusicEnable); UpdateBackgroundVolume(); }
	}
	private float mMusicVolume;
	public float MusicVolume {
		get { return mMusicVolume; }
		set { mMusicVolume = value; LocalGlobalConfig.SetFloat(MUSIC_VOLUME, mMusicVolume); UpdateBackgroundVolume(); }
	}
	
	//所有背景音乐
	private Dictionary<int, BackgroundMusic> mBackgroundMusics = new Dictionary<int, BackgroundMusic>();
	//当前的Audio Listener
	private GameObject mListener = null;
	public void Initialize() {
		mSoundVolume = LocalGlobalConfig.GetFloat(SOUND_VOLUME, 0.5f);
		mMusicVolume = LocalGlobalConfig.GetFloat(MUSIC_VOLUME, 0.5f);
		mSoundEnable = LocalGlobalConfig.GetBool(SOUND_ENABLE, true);
		mMusicEnable = LocalGlobalConfig.GetBool(MUSIC_ENABLE, true);
		if (DefaultListener == null) {
			DefaultListener = new GameObject("__AudioListener");
			GameObject.DontDestroyOnLoad(DefaultListener);
		}
		if (DefaultSoundSource == null) {
			DefaultSoundSource = new GameObject("__SoundSource").AddComponent<AudioSource>();
			GameObject.DontDestroyOnLoad(DefaultSoundSource.gameObject);
		}
		SetListener(DefaultListener);
	}
	public void Shutdown() {
		DeleteAllBackgroundMusic();
	}
	public void SetListener(GameObject listener) {
		if (mListener == listener) { return; }
		if (mListener != null) {
			GameObject.Destroy(mListener.GetComponent<AudioListener>());
		}
		mListener = listener ?? DefaultListener;
		EngineUtil.AddComponent<AudioListener>(mListener);
	}
    private AudioSource SoundSource => DefaultSoundSource;
	public void SetBackgroundMusic(AudioClip clip) {
		SetBackgroundMusic(0, clip);
	}
	public void SetBackgroundMusic(int index, AudioClip clip) {
        BackgroundMusic music;
		if (mBackgroundMusics.TryGetValue(index, out music)) {
            music.Reset(clip);
        } else {
            mBackgroundMusics.Add(index, new BackgroundMusic(index, clip));
        }
	}
	public void SetBackgroundMusicVolume(int index, float volume) {
        BackgroundMusic music;
		if (mBackgroundMusics.TryGetValue(index, out music)) {
            music.SetVolume(volume);
        }
	}
	public BackgroundMusic GetBackgroundMusic(int index) {
		BackgroundMusic music;
		if (mBackgroundMusics.TryGetValue(index, out music))
		{
			return music;
		}
		return null;
	}
	
	public void SetAllBackgroundMusicVolume(float volume) {
		foreach (var pair in mBackgroundMusics)
			pair.Value.SetVolume(volume);
	}
	public void DeleteBackgroundMusic(int index) {
        BackgroundMusic music;
		if (mBackgroundMusics.TryGetValue(index, out music)) {
            music.Shutdown();
            mBackgroundMusics.Remove(index);
        }
	}
	public void DeleteAllBackgroundMusic() {
		foreach (var pair in mBackgroundMusics) {
			pair.Value.Shutdown();
		}
		mBackgroundMusics.Clear();
	}
	//更新背景音乐音量
	private void UpdateBackgroundVolume() {
		foreach (var pair in mBackgroundMusics)
			pair.Value.UpdateVolume();
	}
    public void PlaySound(AudioClip clip) {
        PlaySound(clip, -1, 1);
    }
	public void PlaySound(AudioClip clip, float delay) {
        PlaySound(clip, delay, 1);
	}
    public void PlaySound(AudioClip clip, float delay, float volume) {
		PlaySound(clip, delay, volume, false);
    }
	//加上 AddGameTimer 可以多线程播放声音
	public void PlaySound(AudioClip clip, float delay, float volume, bool force) {
		if (SoundVolume <= 0 || clip == null) { return; }
		if (!SoundEnable && !force) { return; }
		if (delay < 0) {
			SoundSource.PlayOneShot(clip, SoundVolume * volume);
		} else if (delay == 0) {
			LooperManager.Instance.Run((args) => { SoundSource.PlayOneShot(clip, SoundVolume * volume); }, null);
		} else {
			TimerManager.Instance.AddGameTimer(delay, (timer, args, fixedArgs) => { SoundSource.PlayOneShot(clip, SoundVolume * volume); }, null);
		}
	}
}
