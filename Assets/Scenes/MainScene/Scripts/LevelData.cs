using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class LevelData:ScriptableObject{


    
    [Serializable]
    public class TileConfig{
        [ReadOnlyWhenPlaying]public Color color;
	    [ReadOnlyWhenPlaying]public Sprite sprite;

    }


	[Header("View Data")]
	
	[ReadOnlyWhenPlaying] [Range(0, 100)] public float tileSpacing;
	[ReadOnlyWhenPlaying] public TileActor tileInstance;
    [ReadOnlyWhenPlaying] public List<TileConfig> tileConfigs;

	
	[Header("GamePlay Data")]
	[ReadOnlyWhenPlaying] public float swipeDelta = 0.5f;
	[ReadOnlyWhenPlaying] public float tileScaleTime = 0.5f;
	[ReadOnlyWhenPlaying] public float tileMovementTime = 0.5f;
	[ReadOnlyWhenPlaying] public float tileDeathTime = 0.5f;


    [Header("Model Data")]
    [ReadOnlyWhenPlaying] public int numRows;
    [ReadOnlyWhenPlaying] public int numCols;
    [ReadOnlyWhenPlaying] public int numMoves;
    [ReadOnlyWhenPlaying] public int targetScore;
    [ReadOnlyWhenPlaying] public int chainBaseScore;
    [ReadOnlyWhenPlaying] public bool useSeed;
    [ReadOnlyWhenPlaying] public int seed;
	
}
