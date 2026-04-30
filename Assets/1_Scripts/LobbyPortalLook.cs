using UnityEngine;

public class LobbyPortalLook : MonoBehaviour
{
    public Animator portalAnim;

    public int isSelectLevelHash;

    public void Awake()
    {
        isSelectLevelHash = Animator.StringToHash("isSelectLevel");
    }

    public void ActivePortalLook()
    {
        portalAnim.SetBool(isSelectLevelHash, true);
    }

    public void DeactivePortalLook()
    {
        portalAnim.SetBool(isSelectLevelHash, false);
    }
}
