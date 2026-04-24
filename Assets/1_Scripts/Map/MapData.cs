using UnityEngine;
using System.Collections.Generic;

public class MapData : MonoBehaviour
{
    [SerializeField] Transform _respawnSpot;
    [SerializeField] Transform _cameraPosition;
    [SerializeField] List<MapTransitionTrigger> _transitionTriggers = new List<MapTransitionTrigger>();
    [SerializeField] List<ItemObject> _items = new List<ItemObject>();
    PlayerStateData _savedState;

    public Transform RespawnSpot => _respawnSpot;
    public Transform CameraPosition => _cameraPosition;

    private void Awake()
    {
        if (_transitionTriggers != null)
        {
            foreach (var trigger in _transitionTriggers)
            {
                if (trigger != null)
                {
                    trigger.SetOwnerMap(this);
                }
            }
        }
    }

    public void SavePlayerState(GameObject player)
    {
        PlayerInput input = player.GetComponent<PlayerInput>();
        PlayerLook look = player.GetComponent<PlayerLook>();
        if (input != null && input._moveState != null && look != null)
        {
            _savedState = new PlayerStateData(input._moveState.GetType(), look.CoreColor);
        }
    }

    public void MapReset(GameObject player)
    {
        if (_items != null)
        {
            foreach (ItemObject item in _items)
            {
                if (item != null)
                {
                    item.ResetCooldown();
                }
            }
        }

        if (_savedState != null)
        {
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            PlayerLook look = player.GetComponent<PlayerLook>();

            if (playerManager != null && _savedState.SavedMovementType != null)
            {
                BaseMovementState newState = (BaseMovementState)System.Activator.CreateInstance(_savedState.SavedMovementType);
                playerManager.PlayerChangeAbillity(newState);
            }

            if (look != null)
            {
                look.SetCoreColor(_savedState.SavedCoreColor);
            }
        }
    }
}
