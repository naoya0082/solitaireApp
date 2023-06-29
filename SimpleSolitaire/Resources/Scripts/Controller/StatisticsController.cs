using SimpleSolitaire.Controller;
using SimpleSolitaire.Model.Enum;
using System;
using UnityEngine;
using Text = UnityEngine.UI.Text;
using Toggle = UnityEngine.UI.Toggle;

public class StatisticsController : MonoBehaviour
{
    public static StatisticsController Instance { get; private set; }

    public Action SaveStat { get; set; }
    public Action GetStat { get; set; }
    public Action<long> IncreaseGamesTime { get; set; }
    public Action PlayedGames { get; set; }
    public Action<long> PlayedGamesTime { get; set; }
    public Action<long> IncreaseScore { get; set; }
    public Action IncreaseWonGames { get; set; }
    public Action AverageTime { get; set; }
    public Action<long> BestTime { get; set; }
    public Action AverageScore { get; set; }
    public Action<long> BestMoves { get; set; }
    public Action Moves { get; set; }

    public DeckRule CurrentStatisticRule { get; set; }

    [SerializeField]
    private CardLogic _cardLogicComponent;

    [Header("Rule toggles")]
    [SerializeField]
    private Toggle _oneDrawRuleToggle;
    [SerializeField]
    private Toggle _threeDrawRuleToggle;

    [Space(10f)]

    private long _gameTimeAmount;
    [Header("Statistics UI")]
    public Text GameTimeAmountText;
    public long GameTimeAmount
    {
        get { return _gameTimeAmount; }
        set
        {
            _gameTimeAmount = value;
            GameTimeAmountText.text = ConvertLongToTimeFormat(_gameTimeAmount);
        }
    }

    private long _timeForAllPlayedGames;
    public long TimeForAllPlayedGames
    {
        get { return _timeForAllPlayedGames; }
        set
        {
            _timeForAllPlayedGames = value;
        }
    }

    private long _averageGameTime;
    public Text AverageGameTimeText;
    public long AverageGameTime
    {
        get { return _averageGameTime; }
        set { _averageGameTime = value; AverageGameTimeText.text = ConvertLongToTimeFormat(_averageGameTime);}
    }

    private long _bestGameTime;
    public Text BestGameTimeText;
    public long BestGameTime
    {
        get { return _bestGameTime; }
        set { _bestGameTime = value; BestGameTimeText.text = ConvertLongToTimeFormat(_bestGameTime); }
    }

    private long _bestGameMoves;
    public Text BestGameMovesText;
    public long BestGameMoves
    {
        get { return _bestGameMoves; }
        set { _bestGameMoves = value; BestGameMovesText.text =_bestGameMoves.ToString(); }
    }

    private int _playedGamesAmount;
    public Text PlayedGamesAmountText;
    public int PlayedGamesAmount
    {
        get { return _playedGamesAmount; }
        set { _playedGamesAmount = value; PlayedGamesAmountText.text = _playedGamesAmount.ToString(); }
    }

    private int _wonGamesAmount;
    public Text WonGamesAmountText;
    public int WonGamesAmount
    {
        get { return _wonGamesAmount; }
        set { _wonGamesAmount = value; WonGamesAmountText.text = _wonGamesAmount.ToString(); }
    }

    private long _movesAmount;
    public Text MovesAmountText;
    public long MovesAmount
    {
        get { return _movesAmount; }
        set { _movesAmount = value; MovesAmountText.text = _movesAmount.ToString(); }
    }

    private long _allScoreAmount;
    public long AllScoreAmount
    {
        get { return _allScoreAmount; }
        set { _allScoreAmount = value; }
    }

    private long _averageScoreAmount;
    public Text AverageScoreAmountText;
    public long AverageScoreAmount
    {
        get { return _averageScoreAmount; }
        set { _averageScoreAmount = value; AverageScoreAmountText.text = _averageScoreAmount.ToString();}
    }

    private string _gameVersion;
    public Text GameVersionText;
    public string GameVersion
    {
        get { return _gameVersion; }
        set { _gameVersion = value; GameVersionText.text = $"GAME VERSION V.{_gameVersion}"; }
    }

    private DateTime _lastTime;
    private int _time;
    private readonly string _statisticOneRulePrefs = "STATISTICS_ONE_DECK_RULE";
    private readonly string _statisticThreeRulePrefs = "STATISTICS_THREE_DECK_RULE";
    private readonly string _statisticSpiderPrefs = "STATISTICS_SPIDER";
    private readonly string _statisticFreecellPrefs = "STATISTICS_FREECELL";
    
    private void Awake()
    {
        Instance = this;
        _time = 1;
        SubscribeEvents();
        if (GetStat != null) GetStat.Invoke();
    }

    private void Start()
    {
        if (AverageTime != null) AverageTime.Invoke();
        if (PlayedGames != null) PlayedGames.Invoke();
        if (AverageScore != null) AverageScore.Invoke();
    }

