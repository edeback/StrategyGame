using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public static class Utils
{
	static Texture2D _whiteTexture;
	public static Texture2D WhiteTexture
	{
		get
		{
			if (_whiteTexture == null)
			{
				_whiteTexture = new Texture2D(1, 1);
				_whiteTexture.SetPixel(0, 0, Color.white);
				_whiteTexture.Apply();
			}

			return _whiteTexture;
		}
	}

	public static void DrawScreenRect(Rect rect, Color color)
	{
		GUI.color = color;
		GUI.DrawTexture(rect, WhiteTexture);
		GUI.color = Color.white;
	}

	public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
	{
		Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
		Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
		Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
		Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
	}

	public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
	{
		// Move origin from bottom left to top left
		screenPosition1.y = Screen.height - screenPosition1.y;
		screenPosition2.y = Screen.height - screenPosition2.y;
		// Calculate corners
		var topLeft = Vector3.Min(screenPosition1, screenPosition2);
		var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
		// Create Rect
		return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
	}
}

public class GUIHandler : MonoBehaviour {

	Vector3 startPos;
	Vector3 endPos;
	bool inSelection;

	// Use this for initialization
	void Start () {
		startPos = endPos = new Vector2(0, 0);
		inSelection = false;
	}

	private void OnGUI()
	{
		if(inSelection)
		{
			Rect rect = Utils.GetScreenRect(startPos, Input.mousePosition);
			Utils.DrawScreenRectBorder(rect, 2, Color.white);
		}
	}

	// Update is called once per frame
	void Update () {
		
		if(Input.GetMouseButtonDown(0))
		{
			startPos = Input.mousePosition;
			DeselectUnits();
			inSelection = true;
		}
		else if(Input.GetMouseButtonUp(0))
		{
			endPos = Input.mousePosition;
			SelectUnits(Camera.main.ScreenToWorldPoint(startPos), Camera.main.ScreenToWorldPoint(endPos));
			inSelection = false;
		}
		else if(Input.GetMouseButtonDown(1))
		{
			SetDestination(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}

	}

	void DeselectUnits()
	{
		GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
		foreach (var unit in playerUnits)
		{
			unit.GetComponent<Collider>().owner.SetIsSelected(false);
		}
	}

	void SelectUnits(Vector2 startPos, Vector2 endPos)
	{
		if(startPos.x > endPos.x)
		{
			float temp = startPos.x;
			startPos.x = endPos.x;
			endPos.x = temp;
		}
		if(startPos.y > endPos.y)
		{
			float temp = startPos.y;
			startPos.y = endPos.y;
			endPos.y = temp;
		}

		GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
		foreach (var unit in playerUnits)
		{
			if(unit.transform.position.x > startPos.x && unit.transform.position.x < endPos.x && unit.transform.position.y > startPos.y && unit.transform.position.y < endPos.y)
			{
				unit.GetComponent<Collider>().owner.SetIsSelected(true);
			}
		}
	}

	void SetDestination(Vector2 destination)
	{
		GameObject[] playerUnits = GameObject.FindGameObjectsWithTag("PlayerUnit");
		foreach (var u in playerUnits)
		{
			UnitPiece unit = u.GetComponent<Collider>().owner as UnitPiece;
			if(unit.GetIsSelected())
			{
				unit.SetDestination(destination);
				unit.SetIsSelected(false);
			}
		}
	}
}
