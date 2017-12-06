using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    public const float thickness = 0.6f;
    [SerializeField] private bool _isActive;
    public bool isActive
    {
        get { return _isActive; }
    }
    [SerializeField]
    private Sprite[] sprites;
    [SerializeField]
    private Texture[] textures;
    [SerializeField]
    private Material[] materials;
    public Coord coord { get; private set; }
    public BoxCollider2D boxCol { get; private set; }
    public BoxCollider boxCol3D { get; private set; }
    private MeshRenderer mr;
    private MeshRenderer topMr;
    private SpriteRenderer sr;
    private SpriteGlow glow;
    public Material material { get; set; }
    public Polyomino occupyingPiece { get; private set; }
    public Polyomino pieceParent { get; private set; }
    public Blueprint occupyingBlueprint { get; private set; }
    public Structure occupyingStructure { get; private set; }
    public SuperDestructorResource occupyingResource { get; private set; }
    private Color prevColor;
    private Color targetColor_;
    private Color targetColor
    {
        get { return targetColor_; }
        set
        {
            //prevColor = sr.color;
            prevColor = material.color;
            targetColor_ = value;
            changingColor = true;
            colorChangeTimeElapsed = 0;
        }
    }
    private float colorChangeTimeElapsed;
    [SerializeField]
    private float colorChangeDuration;
    private bool changingColor;
    private SpriteRenderer maskSr;

    public void Init(Coord coord_)
    {
        coord = coord_;
        //boxCol = GetComponent<BoxCollider2D>();
        //sr = GetComponentInChildren<SpriteRenderer>();
        //glow = GetComponent<SpriteGlow>();
        //maskSr = GetComponentInChildren<SpriteMask>().gameObject.GetComponent<SpriteRenderer>();
        //glow.OutlineWidth = 0;
        boxCol3D = GetComponent<BoxCollider>();
        mr = GetComponent<MeshRenderer>();
        topMr = GetComponentsInChildren<MeshRenderer>()[1];
        material = mr.material;
        transform.position = new Vector3(coord.x, coord.y, 0);
        transform.localScale = new Vector3(1, 1, thickness);
        //if ((coord.x + coord.y) % 2 == 0)
        //{
        //    sr.color = Services.GameManager.MapColorScheme[0];
            material.color = Services.GameManager.MapColorScheme[0];
        topMr.material.color = material.color;
        //}
        //else
        //{
        //    sr.color = Services.GameManager.MapColorScheme[1];
        //}
    }

    public void Init(Coord coord_, Polyomino pieceParent_)
    {
        pieceParent = pieceParent_;
        Init(coord_);
        //sr.sortingOrder += 5;
    }

	public void OnRemove(){
        //Destroy(this);
	}

    public void SetCoord(Coord newCoord)
    {
        coord = newCoord;
    }

    public void SetColor(Color color)
    {
        //sr.color = color;
        material.color = color;
        topMr.material.color = color;
    }

    public void ShiftColor(Color color)
    {
        targetColor = color;
    }

    public void SetGlowOutLine(int i)
    {
        glow.OutlineWidth = i;
    }

    public void SetGlowColor(Color color)
    {
        glow.GlowColor = color;
    }

    public void SetMaskSrAlpha(float alpha)
    {
        maskSr.color = new Color(maskSr.color.r, maskSr.color.g, maskSr.color.b, alpha);
    }

    public void ActivateTile(Player player, BuildingType buildingType)
    {
        _isActive = true;
        if (player == null)
        {
            //sr.color = Services.GameManager.SuperDestructorResourceColor;
            material.color = Services.GameManager.SuperDestructorResourceColor;
            topMr.material.color = material.color;
        }
        //else if(buildingType == BuildingType.NONE)
        else
        {
            //sr.color = player.ColorScheme[0];
            material.color = player.ColorScheme[0];
            topMr.material.color = material.color;
        }
        //else sr.color = player.ColorScheme[1];
    }

    public void SetOccupyingPiece(Polyomino piece)
    {
        occupyingPiece = piece;
    }

    public void SetOccupyingResource(SuperDestructorResource resource)
    {
        occupyingResource = resource;
    }

    public void SetOccupyingBlueprint(Blueprint blueprint)
    {
        occupyingBlueprint = blueprint;
    }

    public void SetOccupyingStructure(Structure structure)
    {
        occupyingStructure = structure;
    }

    public bool IsOccupied()
    {
        return occupyingPiece != null;
    }

    public bool PartOfExistingBlueprint()
    {
        return occupyingBlueprint != null;
    }

    public bool HasResource()
    {
        return occupyingResource != null;
    }

    public Polyomino OccupiedBy()
    {
        if(IsOccupied())
        {
            return occupyingPiece;
        }
        else
        {
            return null;
        }
    }

    private void Update()
    {
        if (changingColor) LerpToTargetColor();
    }

    void LerpToTargetColor()
    {
        colorChangeTimeElapsed += Time.deltaTime;
        //sr.color = Color.Lerp(prevColor, targetColor, colorChangeTimeElapsed / colorChangeDuration);
        material.color = Color.Lerp(prevColor, targetColor, 
            colorChangeTimeElapsed / colorChangeDuration);
        topMr.material.color = material.color;
        if (colorChangeTimeElapsed >= colorChangeDuration)
        {
            changingColor = false;
        }
    }


    public void SetSprite(int spriteIndex)
    {
        //sr.sprite = sprites[spriteIndex];
        //material.EnableKeyword("_MainTex");
        //material.SetTexture("_MainTex", textures[spriteIndex]);
        //material.mainTexture = textures[spriteIndex];
        topMr.material = materials[spriteIndex];
        topMr.material.color = material.color;
        Debug.Log("Setting sprite index to " + spriteIndex);
    }

    public void SetAlpha(float alpha)
    {
        //targetColor = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
        targetColor = new Color(material.color.r, material.color.g, material.color.b, alpha);
    }

    public void IncrementSortingOrder(int inc)
	{
		sr.sortingOrder += inc;
	}

    //public void GrowSize(float scale)
    //{
    //    transform.localScale = new Vector3(transform.localScale.x,
    //        transform.localScale.y, scale);
    //}
}
