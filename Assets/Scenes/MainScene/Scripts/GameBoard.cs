using System.Collections;
using System.Collections.Generic;
using Match3;



#if UNITY_EDITOR
using UnityEditor;
#endif


using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;


[DisallowMultipleComponent]
[RequireComponent(typeof(UserInput))]
public class GameBoard : MonoBehaviour,UserInputEventHandler{




	[SerializeField] private LevelData defaultData;


	private void OnValidate(){
		Assert.IsNotNull(defaultData,"attach a default data for testing");
		
		currentData = defaultData;

	}


	private void Awake(){


		myTransform = transform;
		userInput = GetComponent<UserInput>();	


	}

	private void Start(){
		LevelData level = GetComponent<LevelManager>().getCurrentLevel();
		resetGame(level);
	}

	public void resetGame(LevelData gameData){
		
		
		deleteTileActors();

		boardModel = null;
		if(userInput!=null)userInput.unregisterInputHandler(this);
		scoreGui = movesRemaining = 0;

		
		if (gameData != null){
			currentData = gameData;
		}
		else{
			currentData = defaultData;
		}
		
		if (currentData.useSeed){
			Random.InitState(currentData.seed);
		}
		
		
		//controller work
		boardModel = new BoardModel(currentData.tileConfigs.Count,currentData.numRows,currentData.numCols,currentData.numMoves,currentData.targetScore,currentData.chainBaseScore);
		createTilesFromModel(boardModel);
		
		
		userInput.registerInputHandler(this);
		movesRemaining = currentData.numMoves;

	}




	
	
	private void OnGUI(){
		GUIStyle myStyle = new GUIStyle();        
		myStyle.fontSize = 20;
		myStyle.normal.textColor = Color.black;
		
		GUILayout.BeginHorizontal();
		GUILayout.Label("Score "+scoreGui,myStyle);	
		GUILayout.Space(20);
		
		GUILayout.Label("Moves Remaining "+movesRemaining,myStyle);
		GUILayout.Space(20);

		
		
		GUILayout.Label("Target "+currentData.targetScore,myStyle);
		GUILayout.EndHorizontal();
		
	}



	//C# interface requirments :( making mmebers public 
	public void onInputDisable(){
	}

	public void onInputEnable(){
	}
	
	public void onInputBegin(){
		
	
		Assert.IsNull(selectedTile);
		TileActor t  = userInput.getActorUnderPointer(20.0f);
		if(t == null)return;
		
		selectedTile = t;
		Color32 tC = selectedTile.getColor();
		
		selectedTile.setHighlight(new Color(tC.r,tC.g,tC.b,0.5f));

	}

	public void onInputHold(){
			
		if(selectedTile == null)return;
		Debug.DrawLine(selectedTile.transform.position,userInput.getPointerPosInWorldSpace(),Color.black);

	}
	

	delegate void OncompleteBoardStabilization();

	private IEnumerator _tileMovmentAndStabilization(TileActor first,TileActor second,float moveTime,OncompleteBoardStabilization callback){
		
		float timer = 0;
		if (moveTime <= 0) moveTime = Mathf.Epsilon;


		Vector3 firstPos = first.transform.localPosition;
		Vector3 secondPos = second.transform.localPosition;
		
		//move sprites
		while (timer <= moveTime){
			float factor = Match3.Utils.smoothStop2(timer / moveTime);
			
			first.transform.localPosition = Vector3.Lerp(firstPos,secondPos,factor );
			second.transform.localPosition = Vector3.Lerp(secondPos, firstPos, factor);
			timer += Time.deltaTime;
			yield return null;
		}

		//snap positions
		first.transform.localPosition = secondPos;
		second.transform.localPosition = firstPos;
		yield return null;
		
	
		
		
		

		
		Chains c = boardModel.deleteChains();
		TileModel[] tilesDeleted = c.toArray();

		
		
		//update gui variables
		--movesRemaining;

		while (tilesDeleted.Length > 0){


			
			//delete chained actors
			yield return StartCoroutine(_deleteTileActors(tilesDeleted, currentData.tileDeathTime));
			BoardModel.TileShiftData [] tilesShift = boardModel.settleTiles();

			
			
			//updating gui
			scoreGui += c.getScore();

			//make cookies fall
			yield return StartCoroutine(_fallTiles(tilesShift));			
						
			//add new cookies
//			boardModel.print("before adding");
			List<TileModel> addedTiles = boardModel.addTilesTillfull();
//			Debug.Log("tiles added "+addedTiles.Count);
//			boardModel.print("after adding");
			yield return StartCoroutine(_addNewTileActors(addedTiles,currentData.tileScaleTime));
			

			//next iteration setup here
			c = boardModel.deleteChains();
			tilesDeleted = c.toArray();


		}

		

		
		checkGameOver();
		
		callback();
		
	}


