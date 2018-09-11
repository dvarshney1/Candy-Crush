using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SocialPlatforms.Impl;

namespace Match3{
    
    
    

    public class TileModel{

        


        public TileModel(int row, int col, int type){
            this.row = row;
            this.col = col;
            this.type = type;
        }



        public bool isOffBorad(){
            return row == -1 && col == -1 &&  type == -1;
        }
        public void clearBoardData(){
            row = col = type = -1;
        }
        
        public int row =  -1;
        public int col =  -1;
        public int type = -1;


        
        public TileActor AttachedTileActor{
            get{
                return tileActor;
            }

            set{
                tileActor = value;
            }
        }

        private TileActor tileActor;
        
        
        
        public static void swapBoardData(TileModel t1,TileModel t2){
            
            
            int temp = t1.row;
            t1.row = t2.row;
            t2.row = temp;
            temp = t1.col;
            t1.col = t2.col;
            t2.col = temp;


        }
        
        public override string ToString(){
            return "row is " + row + " col is " + col + " type is " + type;
        }
        
        
        
    }
    public class BoardModel{
        
        
        //check no matches by default
        public BoardModel(int numTypes,int numRows,int numCols,int numMoves,int targetScore,int chainBaseScore){
            this.numTypes = numTypes;
            this.numRows = numRows;
            this.numCols = numCols;
            this.numMoves =numMoves;
            this.targetScore = targetScore;
            this.chainBaseScore = chainBaseScore;
            
            
            tiles = new List<List<TileModel>>(numRows);
            for (int i = 0; i < numRows; i++){
                // adds a row
                tiles.Add(new List<TileModel>(numCols));
                // creating a 2D array
                for (int j = 0; j < numCols; j++){

                    TileModel t = new TileModel(i,j,Random.Range(0,numTypes));
                    // selecting a location for the tile to add
                    //TODO avoid infinite loop here handling
                    while (isMatch3Horizontalleft(i, j, t.type) || isMatch3VerticalDown(i,j,t.type)){
                   
                        t.type = Random.Range(0,this.numTypes);
                        // replacing so that no 3 tiles are similar at the start of the game
                    }
                    // adding elements in tiles
                    tiles[i].Add(t);
                    
                }
            }
            comparer = new TileOnBoardEqualityComparer(numRows, numCols);

        }
        
        
        

        private bool isMatch3Horizontalleft(int row, int col, int type){

            Assert.IsTrue(row >= 0 && row < numRows && col >= 0 && col < numCols);
			
            return  col >=2 && tiles[row][col - 1]!=null && tiles[row][col - 1].type == type && tiles[row][col - 2]!=null && tiles[row][col - 2].type == type;
            
            
        }
        private bool isMatch3VerticalDown(int row, int col, int type){
            
            Assert.IsTrue(row >= 0 && row < numRows && col >= 0 && col < numCols);
            
            return   row >=2 && tiles[row-1][col] !=null && tiles[row-1][col].type == type && tiles[row-2][col]!=null && tiles[row-2][col].type == type;

        }


        
        
        public TileModel getTileModelAt(int row, int col){
            Assert.IsTrue(row >= 0 && row < numRows && col >= 0 && col < numCols);
            Assert.IsTrue(tiles[row][col] == null || (tiles[row][col].row == row && tiles[row][col].col == col),"index mismatch");
            
            
            return tiles[row][col];
 
        }

        //can swap only adjacent tiles
        public bool canSwap(TileModel a, TileModel b){
            Assert.IsTrue(tiles[a.row][a.col] == a && tiles[b.row][b.col] == b);

            int rowDiff = Mathf.Abs(a.row - b.row);
            int colDiff = Mathf.Abs(a.col - b.col);

            return rowDiff<=1 && colDiff <=1 && rowDiff != colDiff;
            
        }

        public void swapTiles(TileModel a, TileModel b){
            Assert.IsTrue(a != null && b != null && canSwap(a,b),"cannot swap nulls externally");
                TileModel.swapBoardData(a, b);
                // swaps the contents (rows, cols) of the tiles internally.
                // update board
                // swaps the pointers pointing to the tiles to their new positions. (changing references)
                tiles[a.row][a.col] = a;
                tiles[b.row][b.col] = b;
        }

        private Chains findChains(){
            var horizontal = findHorizontalChains();
            var vertical = findVerticalChains();
            horizontal.merge(vertical);
            
            return horizontal;
        }


