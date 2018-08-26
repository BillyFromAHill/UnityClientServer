using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private NetworkManager _networkManager;


	void Start () {
		_networkManager = new NetworkManager();
        _networkManager.Start();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