    private void FixedUpdate()
    {
        GameTimer();
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    private void SubscribeEvents()
    {
        SaveStat += SaveStatisticInPrefs;
        GetStat += GetStatisticFromPrefs;
        IncreaseGamesTime += IncreasePlayedGamesTime;
        PlayedGames += IncreasePlayedGamesAmount;
        PlayedGamesTime += IncreasePlayedGamesTime;
        IncreaseScore += IncreaseScoreAmount;
        IncreaseWonGames += IncreaseWonGamesAmount;
        AverageTime += GetAverageGameTime;
        AverageScore += GetAverageScore;
        Moves += IncreaseMovesAmount;
        BestMoves += SetBestWinMoves;
        BestTime += SetBestWinTime;
        _oneDrawRuleToggle.onValueChanged.AddListener(delegate { ChangeStatisticType(DeckRule.ONE_RULE); });
        _threeDrawRuleToggle.onValueChanged.AddListener(delegate { ChangeStatisticType(DeckRule.THREE_RULE); });
    }

    private void UnsubscribeEvents()
    {
        SaveStat -= SaveStatisticInPrefs;
        GetStat -= GetStatisticFromPrefs;
        IncreaseGamesTime -= IncreasePlayedGamesTime;
        PlayedGames -= IncreasePlayedGamesAmount;
        PlayedGamesTime -= IncreasePlayedGamesTime;
        IncreaseScore -= IncreaseScoreAmount;
        IncreaseWonGames -= IncreaseWonGamesAmount;
        AverageTime -= GetAverageGameTime;
        AverageScore -= GetAverageScore;
        Moves -= IncreaseMovesAmount;
        BestMoves -= SetBestWinMoves;
        BestTime -= SetBestWinTime;
        _oneDrawRuleToggle.onValueChanged.RemoveAllListeners();
        _threeDrawRuleToggle.onValueChanged.RemoveAllListeners();
    }

    /// <summary>
    /// Save statistics to <see cref="_statisticOneRulePrefs"/> prefs.
    /// </summary>
    private void SaveStatisticInPrefs()
    {
        if (_cardLogicComponent is KlondikeCardLogic logic)
        {
            switch (logic.CurrentRule)
            {
                case DeckRule.ONE_RULE:
                    SaveByRule(_statisticOneRulePrefs);
                    break;
                case DeckRule.THREE_RULE:
                    SaveByRule(_statisticThreeRulePrefs);
                    break;
            }
        }
        else if (_cardLogicComponent is SpiderCardLogic)
        {
            SaveByRule(_statisticSpiderPrefs);
        }
        else if (_cardLogicComponent is FreecellCardLogic)
        {
            SaveByRule(_statisticFreecellPrefs);
        }
    }

    private void SaveByRule(string prefsByRule)
    {
        string statistic = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}/{9}", GameTimeAmount, TimeForAllPlayedGames,
            AverageGameTime, PlayedGamesAmount, WonGamesAmount, MovesAmount,
            AllScoreAmount, AverageScoreAmount, BestGameTime, BestGameMoves);

