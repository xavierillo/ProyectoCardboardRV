using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TeleportManager : MonoBehaviour
{
    public static TeleportManager Instance;
    [Header("Teleport")]
    public Image imgFade;
    [Range(0f, 1f)] public float timeTeleport = 0.5f;
    public Transform player;
    private float playerGroundPos;

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
            // Opcional: Si deseas que persista entre escenas
            // DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerGroundPos = player.position.y;
        Fade(true);
    }

    public void Fade(bool isFadeIn)
    {
        if (isFadeIn)
        imgFade.CrossFadeAlpha(0, timeTeleport, true);
        else
        imgFade.CrossFadeAlpha(1, timeTeleport, true);
    }

    public void Teleport(Vector3 newPos)
    {
        StartCoroutine(MovePosition(newPos));
    }

    private IEnumerator MovePosition(Vector3 newPos)
    {
        Fade(false);
        yield return new WaitForSeconds(timeTeleport);
        player.position = new Vector3(newPos.x, playerGroundPos,
        newPos.z);
        yield return new WaitForSeconds(timeTeleport);
        Fade(true);
    }
}