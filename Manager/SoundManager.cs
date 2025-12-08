using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// 관리할 사운드의 카테고리를 정의합니다.
/// </summary>
public enum SoundType
{
    BGM,    // 배경 음악
    EFFECT, // 효과음 (단발성 사운드)
    MaxCount, // AudioSource 배열 크기 정의용
}

/// <summary>
/// 사운드 관리자
/// 게임의 모든 사운드 재생, 볼륨 조절, Addressables 리소스 로딩 및 믹싱을 담당하는 MonoSingleton 클래스입니다.
/// </summary>
public class SoundManager : MonoSingleton<SoundManager>
{
    // ====== Inspector References ======

    [Tooltip("전체 사운드 믹싱을 위한 AudioMixerGroup을 연결합니다.")]
    [SerializeField] private AudioMixerGroup audioMixerGroup;

    // ====== Runtime State & Caches ======

    private Dictionary<string, AudioClip> m_clipDic = new(); // 효과음 캐시 딕셔너리 (경로 -> AudioClip)
    private AudioSource[] m_audioSources = new AudioSource[(int)SoundType.MaxCount]; // BGM, EFFECT 전용 AudioSource 배열

    // Note: m_currentBGMValue, m_currentEffectValue 필드는 사용되지 않으므로 제거하거나 볼륨 저장용으로 재정의 필요

    // ----------------------------------------------------------------------
    // ## Initialization
    // ----------------------------------------------------------------------

    /// <summary>
    /// SoundManager를 초기화하고 SoundType별 AudioSource를 생성합니다.
    /// </summary>
    public override void Init()
    {
        base.Init();

        // 믹서 그룹을 찾기 위한 SoundType 이름 배열
        string[] soundNames = System.Enum.GetNames(typeof(SoundType));

        // SoundType.MaxCount까지 반복하여 AudioSource 생성 (MaxCount 제외)
        for (int i = 0; i < (int)SoundType.MaxCount; i++)
        {
            // 1. AudioSource GameObject 생성 및 부모 설정
            GameObject go = new GameObject { name = soundNames[i] };
            go.transform.parent = transform;

            // 2. AudioSource 컴포넌트 추가 및 배열에 저장
            AudioSource source = go.AddComponent<AudioSource>();
            m_audioSources[i] = source;

            // 3. AudioMixerGroup 연결
            // AudioMixer에서 해당 SoundType 이름과 일치하는 그룹을 찾아 연결합니다.
            AudioMixerGroup[] matchingGroups = audioMixerGroup.audioMixer.FindMatchingGroups(soundNames[i]);

            if (matchingGroups == null || matchingGroups.Length == 0)
            {
                // AudioMixer에 BGM 또는 EFFECT 그룹이 정의되어 있는지 확인해야 합니다.
                Logger.LogError($"[SoundManager] AudioMixer Group '{soundNames[i]}' not found.");
                continue;
            }

            source.outputAudioMixerGroup = matchingGroups[0];
        }

        // BGM 전용 AudioSource는 루프 설정
        m_audioSources[(int)SoundType.BGM].loop = true;
    }

    // ----------------------------------------------------------------------
    // ## Core Playback & Resource Management
    // ----------------------------------------------------------------------

    /// <summary>
    /// 경로를 통해 사운드를 비동기로 로드/캐시하고 재생합니다.
    /// </summary>
    /// <param name="path">Addressables 에셋 경로</param>
    /// <param name="type">BGM 또는 EFFECT</param>
    /// <param name="pitch">재생 피치 (기본 1.0f)</param>
    public async UniTask Play(string path, SoundType type = SoundType.EFFECT, float pitch = 1f)
    {
        AudioClip audioClip = await GetOrAddAudioClip(path, type);

        if (audioClip != null)
        {
            Play(audioClip, type, pitch);
        }
    }

    /// <summary>
    /// 사운드 클립을 Addressables에서 로드하거나 캐시된 클립을 가져옵니다.
    /// BGM은 매번 새로 로드하며, EFFECT는 딕셔너리에 캐시됩니다.
    /// </summary>
    /// <param name="path">Addressables 에셋 경로</param>
    /// <param name="type">사운드 타입</param>
    /// <returns>로드되거나 캐시된 AudioClip</returns>
    private async UniTask<AudioClip> GetOrAddAudioClip(string path, SoundType type = SoundType.EFFECT)
    {
        AudioClip audioClip = null;

        if (type == SoundType.BGM)
        {
            // BGM은 보통 잦은 교체가 없고 메모리에서 유지될 가능성이 높아 캐시 없이 로드하는 방식 사용
            // (하지만 AddressableManager의 LoadAssetAndCacheAsync가 사용되었으므로 실제로는 캐시됩니다.)
            audioClip = await AddressableManager.Instance.LoadAssetAndCacheAsync<AudioClip>(path);
        }
        else // EFFECT
        {
            // 효과음은 딕셔너리(m_clipDic)에서 먼저 검색하여 재사용 시도
            if (m_clipDic.TryGetValue(path, out audioClip) == false)
            {
                // 캐시된 클립이 없으면 Addressables에서 로드 및 딕셔너리에 추가
                audioClip = await AddressableManager.Instance.LoadAssetAndCacheAsync<AudioClip>(path);
                if (audioClip != null)
                {
                    m_clipDic.Add(path, audioClip);
                }
            }
        }

        if (audioClip == null)
            Logger.LogWarning($"[SoundManager] AudioClip Missing! Addressable path: {path}");

        return audioClip;
    }

