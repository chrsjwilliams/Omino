using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

public class InGameShopSceneScript : Scene<TransitionData>
{
    public bool[] humanPlayers { get; private set; }

    public static string progressFileName
    {
        get
        {
            return Application.persistentDataPath + Path.DirectorySeparatorChar +
              "progress.txt";
        }
    }

    [SerializeField]
    private GameObject backButton;
    [SerializeField]
    private GameObject optionButton;

    // Use this for initialization
    void Start()
    {

    }

    private TaskManager _tm = new TaskManager();

    /*
     *      Look at hyper mode for color selection
     * 
     */ 

    internal override void OnEnter(TransitionData data)
    {
    }

    internal override void OnExit()
    {
    }

    internal override void ExitTransition()
    {
    }

    void Update()
    {
        _tm.Update();
    }
}