        private Chains findVerticalChains(){
          
            
            HashSet<TileModel> set  = new HashSet<TileModel>(comparer);
            List<TileModel> temp = new List<TileModel>();
            
            int score = 0;
            for (int i = 0; i < numCols; ++i){
                
                for (int j = numRows-1; j>=0;){
                    if(tiles[j][i]==null){
                        --j;
                        continue;
                        
                    }
                    int type = tiles[j][i].type;                    
                    
                    temp.Clear();
                    for (; j >= 0; --j){
                        if (tiles[j][i]==null || tiles[j][i].type!= type)
                            break;
                        temp.Add(tiles[j][i]);
                    }
                    if (temp.Count >= 3){
                        set.UnionWith(temp);
                        score += (temp.Count-2)*chainBaseScore;

                    }
                }
                
            }

            return new Chains(set,score);
        }
        private Chains findHorizontalChains(){

            

            HashSet<TileModel> set = new HashSet<TileModel>(comparer);
            
            List<TileModel> temp = new List<TileModel>();


            int score = 0;
            for (int i = 0; i < numRows; ++i){
                
                for (int j = numCols-1; j>=0;){
                    if(tiles[i][j]==null){
                        --j;
                        continue;
                        
                    }
                    int type = tiles[i][j].type;                    
                    
                    temp.Clear();
                    for (; j >= 0; --j){
                        if (tiles[i][j]==null || tiles[i][j].type!= type)
                            break;
                        temp.Add(tiles[i][j]);
                    }
                    
                    if (temp.Count >= 3){
                        set.UnionWith(temp);
                        score += (temp.Count-2)*chainBaseScore; // 50 = chain base score 
                        
                    }
                    
                }
                
                
            }

            return new Chains(set,score);
            
        }


        public Chains deleteChains(){
            Chains c = findChains();
            
            var arr = c.toArray();
            
            for(int i = 0;i<arr.Length;++i){
                TileModel t = arr[i];
            
                //deletion from 2d array (of pointer)
                tiles[t.row][t.col] = null;
                
                //deletion of array from self (clearing rows, cols and type by calling a function of the class)
                t.clearBoardData();
            }
            return c;
            
        }


        public struct TileShiftData{
            public int tileMovement;
            public int minTileIndex;
        }
        public TileShiftData [] settleTiles(){

            TileShiftData [] tilesShift =  new TileShiftData[numCols];
            
            //note this logic is only valid for match 3,for more there can be more than one nulls.
            
            for (int i = 0; i < numCols; i++){
                int row = 0;
                while (row < numRows && tiles[row][i] != null){
                    ++row;
                }
                int j = row;
                
                tilesShift[i].minTileIndex = j;

                for (; j < numRows && tiles[j][i] == null; ++j);

                tilesShift[i].tileMovement = j - row;

                if (j == numRows){
                    tilesShift[i].minTileIndex = j;
                    tilesShift[i].tileMovement = 0;
                    continue;
                    
                }
                
                //shift the model tiles
                for (; j < numRows; ++j){
                    TileModel t = tiles[j][i];
                    if (t == null){
                        break;
                    }
                    int otherRow = j - tilesShift[i].tileMovement;
                    Assert.IsNull(tiles[otherRow][t.col], "problem");
                    Assert.IsTrue(t!=null && t.AttachedTileActor!=null && t.col == i);
                    
                    
                    //move myself to null position 
                    t.row = otherRow;
                    tiles[otherRow][t.col] = t;
                    //move null up
                    tiles[j][t.col] = null;
                    
             
                    
                }

            }

            return tilesShift;
        }

        public override string ToString(){
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = numRows-1; i >= 0; --i){
                for (int j = 0; j < numCols; ++j){
                    stringBuilder.Append(tiles[i][j]+" ");
                }
                stringBuilder.Append("\n");
            }

            return stringBuilder.ToString();
            
        }

        public void print(string header){
            Debug.Log(header);
            for (int i = numRows-1; i >= 0; --i){
                StringBuilder stringBuilder = new StringBuilder();
                for (int j = 0; j < numCols; ++j){
                    stringBuilder.Append(tiles[i][j] == null?"false ":"true ");
                }
                Debug.Log(stringBuilder);
            }
        }

        public bool isStable(){
            bool ans = true;
            for (int i = numRows - 1; i >= 0; --i){
                for (int j = 0; j < numCols; ++j){
                    ans &= tiles[i][j] == null || (tiles[i][j].row == i && tiles[i][j].col == j);
                    
                }
            }
            return ans;
        }

        //note added tiles might make combos themselves,we let the game resolve itself.
        public List<TileModel> addTilesTillfull(){

            List<TileModel> modelsAdded = new List<TileModel>();

            for (int i = numRows-1; i >=0 ;--i){
                for (int j = 0; j < numCols ; ++j){
                    if (tiles[i][j] == null){
                        TileModel t = new TileModel(i, j, Random.Range(0, numTypes));
                        tiles[i][j] = t;
                        modelsAdded.Add(t);
                    }
                }
            }
            return modelsAdded;
        }
        
        private readonly IEqualityComparer<TileModel> comparer;
        private readonly int numTypes;
        private  List<List<TileModel>> tiles;
        private readonly int numRows;
        private readonly int numCols;
        public readonly int numMoves;
        public readonly int targetScore;
        private int chainBaseScore;

    }

    
    class TileOnBoardEqualityComparer : IEqualityComparer<TileModel>
    {

        public TileOnBoardEqualityComparer(int numRows,int numCols){
            this.numRows = numRows;
            this.numCols = numCols;
        }
        public bool Equals(TileModel b1, TileModel b2)
        {
            if (b2 == null && b1 == null)
                return true;
            if (b1 == null || b2 == null)
                return false;
            return b1.row == b2.row && b1.col == b2.col;
            
        }

        public int GetHashCode(TileModel t)
        {
            int hCode = t.row*numCols+t.col;
            
            return hCode.GetHashCode();
        }
        private int numRows;
        private int numCols;
         
    }


    
    

    
}