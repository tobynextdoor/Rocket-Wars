﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RocketScript : MonoBehaviour {
	private bool hasStarted = false;
	
	[SerializeField]
	public GameObject marker;
	[SerializeField]
	public GameObject partSpawn;
	
	[SerializeField]
	RocketBuilderScript rocketBuilder;
	[SerializeField]
	private float partSpawnDistance = 1f;
	private GameObject[] spawnParts = new GameObject[0];
	private GameObject lastSpawnedPart;
	
	private List<PartScript> parts = new List<PartScript>();

	void Start () {
		Camera cam = Camera.main;
		float camHeight = 2f * cam.orthographicSize;
		float camWidth = camHeight * cam.aspect;

		var position = transform.position;
		if (position.x < 0) {
			position.x = -camWidth /2 + marker.GetComponent<Renderer> ().bounds.size.x /2;
		} else {
			position.x = camWidth /2 - marker.GetComponent<Renderer> ().bounds.size.x /2;
		}
		transform.position = position;
	}

	public void spawn() {
		spawnParts = rocketBuilder.createRocket ();
	}
	
	void Update() {
		// create rocket piece by piece (if no piece spanwed yet or last piece arrived at bottom (?)
		if (parts.Count < spawnParts.Length && (!lastSpawnedPart || transform.position.y - lastSpawnedPart.transform.position.y > partSpawnDistance)) {
			var nextPart = Instantiate(spawnParts[parts.Count], partSpawn.transform.position, Quaternion.identity) as GameObject;
			nextPart.GetComponent<PartScript> ().collectable = false;
			nextPart.transform.SetParent(transform);
			nextPart.GetComponent<SpriteRenderer>().sortingLayerName = "UI Part";
			
			parts.Add(nextPart.GetComponent<PartScript>());

			lastSpawnedPart = nextPart;
		}
	}
	
	void FixedUpdate () {
		if (hasStarted) {
			parts[0].GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 100));
		}
	}
	
	public void ignite() {
		hasStarted = true;
		
		foreach (PartScript part in parts) {
			part.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
		}
		foreach (ParticleSystem ps in parts[0].GetComponentsInChildren<ParticleSystem>()) {
			ps.Play();
		}
	}

	public List<PartScript> remainingParts() {
		return parts.FindAll (part => !part.collected);
	}
	
	public GameObject findNextPartWithIdentifier(string partIdentifier) {
		if (remainingParts ().Count > 0 && remainingParts () [0].partIdentifier == partIdentifier) {
			return remainingParts () [0].gameObject;
		}
		return null;
	}
	
	public GameObject findNextPart() {
		if (remainingParts ().Count > 0) {
			return remainingParts () [0].gameObject;
		}
		return null;
	}
}
