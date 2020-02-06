using UnityEngine;

public class MenuObject : MonoBehaviour
{
    protected RectTransform rect;
    public const float movementTime = 0.3f;
    public const float offset = 1400;
    private ObjectLerper lerper;
    [SerializeField]
    protected GameObject objectPrefabToSpawn;
    protected GameObject associatedObject;
    [SerializeField]
    private Vector2 customOffset;

    // Use this for initialization
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void Load()
    {
        associatedObject = Instantiate(objectPrefabToSpawn, Services.MenuManager.menuHolder);
        associatedObject.transform.localPosition = Vector3.zero;
        rect = associatedObject.GetComponent<RectTransform>();
        lerper = rect.gameObject.AddComponent<ObjectLerper>();
    }

    public void Unload()
    {
        Destroy(associatedObject);
    }

    public virtual void Show(Vector2 pos, float delay, bool status = true)
    {
        associatedObject.SetActive(true);
        lerper.LerpTo(pos + (offset * Vector2.up) + customOffset, pos + customOffset, delay, movementTime);
    }

    public virtual void Hide(float delay)
    {
        lerper.LerpTo(rect.anchoredPosition,
            rect.anchoredPosition + (offset * Vector2.down),
            delay, movementTime);
    }
}
