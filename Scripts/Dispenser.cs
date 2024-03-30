using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dispenser : MonoBehaviour
{

    public LifeResourceType lifeResource;
    public int capacity;

    private Text textComponent;
    public ITank tank;

    public void reset() {
        tank.reset();
    }

    public LifeResourceType tankResourceType() {
        return tank.tankResourceType();
    }

    // Start is called before the first frame update
    void Start()
    {
        this.tank = new TankEndless(lifeResource, capacity, capacity, -1, -1);
        //resourceProvider = this.GetComponent<ResourceProvider>();
        textComponent = this.transform.Find("Canvas/Info").gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        //textComponent.text = resourceProvider.currentAmount + " / " + resourceProvider.volume + "\n" + resourceProvider.stats_Refilled;
    }
}
