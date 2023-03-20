using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game List", menuName = "TailGlider/GameList", order = 10)]
[System.Serializable]
public class SO_GameList : ScriptableObject
{
    public List<SO_MiniGame> GameList;
}