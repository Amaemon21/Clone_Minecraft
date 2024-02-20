using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Vector3Int _currentPlayerChunkPosition;
    [SerializeField] private float _detectionTime = 1;
    [SerializeField] private Character _playerPrefab;
    [SerializeField] private World _world;

    private Character _player;
    private Vector3Int _currentChunkCenter = Vector3Int.zero;

    public void SpawnPlayer()
    {
        if (_player != null)
            return;

        Vector3Int raycastStartposition = new Vector3Int(_world.chunkSize / 2, 100, _world.chunkSize / 2);
        RaycastHit hit;

        if (Physics.Raycast(raycastStartposition, Vector3.down, out hit, 120))
        {
            _player = Instantiate(_playerPrefab, hit.point + Vector3Int.up, Quaternion.identity);
            _player.Init(_world);
            StartCheckingTheMap();
        }
    }

    public void StartCheckingTheMap()
    {
        SetCurrentChunkCoordinates();
        StopAllCoroutines();
        StartCoroutine(CheckIfShouldLoadNextPosition());
    }

    IEnumerator CheckIfShouldLoadNextPosition()
    {
        yield return new WaitForSeconds(_detectionTime);

        if (Mathf.Abs(_currentChunkCenter.x - _player.transform.position.x) > _world.chunkSize || Mathf.Abs(_currentChunkCenter.z - _player.transform.position.z) > _world.chunkSize || (Mathf.Abs(_currentPlayerChunkPosition.y - _player.transform.position.y) > _world.chunkHeight))
            _world.LoadAdditionalChunksRequest(_player);
        else
            StartCoroutine(CheckIfShouldLoadNextPosition());
    }

    private void SetCurrentChunkCoordinates()
    {
        _currentPlayerChunkPosition = WorldDataHelper.ChunkPositionFromBlockCoords(_world, Vector3Int.RoundToInt(_player.transform.position));
        _currentChunkCenter.x = _currentPlayerChunkPosition.x + _world.chunkSize / 2;
        _currentChunkCenter.z = _currentPlayerChunkPosition.z + _world.chunkSize / 2;
    }
}