	private void checkGameOver(){
	
		
		//winning case,go to next level
		if (scoreGui >= currentData.targetScore){

			
			GetComponent<LevelManager>().onGameEnd(true);
		}
		//losing case,game over
		else if (movesRemaining <= 0){

			GetComponent<LevelManager>().onGameEnd(false);
			
		}
		
		
	}
	
	IEnumerator _addNewTileActors(List<TileModel> addedTiles,float aliveTime){


		if(addedTiles.Count == 0)yield break;
		
		TileActor[] actors = new TileActor[addedTiles.Count];
		
		for (int i = 0; i < addedTiles.Count; ++i){
			actors[i] = createTileActor(addedTiles[i]);
			actors[i].transform.localScale = Vector3.zero;
		}
		
		
		
		
		float timer = 0;
		
		if (aliveTime <= 0)aliveTime = Mathf.Epsilon;
	
		while (timer <= aliveTime){
			for (int i = 0; i < addedTiles.Count; ++i){
				
				float factor = timer / aliveTime;
				
				actors[i].transform.localScale = new Vector3(factor,factor,factor);
				
				
			}
			
			timer += Time.deltaTime;
			yield return null;

		}
		
		
		
		
		
		yield return null;
		
		//snapping
		for (int i = 0; i < addedTiles.Count; ++i){
				
			actors[i].transform.localScale = Vector3.one;
		
		}

	}


	IEnumerator _fallTiles(BoardModel.TileShiftData [] tilesShift){
		
		if(tilesShift.Length == 0)yield break;
		
		
		
		float timer = 0;
		float settleTime = 0;

		for (int i = 0; i < tilesShift.Length; ++i){
			
			if(tilesShift[i].minTileIndex < currentData.numRows)
				settleTime = Mathf.Max(settleTime, tilesShift[i].tileMovement);
		}
		Debug.Log("max settle time is "+settleTime);
		
		
	#if UNITY_EDITOR
		if (!boardModel.isStable()){
			
			
			
			
			EditorApplication.ExecuteMenuItem("Edit/Pause");
			
		}
	#endif
	
		if (settleTime > Mathf.Epsilon){
			while (timer <= settleTime){


				
//				float factor = timer/settleTime;
				
				for (int i = 0; i < tilesShift.Length; ++i){
				
					
					for (int j = tilesShift[i].minTileIndex; j < currentData.numRows; ++j){

						TileModel t = boardModel.getTileModelAt(j, i);
						//this happens when we reach tiles above the highest level
						//possbile,need to add more tiles(happens in next coroutine)
						if (t==null){
							break;
						}
						
						t.AttachedTileActor.transform.localPosition -= new Vector3(0,(Time.deltaTime/settleTime)*currentData.tileSpacing*tilesShift[i].tileMovement);
						
					}
					
				}

				timer += Time.deltaTime;
				yield return null;
			}
			
			
		}
		yield return null;

	}
	IEnumerator _deleteTileActors(TileModel [] modelsDeleted ,float deathTime){
		
		
	
		if(modelsDeleted.Length == 0)yield break;
	
		float timer = 0;
		if (deathTime <= 0) deathTime = Mathf.Epsilon;
	
		while (timer <= deathTime){
			for (int i = 0; i < modelsDeleted.Length; ++i){
				TileActor act = modelsDeleted[i].AttachedTileActor;
				Color c = act.getColor();
				act.setColor(new Color(c.r,c.g,c.b,1-timer/deathTime));				
			}
			timer += Time.deltaTime;
			yield return null;

		}
		yield return null;
		//actual deletion
		for (int i = 0; i < modelsDeleted.Length; ++i){
			Assert.IsTrue(modelsDeleted[i].isOffBorad());
			TileActor act = modelsDeleted[i].AttachedTileActor;
			Destroy(act.gameObject);
			modelsDeleted[i].AttachedTileActor = null;
			
		}
		
	}
	
	
	
