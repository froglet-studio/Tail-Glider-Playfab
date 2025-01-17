using Newtonsoft.Json;
using StarWriter.Core;
using StarWriter.Utility.Singleton;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatsManager : Singleton<StatsManager>
{
    [SerializeField] List<GameObject> EndOfRoundStatContainers;
    [SerializeField] public bool nodeGame = false;

    public Dictionary<Teams, RoundStats> teamStats = new();
    public Dictionary<string, RoundStats> playerStats = new();

    bool RecordStats = true;

    public void CrystalCollected(Ship ship, CrystalProperties crystalProperties)
    {
        if (!RecordStats)
            return;

        MaybeCreateDictionaryEntries(ship.Team, ship.Player.PlayerName);

        var roundStats = teamStats[ship.Team];
        roundStats.crystalsCollected++;
        teamStats[ship.Team] = roundStats;

        roundStats = playerStats[ship.Player.PlayerName];
        roundStats.crystalsCollected++;
        playerStats[ship.Player.PlayerName] = roundStats;
    }

    public void BlockCreated(Teams creatingTeam, string creatingPlayerName, TrailBlockProperties createdTrailBlockProperties)
    {
        if (!RecordStats)
            return;

        MaybeCreateDictionaryEntries(creatingTeam, creatingPlayerName);

        var roundStats = teamStats[creatingTeam];
        roundStats.blocksCreated++;
        roundStats.blocksRemaining++;
        roundStats.volumeCreated += createdTrailBlockProperties.volume;
        roundStats.volumeRemaining += createdTrailBlockProperties.volume;
        teamStats[creatingTeam] = roundStats;

        roundStats = playerStats[creatingPlayerName];
        roundStats.blocksCreated++;
        roundStats.blocksRemaining++;
        roundStats.volumeCreated += createdTrailBlockProperties.volume;
        roundStats.volumeRemaining += createdTrailBlockProperties.volume;
        playerStats[creatingPlayerName] = roundStats;
    }

    public void BlockDestroyed(Teams destroyingTeam, string destroyingPlayerName, TrailBlockProperties destroyedTrailBlockProperties)
    {
        if (!RecordStats)
            return;

        MaybeCreateDictionaryEntries(destroyingTeam, destroyingPlayerName);
        MaybeCreateDictionaryEntries(destroyedTrailBlockProperties.trailBlock.Team, destroyedTrailBlockProperties.trailBlock.PlayerName);

        // Team Destruction Stats
        var roundStats = teamStats[destroyingTeam];
        roundStats.blocksDestroyed++;
        roundStats.friendlyBlocksDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? 1 : 0;
        roundStats.hostileBlocksDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? 0 : 1;
        roundStats.volumeDestroyed += destroyedTrailBlockProperties.volume;
        roundStats.friendlyVolumeDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? destroyedTrailBlockProperties.volume : 0;
        roundStats.hostileVolumeDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? 0 : destroyedTrailBlockProperties.volume;
        teamStats[destroyingTeam] = roundStats;

        // Team Remaining
        roundStats = teamStats[destroyedTrailBlockProperties.trailBlock.Team];
        roundStats.blocksRemaining--;
        roundStats.volumeRemaining -= destroyedTrailBlockProperties.volume;
        teamStats[destroyedTrailBlockProperties.trailBlock.Team] = roundStats;

        // Player Destruction Stats
        roundStats = playerStats[destroyingPlayerName];
        roundStats.blocksDestroyed++;
        roundStats.friendlyBlocksDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? 1 : 0;
        roundStats.hostileBlocksDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? 0 : 1;
        roundStats.volumeDestroyed += destroyedTrailBlockProperties.volume;
        roundStats.friendlyVolumeDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? destroyedTrailBlockProperties.volume : 0;
        roundStats.hostileVolumeDestroyed += destroyingTeam == destroyedTrailBlockProperties.trailBlock.Team ? 0 : destroyedTrailBlockProperties.volume;
        playerStats[destroyingPlayerName] = roundStats;

        // Player Remaining
        roundStats = playerStats[destroyedTrailBlockProperties.trailBlock.PlayerName];
        roundStats.blocksRemaining--;
        roundStats.volumeRemaining -= destroyedTrailBlockProperties.volume;
        playerStats[destroyedTrailBlockProperties.trailBlock.PlayerName] = roundStats;
    }

    public void BlockRestored(Teams restoringTeam, string restoringPlayerName, TrailBlockProperties restoredTrailBlockProperties)
    {
        if (!RecordStats)
            return;

        MaybeCreateDictionaryEntries(restoringTeam, restoringPlayerName);
        MaybeCreateDictionaryEntries(restoredTrailBlockProperties.trailBlock.Team, restoredTrailBlockProperties.trailBlock.PlayerName);

        var roundStats = teamStats[restoringTeam];
        roundStats.blocksRestored++;
        roundStats.blocksRemaining++;
        roundStats.volumeRestored += restoredTrailBlockProperties.volume;
        roundStats.volumeRemaining += restoredTrailBlockProperties.volume;
        teamStats[restoringTeam] = roundStats;

        roundStats = playerStats[restoringPlayerName];
        roundStats.blocksRestored++;
        roundStats.blocksRemaining++;
        roundStats.volumeRestored += restoredTrailBlockProperties.volume;
        roundStats.volumeRemaining += restoredTrailBlockProperties.volume;
        playerStats[restoringPlayerName] = roundStats;
    }

    public void BlockStolen(Teams stealingTeam, string stealingPlayerName, TrailBlockProperties stolenTrailBlockProperties)
    {
        if (!RecordStats)
            return;

        MaybeCreateDictionaryEntries(stealingTeam, stealingPlayerName);
        MaybeCreateDictionaryEntries(stolenTrailBlockProperties.trailBlock.Team, stolenTrailBlockProperties.trailBlock.PlayerName);

        // Team Stealing Stats
        var roundStats = teamStats[stealingTeam];
        roundStats.blocksStolen++;
        roundStats.blocksRemaining++;
        roundStats.volumeStolen += stolenTrailBlockProperties.volume;
        roundStats.volumeRemaining += stolenTrailBlockProperties.volume;
        teamStats[stealingTeam] = roundStats;

        // Player Stealing Stats
        roundStats = playerStats[stealingPlayerName];
        roundStats.blocksStolen++;
        roundStats.blocksRemaining++;
        roundStats.volumeStolen += stolenTrailBlockProperties.volume;
        roundStats.volumeRemaining += stolenTrailBlockProperties.volume;
        playerStats[stealingPlayerName] = roundStats;

        // Team Remaining
        roundStats = teamStats[stolenTrailBlockProperties.trailBlock.Team];
        roundStats.blocksRemaining--;
        roundStats.volumeRemaining -= stolenTrailBlockProperties.volume;
        teamStats[stolenTrailBlockProperties.trailBlock.Team] = roundStats;

        // Player Remaining
        roundStats = playerStats[stolenTrailBlockProperties.trailBlock.PlayerName];
        roundStats.blocksRemaining--;
        roundStats.volumeRemaining -= stolenTrailBlockProperties.volume;
        playerStats[stolenTrailBlockProperties.trailBlock.PlayerName] = roundStats;
    }

    public void AbilityActivated(Teams team, string playerName, InputEvents abilityType, float duration)
    {
        if (!RecordStats)
            return;

        MaybeCreateDictionaryEntries(team, playerName);
        RoundStats roundStats;
        switch (abilityType)
        {
            case InputEvents.FullSpeedStraightAction:
                roundStats = teamStats[team];
                roundStats.fullSpeedStraightAbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.fullSpeedStraightAbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
            case InputEvents.RightStickAction:
                roundStats = teamStats[team];
                roundStats.rightStickAbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.rightStickAbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
            case InputEvents.LeftStickAction:
                roundStats = teamStats[team];
                roundStats.leftStickAbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.leftStickAbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
            case InputEvents.FlipAction:
                roundStats = teamStats[team];
                roundStats.flipAbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.flipAbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
            case InputEvents.Button1Action:
                roundStats = teamStats[team];
                roundStats.button1AbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.button1AbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
            case InputEvents.Button2Action:
                roundStats = teamStats[team];
                roundStats.button2AbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.button2AbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
            case InputEvents.Button3Action:
                roundStats = teamStats[team];
                roundStats.button3AbilityActiveTime += duration;
                teamStats[team] = roundStats;

                roundStats = playerStats[playerName];
                roundStats.button3AbilityActiveTime += duration;
                playerStats[playerName] = roundStats;
                break;
        }
    }

    void OnEnable()
    {
        GameManager.onPlayGame += ResetStats;
        GameManager.onGameOver += OutputRoundStats;
    }

    void OnDisable()
    {
        GameManager.onPlayGame -= ResetStats;
        GameManager.onGameOver -= OutputRoundStats;
    }

    void MaybeCreateDictionaryEntries(Teams team, string playerName)
    {
        if (!teamStats.ContainsKey(team))
            teamStats.Add(team, new RoundStats());

        if (!playerStats.ContainsKey(playerName))
            playerStats.Add(playerName, new RoundStats());
    }

    public void ResetStats()
    {
        RecordStats = true;
        teamStats = new Dictionary<Teams, RoundStats>();
        playerStats = new Dictionary<string, RoundStats>();
    }

    // TODO: p1 - we probably want a UI class that talks to the stats managar and updates the UI rather than doing it in here directly
    void OutputRoundStats()
    {
        RecordStats = false;

        string statsString = "<mspace=4.5px>\n";
        statsString += "<b>Field".PadRight(38) + " | ";

        foreach (var playerName in playerStats.Keys)
        {
            statsString += playerName.PadRight(10) + " | ";
        }
        statsString += "</b>\n";

        var fieldInfoList = typeof(RoundStats).GetFields();
        foreach (var fieldInfo in fieldInfoList)
        {
            string statValues = fieldInfo.Name.PadRight(35) + " | ";

            foreach (var playerName in playerStats.Keys)
            {
                object fieldValue = fieldInfo.GetValue(playerStats[playerName]);
                statValues += fieldValue.ToString().PadRight(10) + " | ";
            }

            statsString.Substring(statsString.Length - 3);
            statsString += statValues + "\n";
        }
        statsString += "</mspace=4.5px>";

        Debug.LogWarning(statsString.ToString());
        Debug.LogWarning(JsonConvert.SerializeObject(playerStats, Formatting.Indented));
        Debug.LogWarning(JsonConvert.SerializeObject(teamStats, Formatting.Indented));

        int i = 0;
        foreach (var team in teamStats.Keys)
        {
            var container = EndOfRoundStatContainers[i];
            Debug.LogWarning($"Team Stats - Team:{team}");
            i++;
        }

        i = 0;
        foreach (var player in playerStats.Keys)
        {
            Debug.LogWarning($"PlayerStats - Player:{player}");

            var container = EndOfRoundStatContainers[i];
            container.transform.GetChild(0).GetComponent<TMP_Text>().text = player;
            container.transform.GetChild(1).GetComponent<TMP_Text>().text = playerStats[player].volumeCreated.ToString("F0");
            container.transform.GetChild(2).GetComponent<TMP_Text>().text = playerStats[player].hostileVolumeDestroyed.ToString("F0");
            //Individual Impact or Score
            container.transform.GetChild(3).GetComponent<TMP_Text>().text = (playerStats[player].volumeCreated + playerStats[player].hostileVolumeDestroyed
                                                                            - playerStats[player].friendlyVolumeDestroyed + (2 * playerStats[player].volumeStolen)).ToString("F0");

            i++;
        }

        //Calculate  and display winner
        var finalScore = teamStats[Teams.Green].volumeRemaining - teamStats[Teams.Red].volumeRemaining;
        var winner = finalScore > 0 ? "Green" : "Red";
        Debug.Log($"Winner: {winner}");
    }
}