using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dispenser : MonoBehaviour
{

    public ResourceProvider resourceProvider;
    private Text textComponent;

    private Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        resourceProvider = this.GetComponent<ResourceProvider>();
        textComponent = this.transform.Find("Canvas/Info").gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        textComponent.text = resourceProvider.currentAmount + " / " + resourceProvider.volume + "\n" + resourceProvider.stats_Refilled;
    }
}
