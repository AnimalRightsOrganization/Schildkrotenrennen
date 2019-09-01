using UnityEngine;
using System.Collections;
using eeGames.Actor;
using UnityEngine.UI;

public class GenerateCoin : MonoBehaviour {

    public Transform Target;
    public Text Score;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
        if(Input.GetMouseButtonDown(0))
        {
            GameObject coin = Instantiate(Resources.Load("coin")) as GameObject;
            var coinActor = coin.AddComponent<Actor>();
          
            coinActor.DoPositionTween(Input.mousePosition, Target.position, 1f, 0, LeanTweenType.easeInExpo);
            coin.transform.SetParent(transform, false);
            UpdateScore();
        }

	}

    private int _score = 5;
    public void UpdateScore() 
    {
        _score++;
        Score.text = "{" +_score.ToString() + "}";
    }
}
