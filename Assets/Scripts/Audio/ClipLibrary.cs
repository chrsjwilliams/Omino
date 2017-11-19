using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Clip Library")]
public class ClipLibrary : ScriptableObject {

    [SerializeField] private AudioClip mainTrackAudio;
    public AudioClip MainTrackAudio { get { return mainTrackAudio; } }

    [SerializeField] private AudioClip piecePlaced;
    public AudioClip PiecePlaced { get { return piecePlaced; } }

    [SerializeField] private AudioClip piecePicked;
    public AudioClip PiecePicked { get { return piecePicked; } }

    [SerializeField] private AudioClip pieceDestroyed;
    public AudioClip PieceDestroyed { get { return pieceDestroyed; } }

    [SerializeField] private AudioClip blueprintPlaced;
    public AudioClip BlueprintPlaced { get { return blueprintPlaced; } }

    [SerializeField] private AudioClip[] playAvailable;
    public AudioClip[] PlayAvailable { get { return playAvailable; } }

    [SerializeField] private AudioClip structureClaimed;
    public AudioClip StructureClaimed { get { return structureClaimed; } }

    [SerializeField] private AudioClip resourceGained;
    public AudioClip ResourceGained { get { return resourceGained; } }

}
