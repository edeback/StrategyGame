using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public GameObject inGameUI;
	public Material defaultMaterial;

	private Dictionary<int, Player> playerDictionary;
    private bool doUpdate;
	private StatisticsRecorder statisticsRecorder;
	private float startTime;

	// Use this for initialization
	void Start () {
		manager = this;
        doUpdate = false;
		statisticsRecorder = new StatisticsRecorder(defaultMaterial);
		startTime = 0f;
	}

    public void StartGame(Level level)
    {
        this.level = level;
        LoadLevel();
		statisticsRecorder.Reset(playerDictionary);
        doUpdate = true;
		startTime = Time.time;
		if(inGameUI)
			inGameUI.SetActive(true);
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
		statisticsRecorder.Update(playerDictionary);
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
            EndGame(false);
        }
        else if(numPlayersWithBases == 1)
        {
            Debug.Log("Human wins!");
            EndGame(true);
        }
    }

    public void EndGame(bool won)
    {
        doUpdate = false;
        foreach(var player in playerDictionary.Values)
        {
            player.DeInit();
        }
        playerDictionary.Clear();
		statisticsRecorder.Render();
		if (inGameUI)
			inGameUI.SetActive(false);
        if (won)
		{
			victoryScreen.GetComponentInChildren<Text>().text = "Time taken: " + ((int)(Time.time - startTime)).ToString() + " seconds";
			victoryScreen.SetActive(true);
		}
		else
		{
			lossScreen.GetComponentInChildren<Text>().text = "Time taken: " + ((int)(Time.time - startTime)).ToString() + " seconds";
			lossScreen.SetActive(true);
		}
    }

	public void ClearRender()
	{
		statisticsRecorder.Reset();
	}

	public static void SwapBase(BasePiece b, Player originalPlayer, Player newPlayer)
	{
		b.Reset(newPlayer);
		newPlayer.AddBase(b);
		originalPlayer.RemoveBase(b);
	}

	public void Quit()
	{
		Application.Quit();
	}
}
