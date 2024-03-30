using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDisplay : MonoBehaviour {

    public Material eventMaterial;

    private Material defaultMaterial;
    private bool inDisplay = false;
    
    // Start is called before the first frame update
    void Start()
    {
        defaultMaterial = this.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision) {
        displayEvent();
    }

    /// Swap wall material, wait time seconds, then swap back to the regular material.
    IEnumerator SwapMaterial(Material mat, float time) {
        GetComponent<Renderer>().material = mat;
        yield return new WaitForSeconds(time); // time in sec
        GetComponent<Renderer>().material = defaultMaterial;
        inDisplay = false;
    }

    public void displayEvent() {
        if (!inDisplay) {
            inDisplay = true;
            StartCoroutine(SwapMaterial(eventMaterial, 0.5f));
        }
    }
}