	public void onInputEnd(){
	
		TileActor selected = selectedTile;
		//clear state at top
		selectedTile = null;
		if (selected!=null){
			selected.restoreHighlight();


			Vector2 diff = getpointerPosInBoardSpace() - (Vector2) selected.transform.localPosition;
			float angle = Mathf.Repeat(Vector2.SignedAngle(myTransform.right,diff)+360.0f,360.0f);

			
			
			Dir userDir = Match3.Utils.getSwipeDirection(diff,currentData.swipeDelta,angle);
			int otherRow = selected.tileModel.row, otherCol = selected.tileModel.col;
			switch (userDir){
				case Dir.Up:
					++otherRow;
					break;
				case Dir.Down:
					--otherRow;
					break;
				case Dir.Left:
					--otherCol;
					break; 
				case Dir.Right:
					++otherCol;
					break;
				case Dir.NoDir:
					return;
			}
			
			if(otherCol < 0 || otherCol >= currentData.numCols|| otherRow < 0   || otherRow >= currentData.numRows)return;




			TileModel other = boardModel.getTileModelAt(otherRow, otherCol);
			if(other == null || !boardModel.canSwap(selected.tileModel,other))return;
			
			Assert.IsNotNull(other.AttachedTileActor);
			
			TileActor otherActor = other.AttachedTileActor;

			//controller work,update model,disable view
			//model work
			boardModel.swapTiles(selected.tileModel,other);
			//view work
			userInput.enabled = false;
			
			StartCoroutine(_tileMovmentAndStabilization(selected,otherActor,currentData.tileMovementTime,onStabilizatonOver));
		
		}
	}

	void onStabilizatonOver(){
		userInput.enabled = true;
	}
	

	
	//returns Vector2 to get rid of z component
	private Vector2 getpointerPosInBoardSpace(){
		return transform.InverseTransformPoint(userInput.getPointerPosInWorldSpace());
	}


	private void createTilesFromModel(BoardModel model){
		
		
		for (int i = 0; i < currentData.numRows; ++i){
			for (int j = 0; j < currentData.numCols; ++j){
				TileModel m = model.getTileModelAt(i,j);
				createTileActor(m);
			}	
		}
	}

	private void deleteTileActors(){
		if(boardModel == null)return;
		
		for (int i = 0; i < currentData.numRows; ++i){
			for (int j = 0; j < currentData.numCols; ++j){
				deleteTileActor(boardModel.getTileModelAt(i, j));
			}	
		}
		
	}


	private void deleteTileActor(TileModel tileModel){
		
		Assert.IsTrue(tileModel!=null && !tileModel.isOffBorad() && tileModel.AttachedTileActor!=null);

		
		Destroy(tileModel.AttachedTileActor.gameObject);
		tileModel.AttachedTileActor = null;
		

	}

		


	private TileActor createTileActor(TileModel tileModel){
		Assert.IsTrue(tileModel!=null && !tileModel.isOffBorad());


		float halfRows = (currentData.numRows-1)*currentData.tileSpacing/2.0f;
		float halfCols = (currentData.numCols-1)*currentData.tileSpacing/2.0f;
		
		
	

		TileActor currentTile = Instantiate(currentData.tileInstance,Vector3.zero, Quaternion.identity,myTransform);
		//updating visual 
		currentTile.tileModel = tileModel;
		currentTile.name = tileModel.row + "," + tileModel.col;
		currentTile.transform.localPosition = new Vector3(tileModel.col*currentData.tileSpacing-halfCols, tileModel.row * currentData.tileSpacing-halfRows);
		currentTile.setupFromConfig(currentData.tileConfigs[tileModel.type]);
		
		
		
		
		//linking visual and model
		Assert.IsNull(tileModel.AttachedTileActor);
		tileModel.AttachedTileActor = currentTile;
		return currentTile;
		
	}

	private void OnDestroy(){
		boardModel = null;
		if(userInput!=null)userInput.unregisterInputHandler(this);
		userInput = null;
		selectedTile = null;
		
		myTransform = null;
	}


	private void OnDrawGizmos(){


		
		float boardWidth = (currentData.numCols - 1) * currentData.tileSpacing+currentData.tileInstance.getWidth();
		float boardHeight =(currentData.numRows - 1) * currentData.tileSpacing+currentData.tileInstance.getHeight();
		

		Gizmos.color = Color.blue;
		//note assumes tiles are centered at this gameobject
		Gizmos.DrawWireCube(transform.position,new Vector3(boardWidth,boardHeight,10));
		
	}



	
	private LevelData currentData;

	
	
	//model data
	private BoardModel boardModel;
	
	
	//score realted varaibles
	private int scoreGui;
	private int movesRemaining;

	
	//input state variables	
	private TileActor selectedTile;
	private UserInput userInput;	
	
	
	//components 	
	private Transform myTransform;

	
	
	
	



}
