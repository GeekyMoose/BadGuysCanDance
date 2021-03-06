﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public GameEvent gameOverEvent;

    public Transform[] listSpawnPoints;
    public GameObject characterPrefab;
    public CharacterData[] listCharacterData;

    [SerializeField]
    [Tooltip("Phase 2 starts as soon as this number of players is reached")]
    private int phase2StartAtNbPlayer = 5;

    [SerializeField]
    [Tooltip("Phase 3 starts as soon as this number of players is reached")]
    private int phase3StartAtNbPlayer = 3;

    private Conductor conductor;
    private int countCharacters = 8; // Hard coded to 8 characters
    private List<Character> listCharacters;
    private int playerIndexInList;
    private SnapGridCharacter player;

    private bool areAIspaned = false;
    private float spawningAIsAccumulator = 0.0f;

    [SerializeField]
    [Tooltip("Amount of time (in seconds) before the AIs spawn")]
    private float spawningAIsAfterSeconds = 6.0f;

    [SerializeField]
    private Animation gridBeatAnimation;

    [SerializeField]
    private Animation gridBarAnimation;


    public void Start()
    {
        Assert.IsTrue(this.countCharacters > 1);

        this.conductor = this.GetComponent<Conductor>();
        this.listCharacters = new List<Character>(this.countCharacters);

        Assert.IsNotNull(this.gridBeatAnimation, "Missing asset");
        Assert.IsNotNull(this.gridBarAnimation, "Missing asset");
        Assert.IsNotNull(this.listCharacterData, "Missing asset");
        Assert.IsTrue(this.listCharacterData.Length >= this.countCharacters, "Invalid asset value");
        Assert.IsNotNull(this.gameOverEvent, "Missing asset");
        Assert.IsTrue(this.listSpawnPoints.Length != 0, "Missing asset");
        Assert.IsNotNull(this.characterPrefab, "Missing asset");
        Assert.IsNotNull(this.conductor, "Missing asset");

        // Generate list characters (kind of character pool)
        for(int k = 0; k < this.countCharacters; ++k)
        {
            GameObject characterObj = Instantiate(characterPrefab);
            Assert.IsNotNull(characterObj.GetComponent<Character>(), "Invalid prefab");
            this.listCharacters.Add(characterObj.GetComponent<Character>());
            characterObj.transform.position = new Vector3(50.0f, 0.0f, 0.0f); // Hack: place outside (I had issue with SetActive to false)
            characterObj.GetComponent<Character>().GiveIdentityToThisPoorCharacter(this.listCharacterData[k]);
        }
        this.SpawnRandomPlayer();

        AkSoundEngine.PostEvent("Set_State_Phase0", gameObject);

        this.conductor.AddBrick(new Brick_SnapGridBeatAnim(this.gridBeatAnimation, this.gridBarAnimation));
    }

    private void Update()
    {
        // Hack: See SpawnRandomPlayer
        if(this.player != null && !this.player.IsPlayerControlled())
        {
            this.player.UsePlayerControls();
        }

        // Ugly but ok for now
        if (!this.areAIspaned)
        {
            this.spawningAIsAccumulator += Time.deltaTime;
            if(this.spawningAIsAccumulator >= this.spawningAIsAfterSeconds)
            {
                AkSoundEngine.PostEvent("Set_State_Phase1", gameObject);
                this.areAIspaned = true;
                this.SpawnRandomAIs();
            }
        }
    }

    public void SpawnRandomPlayer()
    {
        int randomIndex = (int)(Random.Range(0, this.listSpawnPoints.Length - 1));
        Transform randomTransform = this.listSpawnPoints[randomIndex].transform;

        this.playerIndexInList = (int)(Random.Range(0, this.listCharacters.Count - 1));
        
        this.player = this.listCharacters[this.playerIndexInList].GetComponent<SnapGridCharacter>();
        this.player.gameObject.transform.position = randomTransform.position;
        // Hack: calling UsePlayerControls() here bugs
    }

    public void SpawnRandomAIs()
    {
        for(int k = 0; k < this.countCharacters; ++k)
        {
            if(k == this.playerIndexInList)
            {
                // Bypass the player
                continue;
            }

            int index = (int)(Random.Range(0, this.listSpawnPoints.Length - 1));
            Transform randomTransform = this.listSpawnPoints[index].transform;

            this.listCharacters[k].gameObject.transform.position = randomTransform.position;

            // Add in synchro conductor
            this.conductor.AddBrick(new Brick_SnapGridMoveAI(this.listCharacters[k].GetComponent<SnapGridAIController>()));
        }
    }

    public void OnPlayerKilledEvent()
    {
        // There is only one player, so on kill, game is over
        Debug.Log("OnPlayerKilledEvent received");
        countCharacters--;
        this.gameOverEvent.Raise();
        AkSoundEngine.PostEvent("Set_State_End", gameObject);

        // Deactivate characters so that hunter cannot shoot at them in gameover menu
        for (int k = 0; k < this.listCharacters.Count; ++k)
        {
            this.listCharacters[k].enabled = false;
        }
    }

    public void OnAIKilledEvent()
    {
        Debug.Log("OnPlayerKilledEvent received");
        countCharacters--;
        if(countCharacters == this.phase2StartAtNbPlayer)
        {
            AkSoundEngine.PostEvent("Set_State_Phase2", gameObject);
        }
        else if(countCharacters == this.phase3StartAtNbPlayer)
        {
            AkSoundEngine.PostEvent("Set_State_Phase3", gameObject);
        }
    }

    public List<Character> GetListCharacters()
    {
        return this.listCharacters;
    }
}
