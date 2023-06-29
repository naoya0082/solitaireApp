using SimpleSolitaire.Model.Config;
using SimpleSolitaire.Model.Enum;
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace SimpleSolitaire.Controller
{
	public class GameManager : MonoBehaviour
	{
		[Header("Serialized fields:")]
		[SerializeField]
		private RectTransform _bottomPanel;
		[SerializeField]
		private Animator _settingsPanelAnimator;
		
		[Header("Ads Components:")]
		public GameObject AdsBtn;
		[SerializeField]
		private GameObject _adsLayer;
		[SerializeField]
		private GameObject _watchButton;
		[SerializeField]
		private Text _adsInfoText;
		[SerializeField]
		private Text _adsDidNotLoadText;
		[SerializeField]
		private Text _adsClosedTooEarlyText;

		public string NoAdsInfoText = "DO YOU WANNA TO DEACTIVATE ALL ADS FOR THIS GAME SESSION? JUST WATCH LAST REWARD VIDEO AND INSTALL APP. THEN ADS WON'T DISTURB YOU AGAIN!";
		public string GetUndoAdsInfoText = "DO YOU WANNA TO GET FREE UNDO COUNTS? JUST WATCH REWARD VIDEO AND INSTALL APP. THEN UNDO WILL ADDED TO YOUR GAME SESSION!";

		[Header("Components:")]
		[SerializeField]
		private CardLogic _cardLogic;
		//[SerializeField]
		//private InterVideoAds _interVideoAdsComponent;
		[SerializeField]
		private CongratulationManager _congratManagerComponent;
		[SerializeField]
		private UndoPerformer _undoPerformComponent;
		[SerializeField]
		private TutorialManager _tutorialComponent;
		[SerializeField]
		private AutoCompleteManager _autoCompleteComponent;

		[Header("Layers:")]
		[SerializeField]
		private GameObject _gameLayer;
		[SerializeField]
		private GameObject _cardLayer;
		[SerializeField]
		private GameObject _winLayer;
		[SerializeField]
		private GameObject _settingLayer;
		[SerializeField]
		private GameObject _ruleLayer;
		[SerializeField]
		private GameObject _statisticLayer;
		[SerializeField]
		private GameObject _exitLayer;
		[SerializeField]
		private GameObject _continueLayer;
		[SerializeField]
		private GameObject _tutorialLayer;

		[Header("Labels:")]
		[SerializeField]
		private Text _timeLabel;
		[SerializeField]
		private Text _scoreLabel;
		[SerializeField]
		private Text _stepsLabel;
		[SerializeField]
		private Text _timeWinLabel;
		[SerializeField]
		private Text _scoreWinLabel;
		[SerializeField]
		private Text _stepsWinLabel;

		[Header("Switchers:")]
		[SerializeField]
		private SwitchSpriteComponent _soundSwitcher;
		[SerializeField]
		private SwitchSpriteComponent _autoCompleteSwitcher;
		[SerializeField]
		private SwitchSpriteComponent _orientationSwitcher;
		[SerializeField]
		private SwitchSpriteComponent _highlightDraggableSwitcher;
		
		[Space(5f)]
		[SerializeField]
		private TableLayoutGroup _settingsRef;
		
		[Header("Settings:")]
		public bool UseLoadLastGameOption;

		public int TimeCount => _timeCount;
		public int StepCount => _stepCount;
		public int ScoreCount => _scoreCount;
		
		private readonly string _appearTrigger = "Appear";
		private readonly string _disappearTrigger = "Disappear";
		private readonly string _bestScoreKey = "WinBestScore";
		private readonly string _showBottomBarKey = "ShowBar";

		private int _timeCount;
		private int _stepCount;
		private int _scoreCount;
		private Coroutine _timeCoroutine;
		private AudioController _audioController;
		
		private RewardAdsType _currentAdsType = RewardAdsType.None;

		private bool _highlightDraggableEnable;
		private bool _soundEnable;
		private bool _autoCompleteEnable;
		private bool _isBarActive;

		private void Awake()
		{
			InitializeGame();
		}

		/// <summary>
		/// Initialize game structure.
		/// </summary>
		private void InitializeGame()
		{
			Application.targetFrameRate = 300;

			_soundEnable = true;
			_autoCompleteEnable = true;
			_isBarActive = true;
			
			//InterVideoAds.RewardAction += OnRewardActionState;
			
			_cardLogic.SubscribeEvents();
			_audioController = AudioController.Instance;
			_settingsRef.StartCorner = _cardLogic.Orientation == HandOrientation.RIGHT ? TableLayoutGroup.Corner.UpperLeft : TableLayoutGroup.Corner.UpperRight;
		}

		private void Start()
		{
			InitGameState();
		}

		/// <summary>
		/// Appear window with animation.
		/// </summary>
		private void AppearWindow(GameObject window)
		{
			if (window == null)
			{
				return;
			}

			var anim = window.GetComponent<Animator>();
			
			if (anim == null)
			{
				return;
			}

			if (_audioController != null)
			{
				_audioController.Play(AudioController.AudioType.WindowOpen);
			}

			anim.SetTrigger(_appearTrigger);
		}
		
		/// <summary>
		/// Disappear window with animation.
		/// </summary>
		private void DisappearWindow(GameObject window)
		{
			if (window == null)
			{
				return;
			}

			var anim = window.GetComponent<Animator>();
			
			if (anim == null)
			{
				return;
			}

			if (_audioController != null)
			{
				_audioController.Play(AudioController.AudioType.WindowClose);
			}

			anim.SetTrigger(_disappearTrigger);
		}

		/// <summary>
		/// Show tutorial window with animation.
		/// </summary>
		public void ShowTutorialLayer()
		{
			_cardLayer.SetActive(true);
			_tutorialLayer.SetActive(true);
			AppearWindow(_tutorialLayer);
		}
		
		/// <summary>
		/// Hide tutorial layer with animation.
		/// </summary>
		public void HideTutorialLayer()
		{
			DisappearWindow(_tutorialLayer);
			
			StartCoroutine(InvokeAction(delegate
			{
				_tutorialLayer.SetActive(false);
			}, 0.42f));
		}

		/// <summary>
		/// Init new game state or show continue game panel.
		/// </summary>
		private void InitGameState()
		{
			if (UseLoadLastGameOption && _tutorialComponent.IsHasKey() && _undoPerformComponent.IsHasGame())
			{
				_cardLayer.SetActive(false);
				_continueLayer.SetActive(true);
				AppearWindow(_continueLayer);
			}
			else
			{
				InitSettingBtns();
				_cardLogic.InitCardLogic();
				_cardLogic.Shuffle(false);
				InitMenuView(false);
			}
		}

		/// <summary>
		/// Change position of bottom panel. Used for ads banner.
		/// </summary>
		/// <param name="offset"></param>
		public void InitializeBottomPanel(float offset)
		{
			_bottomPanel.anchoredPosition = new Vector2(0, offset);
		}
		
		public void OnNoAdsRewardedUser()
		{
			InitializeBottomPanel(0);
			OnClickAdsCloseBtn();
			AdsBtn.SetActive(false);
		}

		private void OnDestroy()
		{
			//InterVideoAds.RewardAction -= OnRewardActionState;
			_cardLogic.UnsubscribeEvents();
		}

		/// <summary>
		/// Initialize first state of UI elements. And first timer state.
		/// </summary>
		private void InitMenuView(bool isLoadGame)
		{
			_timeCount = isLoadGame ? _undoPerformComponent.StatesData.Time : 0;
			SetTimeLabel(_timeCount);
			_stepCount = isLoadGame ? _undoPerformComponent.StatesData.Steps : 0;
			_stepsLabel.text = _stepCount.ToString();
			_scoreCount = isLoadGame ? _undoPerformComponent.StatesData.Score : 0;
			_scoreLabel.text = _scoreCount.ToString();
			StopGameTimer();
		}

		/// <summary>
		/// Deactivate <see cref="AdsBtn"/> button if we show <see cref="InterVideoAds.ShowInterstitial"/> this ads material.
		/// </summary>
		private void InitSettingBtns()
		{
			//if (PlayerPrefs.HasKey(_interVideoAdsComponent.NoAdsKey))
			{
				AdsBtn.SetActive(false);
			}
		}

		/// <summary>
		/// Update <see cref="_timeLabel"/> view text.
		/// </summary>
		/// <param name="seconds"></param>
		private void SetTimeLabel(int seconds)
		{
			int sec = seconds % 60;
			int min = (seconds % 3600) / 60;
			_timeLabel.text = string.Format("{0,2}:{1,2}", min.ToString().PadLeft(2, '0'), sec.ToString().PadLeft(2, '0'));
		}

		/// <summary>
		/// Win game action.
		/// </summary>
		public void HasWinGame()
		{
			_cardLayer.SetActive(false);
			_winLayer.SetActive(true);

			StopGameTimer();
			_congratManagerComponent.CongratulationTextFill();
			var score = _scoreCount + Public.SCORE_NUMBER / _timeCount;
			_timeWinLabel.text = "YOUR TIME: " + _timeLabel.text;
			_scoreWinLabel.text = "YOUR SCORE: " + score;
			_stepsWinLabel.text = "YOUR MOVES: " + _stepCount;

			if (_audioController != null)
			{
				_audioController.Play(AudioController.AudioType.Win);
			}
			
			SetBestValuesToPrefs(score);

			AppearWindow(_winLayer);

			StatisticsController statisticsController = StatisticsController.Instance;
			
			statisticsController.PlayedGamesTime?.Invoke(_timeCount);
			statisticsController.AverageTime?.Invoke();
			statisticsController.IncreaseScore?.Invoke(score);
			statisticsController.IncreaseWonGames?.Invoke();
			statisticsController.BestTime?.Invoke(_timeCount);
			statisticsController.BestMoves?.Invoke(_stepCount);

			//_interVideoAdsComponent.ShowInterstitial();
		}

		/// <summary>
		/// Save to prefs best score if it need :)
		/// </summary>
		/// <param name="score">Score value</param>
		private void SetBestValuesToPrefs(int score)
		{
			if (!PlayerPrefs.HasKey(_bestScoreKey))
			{
				PlayerPrefs.SetInt(_bestScoreKey, score);
			}
			else
			{
				if (score > PlayerPrefs.GetInt(_bestScoreKey))
				{
					PlayerPrefs.SetInt(_bestScoreKey, score);
				}
			}
		}

		/// <summary>
		/// Click on new game button.
		/// </summary>
		public void OnClickWinNewGame()
		{
			DisappearWindow(_winLayer);
			StartCoroutine(InvokeAction(delegate
			{
				_winLayer.SetActive(false);
				_cardLayer.SetActive(!_statisticLayer.activeInHierarchy && !_ruleLayer.activeInHierarchy && !_winLayer.activeInHierarchy);
			}, 0.42f));

			_cardLogic.Shuffle(false);
			_undoPerformComponent.ResetUndoStates();
				StatisticsController.Instance.PlayedGames?.Invoke();
		}

		/// <summary>
		/// Click on play button in bottom setting layer.
		/// </summary>
		public void OnClickPlayBtn()
		{
			_cardLayer.SetActive(false);
			_gameLayer.SetActive(true);

			if (_cardLogic is KlondikeCardLogic klondikeLogic)
			{
				klondikeLogic.InitRuleToggles();
			}
			else if (_cardLogic is SpiderCardLogic spiderLogic)
			{
				spiderLogic.InitSuitsToggles();
			}
			else if (_cardLogic is FreecellCardLogic freecellLogic)
			{
				freecellLogic.InitFreecellToggles();
			}
			AppearWindow(_gameLayer);
		}

		#region Continue Layer
		/// <summary>
		/// Click on play button in bottom setting layer.
		/// </summary>
		private void LoadGame()
		{
			InitSettingBtns();

			_cardLogic.InitCardLogic();

			_undoPerformComponent.LoadGame();

			_cardLogic.OnNewGameStart();
			
			InitMenuView(true);
		}

		/// <summary>
		/// Start new game.
		/// </summary>
		public void OnClickContinueNoBtn()
		{
			DisappearWindow(_continueLayer);

			StartCoroutine(InvokeAction(delegate
			{
				//Uncomment if you wanna clear last game when User click No button on Continue Layer.
				//_undoPerformComponent.DeleteLastGame();
				_cardLogic.InitCardLogic();
				_cardLogic.Shuffle(false);
				_continueLayer.SetActive(false);
				_cardLayer.SetActive(true);
			}, 0.42f));
		}

		/// <summary>
		/// Continue last game.
		/// </summary>
		public void OnClickContinueYesBtn()
		{
			DisappearWindow(_continueLayer);

			StartCoroutine(InvokeAction(delegate
			{
				LoadGame();
				_continueLayer.SetActive(false);
				_cardLayer.SetActive(true);
			}, 0.42f));
		}
		#endregion

		#region Exit Layer
		/// <summary>
		/// Click on Exit button.
		/// </summary>
		public void OnClickExitBtn()
		{
			_cardLayer.SetActive(false);
			_exitLayer.SetActive(true);
			AppearWindow(_exitLayer);
		}

		/// <summary>
		/// Close <see cref="_adsLayer"/>.
		/// </summary>
		public void OnClickExitNoBtn()
		{
			DisappearWindow(_exitLayer);
			StartCoroutine(InvokeAction(delegate
			{
				_exitLayer.SetActive(false);
				_cardLayer.SetActive(true);
			}, 0.42f));
		}

		/// <summary>
		/// Quit application. Exit game.
		/// </summary>
		public void OnClickExitYesBtn()
		{
			DisappearWindow(_exitLayer);
			StartCoroutine(InvokeAction(OnQuit, 0.42f));

			void OnQuit()
			{
#if UNITY_EDITOR
				_cardLogic.SaveGameState(isTempState: true);
				EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			}
		}
		
		#endregion

		#region Ads Layer

		/// <summary>
		/// Click on NoAds button.
		/// </summary>
		public void OnClickGetUndoAdsBtn()
		{
			_currentAdsType = RewardAdsType.GetUndo;
			ShowAdsLayer();
		}

		/// <summary>
		/// Click on NoAds button.
		/// </summary>
		public void OnClickNoAdsBtn()
		{
			_currentAdsType = RewardAdsType.NoAds;
			ShowAdsLayer();
		}

		/// <summary>
		/// Appearing ads layer with information about ads type.
		/// </summary>
		private void ShowAdsLayer()
		{
			UpdateAdsInfoText(_currentAdsType);

			_cardLayer.SetActive(false);
			_adsLayer.SetActive(true);
			_adsInfoText.enabled = true;
			_adsDidNotLoadText.enabled = false;
			_adsClosedTooEarlyText.enabled = false;
			_watchButton.SetActive(true);
			AppearWindow(_adsLayer);
		}

		/// <summary>
		/// Close <see cref="_adsLayer"/>.
		/// </summary>
		public void OnClickAdsCloseBtn()
		{
			DisappearWindow(_adsLayer);
			StartCoroutine(InvokeAction(delegate
			{
				_adsLayer.SetActive(false);
				_cardLayer.SetActive(true);
			}, 0.42f));
		}

		/// <summary>
		/// Close <see cref="_adsLayer"/>.
		/// </summary>
		public void OnWatchAdsBtnClick()
		{
			switch (_currentAdsType)
			{
				case RewardAdsType.GetUndo:
					//_interVideoAdsComponent.ShowGetUndoAction();
                    break;
				case RewardAdsType.NoAds:
					//_interVideoAdsComponent.NoAdsAction();
					break;
			}
		}

		/// <summary>
		/// Call result of watched reward video.
		/// </summary>
		public void OnRewardActionState(RewardAdsState state, RewardAdsType type)
		{
			DisappearWindow(_adsLayer);
			bool infoText = false;
			bool closedText = false;
			bool notLoadedText = false;
			switch (state)
			{
				case RewardAdsState.TOO_EARLY_CLOSE:
					closedText = true;
					break;
				case RewardAdsState.DID_NOT_LOADED:
					notLoadedText = true;
					break;
			}
			StartCoroutine(InvokeAction(delegate
			{
				_adsLayer.SetActive(true);
				_adsInfoText.enabled = infoText;
				_adsDidNotLoadText.enabled = notLoadedText;
				_adsClosedTooEarlyText.enabled = closedText;
				_watchButton.SetActive(false);
				_cardLayer.SetActive(false);
				AppearWindow(_adsLayer);
			}, 0.42f));
		}

		public void UpdateAdsInfoText(RewardAdsType type)
		{
			switch (type)
			{
				case RewardAdsType.NoAds:
					_adsInfoText.text = NoAdsInfoText;
					break;
				case RewardAdsType.GetUndo:
					_adsInfoText.text = GetUndoAdsInfoText;
					break;
			}
		}
		#endregion

		#region Rule Layer
		/// <summary>
		/// Click on rule button.
		/// </summary>
		public void OnClickSettingLayerRuleBtn()
		{
			StartCoroutine(InvokeAction(delegate { OnClickSettingLayerCloseBtn(); Invoke(nameof(OnRuleAppearing), 0.42f); }, 0f));
		}

		/// <summary>
		/// Close <see cref="_ruleLayer"/>.
		/// </summary>
		public void OnClickRuleBackBtn()
		{
			DisappearWindow(_ruleLayer);
			StartCoroutine(InvokeAction(delegate { _ruleLayer.SetActive(false); OnClickSettingBtn(); }, 0.42f));
		}

		/// <summary>
		/// Show <see cref="_ruleLayer"/>.
		/// </summary>
		private void OnRuleAppearing()
		{
			_ruleLayer.SetActive(true);
			AppearWindow(_ruleLayer);
		}
		#endregion

		#region Settings Layer
		/// <summary>
		/// Click on settings button.
		/// </summary>
		public void OnClickSettingBtn()
		{
			_cardLayer.SetActive(false);
			_settingLayer.SetActive(true);
			AppearWindow(_settingLayer);
		}

		/// <summary>
		/// Close <see cref="_settingLayer"/>.
		/// </summary>
		public void OnClickSettingLayerCloseBtn()
		{
			DisappearWindow(_settingLayer);
			StartCoroutine(InvokeAction(delegate
			{
				_settingLayer.SetActive(false);
				_cardLayer.SetActive(!_statisticLayer.activeInHierarchy && !_ruleLayer.activeInHierarchy);
			}, 0.42f));
		}
		#endregion

		#region Statistics Layer
		/// <summary>
		/// Click on statistics button.
		/// </summary>
		public void OnClickStatisticBtn()
		{
			StartCoroutine(InvokeAction(delegate { OnClickSettingLayerCloseBtn(); Invoke(nameof(OnStatisticAppearing), 0.42f); }, 0f));
		}

		/// <summary>
		/// Call animation which appear statistics popup.
		/// </summary>
		private void OnStatisticAppearing()
		{
			_statisticLayer.SetActive(true);
			AppearWindow(_statisticLayer);
		}

		/// <summary>
		/// Close <see cref="_statisticLayer"/>.
		/// </summary>
		public void OnClickStatisticLayerCloseBtn()
		{
			DisappearWindow(_statisticLayer);
			StartCoroutine(InvokeAction(delegate
			{
				if (_cardLogic is KlondikeCardLogic logic)
				{
					StatisticsController.Instance.InitRuleToggle(logic.CurrentRule);
				}

				_statisticLayer.SetActive(false); OnClickSettingBtn();
			}, 0.42f));
		}
		#endregion

		#region Game Layer
		/// <summary>
		/// Click on random button.
		/// </summary>
		public void OnClickModalRandom()
		{
			DisappearWindow(_gameLayer);
			StartCoroutine(InvokeAction(delegate
			{
				StatisticsController.Instance.PlayedGames?.Invoke();
				_cardLogic.OnNewGameStart();
				_gameLayer.SetActive(false);
				_cardLayer.SetActive(true);
				_cardLogic.Shuffle(false);
				_undoPerformComponent.ResetUndoStates();
			}, 0.42f));
		}

		/// <summary>
		/// Click on replay button.
		/// </summary>
		public void OnClickModalReplay()
		{
			DisappearWindow(_gameLayer);
			StartCoroutine(InvokeAction(delegate
			{
				StatisticsController.Instance.PlayedGames?.Invoke();
				_cardLogic.OnNewGameStart();
				_gameLayer.SetActive(false);
				_cardLayer.SetActive(true);
				_cardLogic.Shuffle(true);
				_undoPerformComponent.ResetUndoStates();
			}, 0.42f));
		}

		/// <summary>
		/// Close <see cref="_gameLayer"/>.
		/// </summary>
		public void OnClickModalClose()
		{
			DisappearWindow(_gameLayer);
			StartCoroutine(InvokeAction(delegate { _gameLayer.SetActive(false); _cardLayer.SetActive(true); }, 0.42f));
		}
		#endregion		

		/// <summary>
		/// Call action via time.
		/// </summary>
		/// <param name="action">Delegate.</param>
		/// <param name="time">Time for invoke.</param>
		/// <returns></returns>
		private IEnumerator InvokeAction(Action action, float time)
		{
			yield return new WaitForSeconds(time);

			if (action != null)
			{
				action();
			}
		}

		/// <summary>
		/// Increase <see cref="_stepCount"/> value and start timer <see cref="GameTimer"/> if count == 1.
		/// </summary>
		public void CardMove()
		{
			_stepCount++;
			StatisticsController.Instance.Moves?.Invoke();

			_stepsLabel.text = _stepCount.ToString();
			if (_stepCount >= 1 && _timeCoroutine == null)
			{
				_timeCoroutine = StartCoroutine(GameTimer());
			}
		}

		/// <summary>
		/// Reset all view and states.
		/// </summary>
		public void RestoreInitialState()
		{
			InitMenuView(false);
		}

		/// <summary>
		/// Update score value <see cref="_scoreCount"/> and view text <see cref="_scoreLabel"/> on UI. 
		/// </summary>
		/// <param name="value">Score</param>
		public void AddScoreValue(int value)
		{
			_scoreCount += value;
			if (_scoreCount < 0)
			{
				_scoreCount = 0;
			}
			_scoreLabel.text = _scoreCount.ToString();
		}

		/// <summary>
		/// Click on sound switch button.
		/// </summary>
		public void OnClickSoundSwitch()
		{
			_soundEnable = !_soundEnable;
			_soundSwitcher.UpdateSwitchImg(_soundEnable);

			if (_audioController != null)
			{
				_audioController.SetMute(!_soundEnable);
			}
		}

		/// <summary>
		/// Click on hand orientation switch button.
		/// </summary>
		public void OnClickOrientationSwitch()
		{
			_cardLogic.Orientation = _cardLogic.Orientation == HandOrientation.RIGHT ? HandOrientation.LEFT : HandOrientation.RIGHT;
			_cardLogic.SetHandOrientation();
			_orientationSwitcher.UpdateSwitchImg(_cardLogic.Orientation != HandOrientation.LEFT);

			_settingsRef.StartCorner = _cardLogic.Orientation == HandOrientation.RIGHT ? TableLayoutGroup.Corner.UpperLeft : TableLayoutGroup.Corner.UpperRight;
		}

		/// <summary>
		/// Click on auto complete off/on switch button.
		/// </summary>
		public void OnClickAutoCompleteEnablingSwitch()
		{
			_autoCompleteEnable = !_autoCompleteEnable;
			_autoCompleteComponent.SetEnableAutoCompleteFeature(_autoCompleteEnable);
			_autoCompleteSwitcher.UpdateSwitchImg(_autoCompleteEnable);
		}
		
		public void OnClickHighlightDraggableSwitch()
		{
			_cardLogic.HighlightDraggable = !_cardLogic.HighlightDraggable;
			for (int i = 0; i < _cardLogic.AllDeckArray.Length; i++)
			{
				_cardLogic.AllDeckArray[i].UpdateBackgroundColor();
			}
			_highlightDraggableSwitcher.UpdateSwitchImg(_cardLogic.HighlightDraggable);
		}


		/// <summary>
		/// Start game timer.
		/// </summary>
		private IEnumerator GameTimer()
		{
			while (true)
			{
				yield return new WaitForSeconds(1.0f);
				_timeCount++;
				if (_timeCount % 30 == 0)
				{
					AddScoreValue(Public.SCORE_OVER_THIRTY_SECONDS_DECREASE);
				}
				SetTimeLabel(_timeCount);
			}
		}

		/// <summary>
		/// Stop game timer.
		/// </summary>
		private void StopGameTimer()
		{
			if (_timeCoroutine != null)
			{
				StopCoroutine(_timeCoroutine);
				_timeCoroutine = null;
			}
		}
		
		public void OnApplicationFocus(bool state)
		{
			if (!_cardLogic.IsGameStarted)
			{
				Debug.LogWarning($"Game does not started.");
				return;
			}
			
			if (!state)
			{
				_cardLogic.SaveGameState(isTempState: true);
			}
			else
			{
				_undoPerformComponent.Undo(removeOnlyState: true);
			}
		}

		public void OnApplicationPause(bool state)
		{
			if (!_cardLogic.IsGameStarted)
			{
				Debug.LogWarning($"Game does not started.");
				return;
			}

			if (state)
			{
				_cardLogic.SaveGameState(isTempState: true);
			}
			else
			{
				_undoPerformComponent.Undo(removeOnlyState: true);
			}
		}

		/// <summary>
		/// Show/hide bottoms bar with animation.
		/// </summary>
		public void TryShowBar()
		{
			_isBarActive = !_isBarActive;
			_settingsPanelAnimator.SetBool(_showBottomBarKey, _isBarActive);
		}
	}
}