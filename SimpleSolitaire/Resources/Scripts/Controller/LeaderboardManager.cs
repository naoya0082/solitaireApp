using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    /// <summary>
    /// UGS Leaderboards を管理するシングルトンクラス
    /// 週間ランキングへのスコア送信・取得を行う
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        private const string WEEKLY_LEADERBOARD_ID = "weekly_clear_time";

        public bool IsInitialized { get; private set; }

        /// <summary>
        /// ランキング取得完了時のイベント
        /// </summary>
        public event Action<List<LeaderboardEntry>> OnRankingsLoaded;

        /// <summary>
        /// 自分のスコア取得完了時のイベント
        /// </summary>
        public event Action<LeaderboardEntry> OnPlayerScoreLoaded;

        /// <summary>
        /// エラー発生時のイベント
        /// </summary>
        public event Action<string> OnError;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private async void Start()
        {
            await InitializeServices();
        }

        /// <summary>
        /// UGSサービスの初期化と匿名認証
        /// </summary>
        private async Task InitializeServices()
        {
            try
            {
                // Unity Servicesの初期化
                await UnityServices.InitializeAsync();

                // 匿名認証（ユーザーアカウント不要）
                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[LeaderboardManager] Signed in as: {AuthenticationService.Instance.PlayerId}");
                }

                IsInitialized = true;
                Debug.Log("[LeaderboardManager] UGS initialized successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] Failed to initialize UGS: {e.Message}");
                OnError?.Invoke(e.Message);
            }
        }

        /// <summary>
        /// クリアタイムをLeaderboardに送信
        /// </summary>
        /// <param name="clearTimeInSeconds">クリアタイム（秒）</param>
        /// <returns>送信結果のLeaderboardEntry（失敗時はnull）</returns>
        public async Task<LeaderboardEntry> SubmitClearTime(int clearTimeInSeconds)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[LeaderboardManager] Not initialized yet.");
                return null;
            }

            try
            {
                // スコア送信（タイムが短いほど良いので、そのまま秒数を送信）
                var result = await LeaderboardsService.Instance.AddPlayerScoreAsync(
                    WEEKLY_LEADERBOARD_ID,
                    clearTimeInSeconds
                );

                Debug.Log($"[LeaderboardManager] Score submitted! Rank: {result.Rank + 1}, Time: {result.Score}s");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] Failed to submit score: {e.Message}");
                OnError?.Invoke(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 週間ランキングを取得
        /// </summary>
        /// <param name="count">取得件数（デフォルト10件）</param>
        /// <returns>ランキングエントリのリスト</returns>
        public async Task<List<LeaderboardEntry>> GetWeeklyRankings(int count = 10)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[LeaderboardManager] Not initialized yet.");
                return null;
            }

            try
            {
                var options = new GetScoresOptions { Limit = count };
                var response = await LeaderboardsService.Instance.GetScoresAsync(
                    WEEKLY_LEADERBOARD_ID,
                    options
                );

                Debug.Log($"[LeaderboardManager] Retrieved {response.Results.Count} rankings.");
                OnRankingsLoaded?.Invoke(response.Results);
                return response.Results;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] Failed to get rankings: {e.Message}");
                OnError?.Invoke(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 自分のランキング情報を取得
        /// </summary>
        /// <returns>自分のLeaderboardEntry（未登録時はnull）</returns>
        public async Task<LeaderboardEntry> GetPlayerScore()
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[LeaderboardManager] Not initialized yet.");
                return null;
            }

            try
            {
                var result = await LeaderboardsService.Instance.GetPlayerScoreAsync(WEEKLY_LEADERBOARD_ID);
                Debug.Log($"[LeaderboardManager] Player rank: {result.Rank + 1}, Time: {result.Score}s");
                OnPlayerScoreLoaded?.Invoke(result);
                return result;
            }
            catch (Exception e)
            {
                // プレイヤーがまだランキングに登録されていない場合
                if (e.Message.Contains("Entry not found") || e.Message.Contains("404"))
                {
                    Debug.Log("[LeaderboardManager] Player has no score yet.");
                    return null;
                }
                
                Debug.LogError($"[LeaderboardManager] Failed to get player score: {e.Message}");
                OnError?.Invoke(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 自分の周辺ランキングを取得（自分の前後のプレイヤー）
        /// </summary>
        /// <param name="rangeLimit">前後何人ずつ取得するか</param>
        /// <returns>周辺ランキングのリスト</returns>
        public async Task<List<LeaderboardEntry>> GetPlayerRange(int rangeLimit = 5)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[LeaderboardManager] Not initialized yet.");
                return null;
            }

            try
            {
                var options = new GetPlayerRangeOptions { RangeLimit = rangeLimit };
                var response = await LeaderboardsService.Instance.GetPlayerRangeAsync(
                    WEEKLY_LEADERBOARD_ID,
                    options
                );

                return response.Results;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LeaderboardManager] Failed to get player range: {e.Message}");
                OnError?.Invoke(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 秒数を表示用文字列に変換 (MM:SS形式)
        /// </summary>
        /// <param name="seconds">秒数</param>
        /// <returns>フォーマットされた時間文字列</returns>
        public static string FormatTime(double seconds)
        {
            int sec = (int)seconds % 60;
            int min = ((int)seconds % 3600) / 60;
            return $"{min:D2}:{sec:D2}";
        }
    }
}

