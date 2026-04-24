using UnityEngine;

public class BackgroundMovement : MonoBehaviour
{
    RectTransform m_RectTransform;
    [SerializeField] RectTransform targetRect;

    [SerializeField] float multi;

    public void Awake()
    {
        if(m_RectTransform == null) m_RectTransform = GetComponent<RectTransform>();
    }

    public void Update()
    {
        m_RectTransform.anchoredPosition = targetRect.anchoredPosition * 0.0001f * multi;
    }
}