    /// <summary>
    /// 이미 로드된 AudioClip을 사용하여 사운드를 재생합니다.
    /// </summary>
    /// <param name="audioClip">재생할 AudioClip 인스턴스</param>
    /// <param name="soundType">사운드 타입 (BGM 또는 EFFECT)</param>
    /// <param name="pitch">재생 피치</param>
    public void Play(AudioClip audioClip, SoundType soundType, float pitch = 1f)
    {
        if (audioClip == null)
            return;

        if (soundType == SoundType.BGM)
        {
            // BGM 재생: 현재 재생 중인 클립이 있으면 중지 후 새 클립 재생
            AudioSource audioSource = m_audioSources[(int)SoundType.BGM];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else // EFFECT 재생
        {
            // 효과음 재생: PlayOneShot을 사용하여 여러 사운드가 동시에 재생될 수 있도록 합니다.
            AudioSource audioSource = m_audioSources[(int)SoundType.EFFECT];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    // ----------------------------------------------------------------------
    // ## Volume Control and User Settings
    // ----------------------------------------------------------------------

    /// <summary>
    /// AudioMixer의 Master 볼륨을 조절합니다.
    /// </summary>
    /// <param name="value">설정할 볼륨 값 (Mixer 파라미터로 전달됨)</param>
    /// <param name="mute">음소거(Mute/Unmute) 기능에서 호출된 경우 UserData 저장을 건너뜁니다.</param>
    public void MasterValue(float value, bool mute = false)
    {
        // Mixeer 파라미터는 "Master" 이름으로 설정되어 있다고 가정
        audioMixerGroup.audioMixer.SetFloat("Master", value);

        if (mute == false)
        {
            // UserSettingData의 볼륨 값을 업데이트하고 저장 (UserSettingData가 Singleton이라고 가정)
            UserSettingData.Instance.userSettingOption.masterSoundValue = value;
            UserSettingData.Instance.SaveData();
        }
    }

    /// <summary>
    /// BGM AudioSource의 볼륨을 조절하고 사용자 설정에 저장합니다.
    /// </summary>
    /// <param name="bgmValue">설정할 볼륨 값 (0.0f ~ 1.0f)</param>
    public void BGMValue(float bgmValue)
    {
        audioMixerGroup.audioMixer.SetFloat(SoundType.BGM.ToString(), bgmValue);
        UserSettingData.Instance.userSettingOption.bgmSoundValue = bgmValue;
        UserSettingData.Instance.SaveData();
    }

    /// <summary>
    /// 효과음 AudioSource의 볼륨을 조절하고 사용자 설정에 저장합니다.
    /// </summary>
    /// <param name="effectValue">설정할 볼륨 값 (0.0f ~ 1.0f)</param>
    public void EffectValue(float effectValue)
    {
        audioMixerGroup.audioMixer.SetFloat(SoundType.EFFECT.ToString(), effectValue);
        UserSettingData.Instance.userSettingOption.effectSoundValue = effectValue;
        UserSettingData.Instance.SaveData();
    }

    /// <summary>
    /// Master 볼륨을 0.0f로 설정하여 전체 사운드를 음소거합니다. (사용자 설정 저장 건너뜀)
    /// </summary>
    public void Mute()
    {
        MasterValue(0.0f, true);
    }

    /// <summary>
    /// Master 볼륨을 기존에 저장된 값으로 복원하여 음소거를 해제합니다. (사용자 설정 저장 건너뜀)
    /// </summary>
    public void UnMute()
    {
        // 저장된 Master 볼륨 값을 불러와 복원합니다.
        MasterValue(UserSettingData.Instance.userSettingOption.masterSoundValue, true);
    }

    // ----------------------------------------------------------------------
    // ## Cleanup
    // ----------------------------------------------------------------------

    /// <summary>
    /// 모든 AudioSource를 중지하고 클립을 제거하며, 효과음 캐시 딕셔너리를 비웁니다.
    /// (씬 전환 시나 게임 종료 시 호출하기 좋습니다.)
    /// </summary>
    public void Clear()
    {
        foreach (var audio in m_audioSources)
        {
            if (audio != null)
            {
                audio.clip = null;
                audio.Stop();
            }
        }

        // 효과음 캐시 릴리즈는 AddressableManager에서 별도로 처리해야 함
        // (m_clipDic의 모든 클립을 AddressableManager.Instance.UnloadAssetAndCacheAsync<AudioClip>(path) 등으로 처리)
        m_clipDic.Clear();
    }

    /// <summary>
    /// Todo: 미사용 음성(보이스) 클립을 Addressables에서 해제하는 로직 (현재는 빈 함수)
    /// </summary>
    public void ReleseVoice()
    {
        // 1. 딕셔너리의 모든 키를 복사합니다.
        List<string> keysToUnload = new List<string>(m_clipDic.Keys);

        foreach (var key in keysToUnload)
        {
            // 2. AddressableManager를 통해 리소스 해제
            // LoadAssetAndCacheAsync를 썼다면 UnloadAssetAndCacheAsync를 쓰는 것이 더 안전합니다.
            AddressableManager.Instance.UnloadAsset(key);

            // 3. 딕셔너리에서 제거
            m_clipDic.Remove(key);
        }

        keysToUnload.Clear();
    }
}