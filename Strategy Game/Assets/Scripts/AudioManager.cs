using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public static AudioManager manager;

	public AudioClip unitDeath;
	public AudioClip baseDestroyed;
	public AudioClip baseRepaired;
	public AudioClip baseUpgraded;
	public AudioClip uiClick;

	private AudioSource audioSource;

	// Use this for initialization
	void Start () {
		audioSource = GetComponent<AudioSource>();
		manager = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onUnitDeath()
	{
		audioSource.PlayOneShot(unitDeath);
	}

	public void onBaseDestroyed()
	{
		audioSource.PlayOneShot(baseDestroyed);
	}

	public void onBaseRepaired()
	{
		audioSource.PlayOneShot(baseRepaired);
	}

	public void onBaseUpgraded(float volume = 1.0f)
	{
		audioSource.PlayOneShot(baseUpgraded, volume);
	}

	public void onUIClick()
	{
		audioSource.PlayOneShot(uiClick, 0.5f);
	}

}
