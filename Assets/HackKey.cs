using UnityEngine;
using System.Collections;
using DG.Tweening;

public enum ColorStatus { lit, unlit, error, success};
public class HackKey : MonoBehaviour
{
   
    private Color defaultColor;
    private Color successColor = new Color(0.0f, 1.0f, 0.0f);
    private Color selectedColor = new Color(0.0f, 0.0f, 1.0f);
    private Color errorColor = new Color(1.0f, 0.0f, 0.0f);
    private ColorStatus colorStatus = ColorStatus.unlit;
    private Renderer myRenderer;
    private Rigidbody rb;
    private Vector3 initialPosition;


    // Use this for initialization
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        myRenderer = gameObject.GetComponent<Renderer>();
        defaultColor = myRenderer.material.color;
        initialPosition = rb.transform.position;
        colorStatus = ColorStatus.unlit;
    }

    // Update is called once per frame
    void Update()
    {
        if (HackKeyManager.instance.jiggle)
        {
            Jiggle();
        }
    }

    public void setStatus(ColorStatus status)
    {
        colorStatus = status;
        if (status == ColorStatus.lit)
        {
            setColor(selectedColor);
        } else if (status == ColorStatus.unlit)
        {
            setColor(defaultColor);
        } else if (status == ColorStatus.error)
        {
            setColor(errorColor);
        } else if (status == ColorStatus.success)
        {
            setColor(successColor);
        }
    }

    public void setColor(Color newColor)
    {
        myRenderer.material.color = newColor;
    }


	public void TouchUp() {
		clickUp();
	}


	#if UNITY_EDITOR
    void OnMouseUp()
    {
        //clickUp();
        Jiggle();
    }
	#endif
    

    private void clickUp()
    {
        HackKeyManager.instance.KeyClicked(gameObject.GetComponent("HackKey") as HackKey);
    }

    public Color getDefaultColor()
    {
        return defaultColor;
    }

   public IEnumerator errorFlash()
    {
        if (colorStatus == ColorStatus.error)
        {
            colorStatus = ColorStatus.unlit;
            setColor(defaultColor);
        } else
        {
            colorStatus = ColorStatus.error;
            setColor(errorColor);
            yield return new WaitForSeconds(0.25f);
            StartCoroutine(errorFlash());
        }
    }

    public IEnumerator successFlash()
    {
        if (colorStatus == ColorStatus.success)
        {
            colorStatus = ColorStatus.unlit;
            setColor(defaultColor);
        }
        else
        {
            colorStatus = ColorStatus.success;
            setColor(successColor);
            yield return new WaitForSeconds(0.25f);
            StartCoroutine(successFlash());
        }
    }

    /*
     * How does jiggle work?
     * If Jiggle is true, a key is moving to a point from initial point, and back
     */
    private void Jiggle()
    {
        if (DOTween.IsTweening(rb))
        {
            Debug.Log("IS TWEENING");
        } else {
            Debug.Log("STARTING");
            rb.DOMove(RandomPoint(), Random.Range(0.5f, 0.8f)).SetRelative().SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear);
        }
    }



    private Vector3 RandomPoint()
    {
        //Get a random direction in radians
        float angle = Random.Range(0.0f, Mathf.PI * 2);

        //Create a vector with length 1
        Vector3 V = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);

        //Scaled to desired length. Maybe 3?

        V *= 1.5f;
        return V;
    }

}