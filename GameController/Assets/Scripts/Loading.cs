using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour {

    [SerializeField] Text loadingText;    
    enum LoadingTextState { OneDot, TwoDots, ThreeDots };
    LoadingTextState textState = LoadingTextState.OneDot;

    // Update is called once per frame
    void Update () {
		
        if(textState == LoadingTextState.OneDot)
        {
            loadingText.text = "Loading.";
            textState = LoadingTextState.TwoDots;
        }
        else if (textState == LoadingTextState.TwoDots)
        {
            loadingText.text = "Loading..";
            textState = LoadingTextState.ThreeDots;
        }
        else if (textState == LoadingTextState.ThreeDots)
        {
            loadingText.text = "Loading...";
            textState = LoadingTextState.OneDot;
        }
    }
}
