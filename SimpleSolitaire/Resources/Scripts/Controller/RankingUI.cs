using System.Collections.Generic;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleSolitaire.Controller
{
    /// <summary>
    /// 週間ランキング表示UIを管理するクラス
    /// </summary>
    public class RankingUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField]
        private Transform _rankingContent;

        [SerializeField]
        private GameObject _rankingEntryPrefab;

        [SerializeField]
        private Text _playerRankText;

        [SerializeField]
        private Text _loadingText;

        [SerializeField]
        private Text _errorText;

        [Header("Settings")]
        [SerializeField]
        private int _rankingCount = 10;

        private List<GameObject> _spawnedEntries = new List<GameObject>();

        /// <summary>
        /// ランキングデータを読み込んで表示
        /// </summary>
        public async void LoadAndDisplayRankings()
        {
            if (LeaderboardManager.Instance == null || !LeaderboardManager.Instance.IsInitialized)
            {
                ShowError("Connecting to server...");
                return;
            }

            ShowLoading(true);
            ClearRankingList();

            // ランキングTop10を取得
            var rankings = await LeaderboardManager.Instance.GetWeeklyRankings(_rankingCount);

            // 自分のランキングを取得
            var playerScore = await LeaderboardManager.Instance.GetPlayerScore();

            ShowLoading(false);

            if (rankings != null)
            {
                DisplayRankings(rankings);
            }
            else
            {
                ShowError("Failed to load rankings");
            }

            DisplayPlayerRank(playerScore);
        }

        /// <summary>
        /// ランキングリストを表示
        /// </summary>
        private void DisplayRankings(List<LeaderboardEntry> rankings)
        {
            ClearRankingList();

            if (_errorText != null)
            {
                _errorText.gameObject.SetActive(false);
            }

            if (rankings == null || rankings.Count == 0)
            {
                ShowError("No rankings yet");
                return;
            }

            foreach (var entry in rankings)
            {
                CreateRankingEntry(entry);
            }
        }

        /// <summary>
        /// ランキングエントリを1行作成
        /// </summary>
        private void CreateRankingEntry(LeaderboardEntry entry)
        {
            if (_rankingEntryPrefab == null || _rankingContent == null)
            {
                Debug.LogWarning("[RankingUI] Missing prefab or content reference");
                return;
            }

            var entryObj = Instantiate(_rankingEntryPrefab, _rankingContent);
            _spawnedEntries.Add(entryObj);

            var texts = entryObj.GetComponentsInChildren<Text>();

            if (texts.Length >= 3)
            {
                // 順位
                texts[0].text = $"{entry.Rank + 1}";
                // プレイヤー名（匿名の場合はIDの先頭8文字）
                texts[1].text = GetDisplayName(entry);
                // クリアタイム
                texts[2].text = LeaderboardManager.FormatTime(entry.Score);
            }
        }

        /// <summary>
        /// 表示用のプレイヤー名を取得
        /// </summary>
        private string GetDisplayName(LeaderboardEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.PlayerName))
            {
                return entry.PlayerName;
            }

            // Player IDの先頭8文字を使用
            if (!string.IsNullOrEmpty(entry.PlayerId) && entry.PlayerId.Length > 8)
            {
                return $"Player_{entry.PlayerId.Substring(0, 8)}";
            }

            return "Anonymous";
        }

        /// <summary>
        /// 自分の順位を表示
        /// </summary>
        private void DisplayPlayerRank(LeaderboardEntry playerScore)
        {
            if (_playerRankText == null) return;

            if (playerScore != null)
            {
                _playerRankText.text = $"YOUR RANK: {playerScore.Rank + 1} ({LeaderboardManager.FormatTime(playerScore.Score)})";
            }
            else
            {
                _playerRankText.text = "YOUR RANK: --";
            }
        }

        /// <summary>
        /// ランキングリストをクリア
        /// </summary>
        private void ClearRankingList()
        {
            foreach (var entry in _spawnedEntries)
            {
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
            _spawnedEntries.Clear();
        }

        /// <summary>
        /// ローディング表示
        /// </summary>
        private void ShowLoading(bool show)
        {
            if (_loadingText != null)
            {
                _loadingText.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        private void ShowError(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
                _errorText.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            ClearRankingList();
        }
    }
}

