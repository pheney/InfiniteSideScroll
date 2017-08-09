using UnityEngine;
using System.Collections;

public class IssPlayerSpawner : MonoBehaviour
{
    [Tooltip("Inspector Assigned.")]
    public GameObject playerSpawnpoint;

    public GameObject playerPrefab;
    private GameObject _player;

    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (_player == null)
            {
                _player = Instantiate(playerPrefab, playerSpawnpoint.transform.position, Quaternion.identity) as GameObject;
            }
            else
            {
                if (_player.transform.position.y < -10)
                {
                    Vector3 spawnPosition = playerSpawnpoint.transform.position;
                    spawnPosition.y = 10;
                    _player.transform.position = spawnPosition;
                    _player.GetComponent<Rigidbody>().velocity = Vector3.down;
                }
            }
        }
    }

    //	External API
    public GameObject GetPlayer()
    {
        return _player;
    }
}
