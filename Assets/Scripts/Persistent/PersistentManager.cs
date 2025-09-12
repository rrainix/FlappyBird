using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 144;
    }

    // For mobile
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
    }
}