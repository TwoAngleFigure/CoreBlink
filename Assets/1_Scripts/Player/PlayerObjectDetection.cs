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

    [Header("Interaction")]
    [SerializeField] private LayerMask _interactionLayer;
    public Action<IInteractable> InteractionEnter;
    public Action<IInteractable> InteractionExit;

    [Header("Detection Collider")]
    [Tooltip("감지에 사용되는 콜라이더. 컷신 등에서 비활성화하여 상호작용/장애물 감지를 차단합니다.")]
    [SerializeField] private Collider _detectionCollider;

    [Header("Ground Check")]
    [SerializeField] float _groundCheckDistance = 0.2f;
    [SerializeField] Vector3 _groundCheckOffset1 = Vector3.zero;
    [SerializeField] Vector3 _groundCheckOffset2 = Vector3.zero;
    [SerializeField] LayerMask _floorLayer;

    bool _isGround = false;
    public Action<bool> GroundAction;

    /// <summary>
    /// 감지 콜라이더의 활성/비활성을 설정합니다.
    /// 컷신, UI 전환 등 상호작용을 차단해야 할 때 사용합니다.
    /// </summary>
    public void SetDetectionEnabled(bool isEnabled)
    {
        if (_detectionCollider != null)
        {
            _detectionCollider.enabled = isEnabled;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            obstacleDetected?.Invoke(true);
        }

        // 상호작용 오브젝트 감지
        if (((1 << other.gameObject.layer) & _interactionLayer) != 0)
        {
            InteractionTrigger trigger = other.GetComponent<InteractionTrigger>();
            if (trigger != null && trigger.Interactable != null)
            {
                trigger.ShowHint();
                InteractionEnter?.Invoke(trigger.Interactable);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        // 상호작용 영역 이탈
        if (((1 << other.gameObject.layer) & _interactionLayer) != 0)
        {
            InteractionTrigger trigger = other.GetComponent<InteractionTrigger>();
            if (trigger != null && trigger.Interactable != null)
            {
                trigger.HideHint();
                InteractionExit?.Invoke(trigger.Interactable);
            }
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
