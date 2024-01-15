using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum Sound {
    Bgm,
    Effect,
    MaxCount //그냥 enum의 개수를 세기 위해 존재(아무것도 아님)
}
public class SoundManager : Singleton<SoundManager> {
    protected SoundManager() { }
    AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount];
    Dictionary<string, AudioClip> _audioClip = new Dictionary<string, AudioClip>();

    //private AudioSource bgmSoundSource;
    //private AudioSource effectSoundSource;

    public int bgmValue = 9;
    public int effectValue = 9;

    //public List<VisualElement> bgmList = new List<VisualElement>();
    //public List<VisualElement> effectList = new List<VisualElement>();

    /// <summary>
    /// SoundManager라는 오브젝트를 만들어서 그 아래에 Sound에 있는 타입 만큼의 오브젝트 생성 후 각각 AudioSource를 붙여줌
    /// </summary>
    public void Init() {
        GameObject root = GameObject.Find("SoundManager"); // "SoundManager라는 이름의 오브젝트를 찾아
        if (root == null) { // 없다면
            root = new GameObject { name = "SoundManager" }; // SoundManager오브젝트를 만들고 
            root.AddComponent<SoundManager>();
            Object.DontDestroyOnLoad(root); // 파괴 보호 방지

            MakeSoundManager(root);
        }
        else {
            MakeSoundManager(root);
        }
    }
    private void MakeSoundManager(GameObject root) {
        string[] soundName = System.Enum.GetNames(typeof(Sound)); // BGM Effect
        for (int i = 0; i < soundName.Length - 1; i++) { // -1 : MaxCount빼기
            GameObject soundObj = new GameObject { name = soundName[i] }; // 이를 부모로 삼는 Bgm, Effect라는 이름의 오브젝트에
            _audioSources[i] = soundObj.AddComponent<AudioSource>(); // AudioSource를 붙인다
            _audioSources[i] = soundObj.GetComponent<AudioSource>();
            soundObj.transform.parent = root.transform;
        }
        _audioSources[0] = root.transform.GetChild(0).GetComponent<AudioSource>();
        _audioSources[1] = root.transform.GetChild(1).GetComponent<AudioSource>();

        _audioSources[(int)Sound.Bgm].loop = true; // bgm 무한 반복 재생
    }

    /// <summary> Clear 함수 사용방법
    /// public void LoadScene(Define.Scene type){
    ///     Managers.Clear();
    ///     SceneManager.LoadScene(GetSceneName(type));
    /// }
    ///
    /// public void Clear() {
    ///     CurrentScene.Clear();
    /// }
    /// </summary> 신이 바뀔 때 호출하자!
    public void Clear() {
        // 재생기 전부 재생 정지, 음반 빼기
        foreach (AudioSource audioSource in _audioSources) {
            audioSource.clip = null;
            audioSource.Stop();
        }
        // 효과음 Dictionary 비우기
        _audioClip.Clear(); // 그럴일없지만 게임이 너무 오래 진행 시 _audioClip에 Dictionary가 계속 추가되어 메모리가 부족해질 수 있음
    }
    public void Play(AudioClip audioClip, Sound type = Sound.Effect, float pitch = 1.0f) { // 음원의 audioClip을 받아 재생시킴
        if (audioClip == null) {
            return;
        }
        if (type == Sound.Bgm) { // bgm음악 재생
            AudioSource audioSource = _audioSources[(int)Sound.Bgm]; // _audioSources[(int)Sound.Bgm]을 통해 재생
            if (audioSource.isPlaying) { // 만약에 재생중이라면 멈추고
                audioSource.Stop();
            }
            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (type == Sound.Effect) {
            AudioSource audioSource = _audioSources[(int)(Sound.Effect)];// _audioSources[(int)Sound.Bgm]을 통해 재생
            // 효과음은 중첩되어도 되기 때문에 if문이 없다
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip); // 원하는 클립을 중첩해서 재생 할 수 있게 PlayOneShot함수 사용
        }
    }
    /// <summary> (path)Play 함수 사용방법
    /// Managers.Sound.Play("UnityChan/univ0001", Define.Sound.Effect); 
    /// Managers.Sound.Play("UnityChan/univ0002"); // Effect 가 디폴트
    /// </summary>
    /// <param name="path">경로</param>
    /// <param name="type">Sound type</param>
    /// <param name="pitch">재생 속도</param>
    public void Play(string path, Sound type = Sound.Effect, float pitch = 1.0f) { // 음원의 path를 받아서 재생시킴
        AudioClip audioClip = GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

    private AudioClip GetOrAddAudioClip(string path, Sound type = Sound.Effect) { // path를 통해 해당 클립을 로드하고 리턴한다
        if (path.Contains("Sounds/") == false) {
            path = $"Sounds/{path}"; // Sounds 폴더 안에 저장해 주기
        }
        AudioClip audioClip = null;
        if (type == Sound.Bgm) {
            audioClip = Resources.Load(path, typeof(AudioClip)) as AudioClip;
        }
        // 효과음에 경우 매우 많이 사용하기 때문에 Dictionary에 보관해두고 가져온다
        else if (type == Sound.Effect) {
            //_audioClip에 Dictionary에 해당path(Key)가 존재하는지 확인한다 
            if (_audioClip.TryGetValue(path, out audioClip) == false) { // 만약 없다면 추가한다
                audioClip = Resources.Load(path, typeof(AudioClip)) as AudioClip;
                _audioClip.Add(path, audioClip);
            }
            // 만약 있다면 그냥 그대로 return
        }

        if (audioClip == null) {
            Debug.Log($"Missing AudioSource...{path}");
        }
        return audioClip;
    }
    public void RegulateSound(Sound type, int volume) {
        if (type == Sound.Bgm) {
            _audioSources[0].volume = (float)((volume + 1) * 0.1);
        }
        else if (type == Sound.Effect) {
            _audioSources[1].volume = (float)((volume + 1) * 0.1);
        }
        else {
            Debug.LogError("sound error...");
        }
    }
}