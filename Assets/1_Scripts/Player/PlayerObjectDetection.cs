using System;
using UnityEngine;

public class PlayerObjectDetection : MonoBehaviour
{
    [Header("Obstacle Check")]
    public LayerMask obstacleLayer;
    public Action<bool> obstacleDetected;

    [Header("Collision Check")]
    public LayerMask CollisionLayer;
    public Action CollisionDetected;

    [Header("Ground Check")]
    [SerializeField] float _groundCheckDistance = 0.2f;
    [SerializeField] Vector3 _groundCheckOffset1 = Vector3.zero;
    [SerializeField] Vector3 _groundCheckOffset2 = Vector3.zero;
    [SerializeField] LayerMask _floorLayer;

    bool _isGround = false;
    public Action<bool> GroundAction;

    public void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            obstacleDetected?.Invoke(true);
        }
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & CollisionLayer) != 0)
        {
            CollisionDetected?.Invoke();
        }
    }

    #region GroundCheck
    public void CheckGroundState()
    {
        Vector3 origin1 = transform.position + _groundCheckOffset1;
        Vector3 origin2 = transform.position + _groundCheckOffset2;
        
        bool temp = _isGround;
        bool isHit1 = Physics.Raycast(origin1, Vector3.down, _groundCheckDistance, _floorLayer);
        bool isHit2 = Physics.Raycast(origin2, Vector3.down, _groundCheckDistance, _floorLayer);
        
        _isGround = isHit1 || isHit2;
        
        if (_isGround == temp) return;
        GroundAction.Invoke(_isGround);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin1 = transform.position + _groundCheckOffset1;
        Vector3 origin2 = transform.position + _groundCheckOffset2;
        Vector3 end1 = origin1 + Vector3.down * _groundCheckDistance;
        Vector3 end2 = origin2 + Vector3.down * _groundCheckDistance;

        bool isHit1 = Physics.Raycast(origin1, Vector3.down, _groundCheckDistance, _floorLayer);
        bool isHit2 = Physics.Raycast(origin2, Vector3.down, _groundCheckDistance, _floorLayer);

        Gizmos.color = isHit1 ? Color.green : Color.red;
        Gizmos.DrawLine(origin1, end1);
        Gizmos.DrawWireSphere(end1, 0.05f);

        Gizmos.color = isHit2 ? Color.green : Color.red;
        Gizmos.DrawLine(origin2, end2);
        Gizmos.DrawWireSphere(end2, 0.05f);
    }
    #endregion
}
