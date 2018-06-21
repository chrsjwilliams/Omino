using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Clip Library")]
public class ClipLibrary : ScriptableObject {

    [SerializeField] private AudioClip blueprintPlaced;
    public AudioClip BlueprintPlaced { get { return blueprintPlaced; } }
    
    [SerializeField] private AudioClip buildingFall;
    public AudioClip BuildingFall { get { return buildingFall; } }
    
    [SerializeField] private AudioClip defeat;
    public AudioClip Defeat { get { return defeat; } }
    
    [SerializeField] private AudioClip illegalPlay;
    public AudioClip IllegalPlay { get { return illegalPlay; } }
    
    [SerializeField] private AudioClip[] levelTracks;
    public AudioClip[] LevelTracks { get { return levelTracks; } }
    
    [SerializeField] private AudioClip menuSong;
    public AudioClip MenuSong { get { return menuSong; } }
    
    [SerializeField] private AudioClip pieceDestroyed;
    public AudioClip PieceDestroyed { get { return pieceDestroyed; } }
    
    [SerializeField] private AudioClip pieceDrawn;
    public AudioClip PieceDrawn { get { return pieceDrawn; } }
    
    [SerializeField] private AudioClip piecePicked;
    public AudioClip PiecePicked { get { return piecePicked; } }
    
    [SerializeField] private AudioClip piecePlaced;
    public AudioClip PiecePlaced { get { return piecePlaced; } }
    
    [SerializeField] private AudioClip pieceRotated;
    public AudioClip PieceRotated { get { return pieceRotated; } }
    
    [SerializeField] private AudioClip prodLevelUp;
    public AudioClip ProdLevelUp { get { return prodLevelUp; } }
    
    [SerializeField] private AudioClip resourceGained;
    public AudioClip ResourceGained { get { return resourceGained; } }
    
    [SerializeField] private AudioClip shieldHit;
    public AudioClip ShieldHit { get { return shieldHit; } }
    
    [SerializeField] private AudioClip silence;
    public AudioClip Silence { get { return silence; } }

    [SerializeField] private AudioClip structureClaimed;
    public AudioClip StructureClaimed { get { return structureClaimed; } }

    [SerializeField] private AudioClip uiButtonPressed;
    public AudioClip UIButtonPressed { get { return uiButtonPressed; } }
    
    [SerializeField] private AudioClip uiClick;
    public AudioClip UIClick { get { return uiClick; } }
    
    [SerializeField] private AudioClip uiReadyOn;
    public AudioClip UIReadyOn { get { return uiReadyOn; } }
        
    [SerializeField] private AudioClip uiReadyOff;
    public AudioClip UIReadyOff { get { return uiReadyOff; } }
    
    [SerializeField] private AudioClip victory;
    public AudioClip Victory { get { return victory; } }
    
    [SerializeField] private AudioClip warning;
    public AudioClip Warning { get { return warning; } }
}
