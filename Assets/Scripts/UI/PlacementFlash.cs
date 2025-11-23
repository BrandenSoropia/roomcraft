using UnityEngine;
using UnityEngine.UI;

public class PlacementFlash : MonoBehaviour
{
    public float minAlpha = 0;
    public float maxAlpha = 1;
    public float speed = 1f;
    public Image flashIMG;
    private bool flash = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (flash == true)
        {
            float t = Mathf.PingPong(Time.time * speed, 1f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            
            Color color = flashIMG.color;
            color.a = alpha;
            flashIMG.color = color;
        }
    }

    public void ActivatePlacementFlash()
    {
        flash = true;
    }

    public void DeactivatePlacementFlash()
    {
        flash = false;
        Color color = flashIMG.color;
        color.a = 0;
        flashIMG.color = color;
    }
}
