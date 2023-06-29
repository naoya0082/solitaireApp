using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance;

        [Header("Audio sources: ")] 
        [SerializeField] private AudioSource _mainSource;
        [SerializeField] private AudioSource _buttonClickSource;

        [Space(10f)]
        [Header("Audio clips: ")] 
        [SerializeField] private AudioClip _move;
        [SerializeField] private AudioClip _moveToPack;
        [SerializeField] private AudioClip _moveToWaste;
        [SerializeField] private AudioClip _error;
        [SerializeField] private AudioClip _win;
        [SerializeField] private AudioClip _buttonClick;
        [SerializeField] private AudioClip _hint;
        [SerializeField] private AudioClip _windowClose;
        [SerializeField] private AudioClip _windowOpen;
        [SerializeField] private List<AudioClip> _moveToAce;

        private void Awake()
        {
            Instance = this;
        }
        
        private void OnDestroy()
        {
            Instance = null;
        }

        /// <summary>
        /// Mute/unmute audio sources.
        /// </summary>
        public void SetMute(bool state)
        {
            _mainSource.mute = state;
            _buttonClickSource.mute = state;
        }

        public void Play(int audioClipIndex)
        {
            if (!Enum.IsDefined(typeof(AudioType), audioClipIndex))
            {
                Debug.LogError($"Index {audioClipIndex} is not defined in enum AudioType");
                return;
            }

            AudioType type = (AudioType)audioClipIndex;

            PlayInternal(type);
        }
        
        public void Play(string audioClipName)
        {
            if (Enum.TryParse(audioClipName, out AudioType type))
            {
                if (!Enum.IsDefined(typeof(AudioType), type))
                {
                    Debug.LogError($"Index {audioClipName} is not defined in enum AudioType");
                    return;
                }
            }

            PlayInternal(type);
        }
        
        public void Play(AudioType type)
        {
            PlayInternal(type);
        }

        /// <summary>
        /// Play audio clip by type.
        /// </summary>
        private void PlayInternal(AudioType type)
        {
            switch (type)
            {
                case AudioType.ButtonClick:
                    _mainSource.PlayOneShot(_buttonClick);
                    break;
                case AudioType.Error:
                    _mainSource.PlayOneShot(_error);
                    break;
                case AudioType.Move:
                    _mainSource.PlayOneShot(_move);
                    break;
                case AudioType.MoveToWaste:
                    _mainSource.PlayOneShot(_moveToWaste);
                    break;
                case AudioType.MoveToPack:
                    _mainSource.PlayOneShot(_moveToPack);
                    break;
                case AudioType.MoveToAce:
                    PlayMoveToAceLogic();
                    break;
                case AudioType.Win:
                    _mainSource.PlayOneShot(_win);
                    break;
                case AudioType.Hint:
                    _mainSource.PlayOneShot(_hint);
                    break;
                case AudioType.WindowClose:
                    _mainSource.PlayOneShot(_windowClose);
                    break;
                case AudioType.WindowOpen:
                    _mainSource.PlayOneShot(_windowOpen);
                    break;
            }
        }

        private float _moveToAceResetTime = 1f;
        private float _moveToAceLastPlayTime;
        private int _moveToAceClipIdToPlay = 0;
        
        private void PlayMoveToAceLogic()
        {
            if (Time.time - _moveToAceLastPlayTime < _moveToAceResetTime)
            {
                if (_moveToAceClipIdToPlay + 1 < _moveToAce.Count)
                {
                    _moveToAceClipIdToPlay++;
                }
            }
            else
            {
                _moveToAceClipIdToPlay = 0;
            }
            
            _moveToAceLastPlayTime = Time.time;
            AudioClip clip = _moveToAce[_moveToAceClipIdToPlay];

            if (clip != null)
            {
                _mainSource.PlayOneShot(_moveToAce[_moveToAceClipIdToPlay]);
            }
            else
            {
                Debug.LogError($"MoveToAce index {_moveToAceClipIdToPlay} not define any AudioClip.");
            }
        }

        public enum AudioType
        {
            ButtonClick = 0,
            Error = 1,
            Move,
            MoveToWaste,
            MoveToPack,
            MoveToAce,
            Win,
            Hint,
            WindowClose,
            WindowOpen,
        }
    }
}