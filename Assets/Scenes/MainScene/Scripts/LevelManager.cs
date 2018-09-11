using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Advertisements;


[DisallowMultipleComponent]
[RequireComponent(typeof(GameBoard))]
public class LevelManager : MonoBehaviour{
    
    
    
    [SerializeField] private LevelList data;
    [Range(0,100)][SerializeField] private int maxTries;
    
    private void Awake(){

        gB = GetComponent<GameBoard>();
        currentTries = maxTries;
        //check which level user last achieved
        if ((currentLevel = PlayerPrefs.GetInt(currentLevelString, -1)) == -1){
            currentLevel = 0;
        }
        
    }
    

    
    public LevelData getCurrentLevel(){
        return data.levels[currentLevel];
    }



    public void onGameEnd(bool won){

        
        if (won){
            Debug.Log("won game");
            //increment game index
            //load the selected preset   
            currentLevel = (currentLevel + 1) % data.levels.Length;

            PlayerPrefs.SetInt(currentLevelString,currentLevel);
            gB.resetGame(data.levels[currentLevel]);
        }
        else if(currentTries > 0){
            
            --currentTries;
            Debug.Log("using tries reamining is "+currentTries);
            gB.resetGame(data.levels[currentLevel]);
            
        }
        else{
            //TODO add ad logic here
            gB.resetGame(data.levels[currentLevel]);

        }
        
    }

    private void OnDestroy(){
        data = null;
        gB = null;
    }
    
    private GameBoard gB;
    private int currentTries;
    private int currentLevel;
    private const string currentLevelString = "currentLevel";

}
