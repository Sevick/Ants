using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameArea : MonoBehaviour
{
    [Tooltip("Material to set on espisodeBegin")]
    public Material episodeBeginGroundMaterial;

    private Transform foodDispenser;
    private Transform waterDispenser;
    private GameObject ground;
    private Renderer groundRenderer;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void awake() {
        foodDispenser = this.transform.Find("FoodDispenser");
        waterDispenser = this.transform.Find("WaterDispenser");
        ground = this.transform.Find("Ground").gameObject;
        groundRenderer = ground.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void reset() {
        foodDispenser.position = new Vector3(Random.Range(-50, 50), 5f, Random.Range(-50, 50)) + this.transform.position;
        waterDispenser.position = new Vector3(Random.Range(-50, 50), 5f, Random.Range(-50, 50)) + this.transform.position;

        foodDispenser.gameObject.GetComponent<ResourceProvider>().reset();
        waterDispenser.gameObject.GetComponent<ResourceProvider>().reset();

        // Swap ground material for a bit to indicate we scored.
        StartCoroutine(GroundChangeMaterial(episodeBeginGroundMaterial, 0.3f));
    }

    /// <summary>
    /// Swap ground material, wait time seconds, then swap back to the regular material.
    /// </summary>
    IEnumerator GroundChangeMaterial(Material mat, float time) {
        Material savedMaterial = groundRenderer.material;
        groundRenderer.material = mat;
        yield return new WaitForSeconds(time);
        groundRenderer.material = savedMaterial;
    }
}
