using UnityEngine;
using System.Collections;

public class CameraRaycast : MonoBehaviour {
    private RaycastHit hit;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.tag == "HackKey")
                {
                    HackKey key = hit.transform.gameObject.GetComponent("HackKey") as HackKey;
                    key.TouchUp();
                }
            }
        }
	}
}
