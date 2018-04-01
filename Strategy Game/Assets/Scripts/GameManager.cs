using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlayerData
{
	public int layer;
	public Color color;
}

public class GameManager : MonoBehaviour {

	private Level level;
	public GameObject baseObject;
	public GameObject unitObject;
	public PlayerData[] playerDatas;
	public float pixelScale;
	public static GameManager manager;
    public GameObject victoryScreen;
    public GameObject lossScreen;

	private Dictionary<int, Player> playerDictionary;
    private bool doUpdate;

	// Use this for initialization
	void Start () {
		manager = this;
        doUpdate = false;
		//LoadLevel();
	}

    public void StartGame(Level level)
    {
        this.level = level;
        LoadLevel();
        doUpdate = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (!doUpdate)
            return;
		foreach(var player in playerDictionary)
		{
			player.Value.Update(Time.deltaTime);
		}
		foreach (var player in playerDictionary)
		{
			player.Value.PostUpdate();
		}
        DetermineVictory();
	}

	void LoadLevel()
	{
		playerDictionary = new Dictionary<int, Player>();
		foreach (var baseInfo in level.bases)
		{
			Player player = AddPlayer(baseInfo.player);
			GameObject baseObj = Instantiate(baseObject, baseInfo.location.transform.position, Quaternion.identity);
			baseObj.layer = 0;
			BasePiece basePiece = new BasePiece(player, baseObj);
			
			player.AddBase(basePiece);
		}
	}

	Player AddPlayer(int id)
	{
		if (playerDictionary.ContainsKey(id))
			return playerDictionary[id];

		Player player = new Player(playerDatas[id]);

		player.isNeutral = (id == 0);
		player.isHuman = (id == 1);

		playerDictionary.Add(id, player);
		return player;
	}

	public Player GetNeutralPlayer()
	{
		foreach (var player in playerDictionary.Values)
		{
			if (player.isNeutral)
				return player;
		}
		return null;
	}

	public List<Player> GetOpponents(Player p)
	{
		List<Player> opponents = new List<Player>();

		foreach(var player in playerDictionary.Values)
		{
			if(player != p)
			{
				opponents.Add(player);
			}
		}

		return opponents;
	}

    void DetermineVictory()
    {
        int numPlayersWithBases = 0;
        bool humanHasBases = false;
        foreach (var player in playerDictionary)
        {
            if (player.Value.isNeutral)
                continue;

            if (player.Value.ReadBases().Count > 0)
            {
                numPlayersWithBases++;
                if (player.Value.isHuman)
                    humanHasBases = true;
            }
        }
        if(!humanHasBases)
        {
            Debug.Log("Human loses!");
            EndGame();
        }
        else if(numPlayersWithBases == 1)
        {
            Debug.Log("Human wins!");
            EndGame();
        }
    }

    void EndGame()
    {
        doUpdate = false;
        foreach(var player in playerDictionary.Values)
        {
            player.DeInit();
        }
        playerDictionary.Clear();
        if (victoryScreen)
            victoryScreen.SetActive(true);
    }

	public static void SwapBase(BasePiece b, Player originalPlayer, Player newPlayer)
	{
		b.Reset(newPlayer);
		newPlayer.AddBase(b);
		originalPlayer.RemoveBase(b);
	}
}
