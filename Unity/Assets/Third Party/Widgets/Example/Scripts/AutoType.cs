using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoType : MonoBehaviour
{

    public float letterPause = 0.1f;
    private string message;
    public Text Typer;

    public bool AutoPlay = false;
    // Use this for initialization

    void Start()
    {
        message = Typer.text;
        Typer.text = "";
        if (AutoPlay)
        {
            StartTyping();
        }
    }
    public void StartTyping() 
    {
        Typer.gameObject.SetActive(true);
        Typer.text = "";
        StartCoroutine(TypeText());
    }
    IEnumerator TypeText()
    {
        Typer.text = "_";
        yield return new WaitForSeconds(.2f);
        Typer.text = "";
        yield return new WaitForSeconds(.2f);
        Typer.text = "_";
        yield return new WaitForSeconds(.2f);
        Typer.text = "";
        yield return new WaitForSeconds(.2f);
        Typer.text = "_";
        yield return new WaitForSeconds(.2f);
        Typer.text = "";
        yield return new WaitForSeconds(.2f);
        Typer.text = "_";
        yield return new WaitForSeconds(.2f);
        Typer.text = "";
        yield return new WaitForSeconds(.2f);
        Typer.text = "_";
        yield return new WaitForSeconds(.2f);
        Typer.text = "";


        foreach (char letter in message.ToCharArray())
        {
            Typer.text += letter;
            yield return null;
            yield return new WaitForSeconds(letterPause);
        }
        yield return new WaitForSeconds(5);
        StartTyping();
    }

    public void StopTyping() 
    {
        StopAllCoroutines();
        Typer.gameObject.SetActive(false);
    }
}