        PlayerPrefs.SetString(prefsByRule, statistic);
    }

    /// <summary>
    /// Get all game statistic values from player prefs and parse it to variables.
    /// </summary>
    private void GetStatisticFromPrefs()
    {
        if (_cardLogicComponent is KlondikeCardLogic)
        {
            switch (CurrentStatisticRule)
            {
                case DeckRule.ONE_RULE:
                    GetByRule(_statisticOneRulePrefs);
                    break;
                case DeckRule.THREE_RULE:
                    GetByRule(_statisticThreeRulePrefs);
                    break;
            }
        }
        else if (_cardLogicComponent is SpiderCardLogic)
        {
            GetByRule(_statisticSpiderPrefs);
        }
        else if (_cardLogicComponent is FreecellCardLogic)
        {
            GetByRule(_statisticFreecellPrefs);
        }
    }

    private void GetByRule(string prefsByRule)
    {
        if (PlayerPrefs.HasKey(prefsByRule))
        {
            string statistic = PlayerPrefs.GetString(prefsByRule);
            string[] stringSeparators = new string[] { "/" };
            string[] textArray = statistic.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

            GameTimeAmount = Convert.ToInt64(textArray[0]);
            TimeForAllPlayedGames = Convert.ToInt64(textArray[1]);
            AverageGameTime = Convert.ToInt64(textArray[2]);
            PlayedGamesAmount = Convert.ToInt32(textArray[3]);
            WonGamesAmount = Convert.ToInt32(textArray[4]);
            MovesAmount = Convert.ToInt64(textArray[5]);
            AllScoreAmount = Convert.ToInt64(textArray[6]);
            AverageScoreAmount = Convert.ToInt64(textArray[7]);
            BestGameTime = Convert.ToInt64(textArray[8]);
            BestGameMoves = Convert.ToInt64(textArray[9]);
            GameVersion = GetGameVersion();
        }
        else
        {
            string statistic = string.Format("{0}/{1}/{2}/{3}/{4}/{5}/{6}/{7}/{8}/{9}", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            PlayerPrefs.SetString(prefsByRule, statistic);
            GetStatisticFromPrefs();
        }
    }

    /// <summary>
    /// Change statistics info by rule<see cref="CurrentStatisticRule"/>.
    /// </summary>
    /// <param name="rule">Deck rule type.</param>
    private void ChangeStatisticType(DeckRule rule)
    {
        if (CurrentStatisticRule == rule) return;

        CurrentStatisticRule = rule;

        GetStatisticFromPrefs();
    }

    /// <summary>
    /// Initialize current toggle by game draw rule.
    /// </summary>
    /// <param name="rule">Deck rule type.</param>
    public void InitRuleToggle(DeckRule rule)
    {
        if (rule == DeckRule.ONE_RULE)
        {
            _oneDrawRuleToggle.isOn = true;
            _threeDrawRuleToggle.isOn = false;
        }
        else
        {
            _oneDrawRuleToggle.isOn = false;
            _threeDrawRuleToggle.isOn = true;
        }
    }

    /// <summary>
    /// Increase played games amount value.
    /// </summary>
    private void IncreasePlayedGamesAmount()
    {
        PlayedGamesAmount++;
        SaveStatisticInPrefs();
    }

    /// <summary>
    /// Increase moves amount value.
    /// </summary>
    private void IncreaseMovesAmount()
    {
        MovesAmount++;
        SaveStatisticInPrefs();
    }

    /// <summary>
    /// Increase played games time amount value. 
    /// </summary>
    /// <param name="value">Time</param>
    private void IncreasePlayedGamesTime(long value)
    {
        TimeForAllPlayedGames += value;
        SaveStatisticInPrefs();
    }

    /// <summary>
    /// Get average game time value from formula "average = all time / games_amount"
    /// </summary>
    private void GetAverageGameTime()
    {
        if (_playedGamesAmount > 0)
        {
            AverageGameTime = _timeForAllPlayedGames / _playedGamesAmount;
            SaveStatisticInPrefs();
        }
    }

    /// <summary>
    /// Get average score value from formula "average = all_score / won_amount"
    /// </summary>
    private void GetAverageScore()
    {
        if (_wonGamesAmount > 0)
        {
            AverageScoreAmount = _allScoreAmount / _wonGamesAmount;
            SaveStatisticInPrefs();
        }
    }

    /// <summary>
    /// Set high score by moves.
    /// </summary>
    private void SetBestWinMoves(long value)
    {
        if (value < BestGameMoves)
        {
            BestGameMoves = value;
            SaveStatisticInPrefs();
        }
        else if (BestGameMoves == 0)
        {
            BestGameMoves = value;
            SaveStatisticInPrefs();
        }
    }

    /// <summary>
    /// Set highscore by moves.
    /// </summary>
    private void SetBestWinTime(long value)
    {
        if (value < BestGameTime)
        {
            BestGameTime = value;
            SaveStatisticInPrefs();
        }
        else if (BestGameTime == 0)
        {
            BestGameTime = value;
            SaveStatisticInPrefs();
        }
    }

    /// <summary>
    /// Increase won games amount value.
    /// </summary>
    private void IncreaseWonGamesAmount()
    {
        WonGamesAmount++;
        SaveStatisticInPrefs();
    }

    /// <summary>
    /// Increase score value.
    /// </summary>
    /// <param name="value">Score</param>
    private void IncreaseScoreAmount(long value)
    {
        AllScoreAmount += value;
        SaveStatisticInPrefs();
    }

    /// <summary>
    /// Return version of application
    /// </summary>
    /// <returns>Version</returns>
    private string GetGameVersion()
    {
        return Application.version;
    }

    /// <summary>
    /// Time counter.
    /// </summary>
    private void GameTimer()
    {
        if (_cardLogicComponent is KlondikeCardLogic logic)
        {
            if (CurrentStatisticRule != logic.CurrentRule) return;
        }
        
        double timeSpan = (DateTime.Now - _lastTime).TotalSeconds;

        if (timeSpan > _time)
        {
            _lastTime = DateTime.Now;
            GameTimeAmount++;
            SaveStatisticInPrefs();
        }
    }

    /// <summary>
    /// This method converting long value into string time format HH:mm:ss
    /// </summary>
    /// <param name="timeAmount">Time counter</param>
    /// <returns>Time in sring format HH:mm:ss</returns>
    private string ConvertLongToTimeFormat(long timeAmount)
    {
        var sec = timeAmount % 60;
        var min = (timeAmount / 60) % 60;
        var hour = timeAmount / 3600;

        return string.Format("{0,2}:{1,2}:{2,2}", hour.ToString().PadLeft(2, '0'), min.ToString().PadLeft(2, '0'), sec.ToString().PadLeft(2, '0'));
    }
}
