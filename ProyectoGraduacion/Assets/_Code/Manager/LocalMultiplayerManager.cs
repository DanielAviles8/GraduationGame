using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiplayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        // Si hay al menos un mando, crea un jugador por cada mando conectado
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Vector3 spawnPos = new Vector3(i * 2, 0, 0); // separarlos un poco
            SpawnPlayer(Gamepad.all[i], spawnPos);
        }
    }

    private void SpawnPlayer(Gamepad gamepad, Vector3 position)
    {
        var playerObj = Instantiate(playerPrefab, position, Quaternion.identity);

        var controller = playerObj.GetComponent<PlayerController>();

        // Asignar ese gamepad al jugador
        //controller.SetDevice(gamepad);

        Debug.Log($"Spawneado jugador con gamepad {gamepad.displayName}");
    }
}
