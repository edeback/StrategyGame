using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticsRecorder {

	private static float RECORD_INTERVAL = 1;

	private Dictionary<Player, List<int>> playerStatistics;
	private float timeSinceLastRecord;
	private List<GameObject> renderObjects;
	private int maxUnits;
	private Material renderMaterial;

	public StatisticsRecorder(Material mat)
	{
		renderMaterial = mat;
	}

	public void Reset(Dictionary<int, Player> playerDictionary = null)
	{
		playerStatistics = new Dictionary<Player, List<int>>();
		maxUnits = 0;
		if (renderObjects == null)
		{
			renderObjects = new List<GameObject>();
		}
		else
		{
			foreach (GameObject obj in renderObjects)
			{
				GameObject.Destroy(obj);
			}
		}
		if(playerDictionary != null)
		{
			foreach(Player p in playerDictionary.Values)
			{
				playerStatistics[p] = new List<int>();
			}
		}
	}

	public void Update(Dictionary<int, Player> playerDictionary)
	{
		timeSinceLastRecord += Time.deltaTime;
		if(timeSinceLastRecord > RECORD_INTERVAL)
		{
			timeSinceLastRecord = 0;
			foreach (Player p in playerDictionary.Values)
			{
				int numUnits = p.ReadUnits().Count;
				playerStatistics[p].Add(numUnits);
				if (numUnits > maxUnits)
					maxUnits = numUnits;
			}
		}
	}

	public void Render()
	{
		foreach(var pair in playerStatistics)
		{
			if (pair.Key.isNeutral || pair.Value.Count < 2)
				continue;
			GameObject renderObject = new GameObject("Statistics Renderer");
			renderObject.transform.position = new Vector3(0, 0, -0.1f);
			renderObjects.Add(renderObject);
			LineRenderer renderer = renderObject.AddComponent<LineRenderer>();
			renderer.colorGradient.mode = GradientMode.Fixed;
			renderer.material = renderMaterial;
			renderer.startColor = renderer.endColor = pair.Key.data.color;
			renderer.startWidth = renderer.endWidth = .05f;
			renderer.positionCount = pair.Value.Count;
			renderer.SetPositions(ConvertPositions(pair.Value));
		}

	}

	private Vector3[] ConvertPositions(List<int> unitCounts)
	{
		float realHeight = Camera.main.orthographicSize * 2.0f;
		float graphHeight = Camera.main.orthographicSize * .7f;
		float heightOffset = graphHeight / 2.0f - graphHeight * 0.2f;
		float realWidth = realHeight * Camera.main.aspect;
		float graphWidth = realWidth * 0.9f;
		float xDist = graphWidth / unitCounts.Count;
		Vector3[] positions = new Vector3[unitCounts.Count];
		for (int i = 0; i < unitCounts.Count; i++)
		{
			positions[i] = new Vector3(i * xDist - graphWidth / 2.0f, (float)unitCounts[i] / (maxUnits > 0 ? maxUnits : 1) * graphHeight - heightOffset, -0.1f);
		}


		return positions;
	}


}
