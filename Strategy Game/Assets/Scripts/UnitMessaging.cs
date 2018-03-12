using UnityEngine.EventSystems;
using UnityEngine;

public interface IUnitMessageTarget : IEventSystemHandler
{
	void SelectMessage(Vector2 start, Vector2 end);
}
