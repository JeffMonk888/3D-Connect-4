using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class AI_Mode : MonoBehaviour
{
    public static AI_Mode instance;
    int numCols = 4;
    int numRows = 4;
    int numLayers = 4;
    int maxSearch = DBManager.difficulty;

    private long[,,,] zobristTable;
    private Dictionary<long, float> transpositionTable = new Dictionary<long, float>();
    private System.Random random = new System.Random();
    public class Move
    {
        public int column;
        public int row;
        public int layer;
        public float score;

        public Move()
        {

        }

        public Move(float _score)
        {
            score = _score;

        }
        public Move(int _row, int _column, int _layer, float _score)
        {
            row = _row;
            column = _column;
            layer = _layer;
            score = _score;

        }
        public Move(int _row, int _column, int _layer)
        {
            row = _row;
            column = _column;
            layer = _layer;
        }

    }

    private void Awake()
    {
        instance = this;
        init_zobrist();
        Debug.Log(maxSearch);
    }

    private void init_zobrist()
    {
        zobristTable = new long[numLayers, numRows, numCols, 2]; // Assuming 2 players
        for (int layer = 0; layer < numLayers; layer++)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    for (int player = 0; player < 2; player++) // 2 players
                    {
                        zobristTable[layer, row, col, player] = RandomLong();
                    }
                }
            }
        }
    }

    private long RandomLong()
    {
        byte[] buffer = new byte[8];
        random.NextBytes(buffer);
        return System.BitConverter.ToInt64(buffer, 0);
    }

    // Assuming player values are 1 and 2, and empty is 0
    private long ComputeHash(int[,,] board)
    {
        long hash = 0;
        for (int layer = 0; layer < numLayers; layer++)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    int player = board[layer, row, col];
                    if (player > 0) // If not empty
                    {
                        hash ^= zobristTable[layer, row, col, player - 1];
                    }
                }
            }
        }
        return hash;
    }

    // Minimax, GetValidMoves, PerformTempMove, EvaluateBoard methods should be updated to use Zobrist hashing

    
    List<Move> GetValidMoves(int[,,] currentBoard)
    {
        List<Move> moveList = new List<Move>();
        for (int col = 0; col < numCols; col++)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int layer = numLayers - 1; layer >= 0; layer--)
                {
                    if (currentBoard[layer, row, col] == 0)
                    {
                        Move m = new Move(row, col, layer);
                        moveList.Add(m);
                        break;
                    }
                }

            }
        }
        return moveList;
    }
    public void BestMove()//start the AI
    {
        Move bestMove = new Move(-1, -1, -1, Mathf.NegativeInfinity);
        int[,,] currentPlayfield = Playfield.instance.CurrentPlayfield();
        List<Move> possibleMoves = new List<Move>();
        possibleMoves.AddRange(GetValidMoves(currentPlayfield));
	 
        foreach (Move move in possibleMoves)
        {
            move.score = Mathf.NegativeInfinity;
            int[,,] tempBoard = PerformTempMove(move, currentPlayfield, 2);
            move.score = Minimax(tempBoard, maxSearch, false, Mathf.NegativeInfinity, Mathf.Infinity);

            if (move.score > bestMove.score)
            {
                bestMove = move;
            }
        }
        //Game Manager - perform the best move
        GameManager.instance.ColumnPressed(bestMove.row, bestMove.column); //switch the row and column if it doesn't work

    }

    int[,,] PerformTempMove(Move move, int[,,] currenBoard, int player)
    {
        int[,,] tempBoard = new int[numLayers, numRows, numCols];
        System.Array.Copy(currenBoard, tempBoard, currenBoard.Length);
        tempBoard[move.layer, move.row, move.column] = player;
        return tempBoard;
    }

    float Minimax(int[,,] currentBoard, int depth, bool isMaximizer, float alpha, float beta)
    {
        long boardHash = ComputeHash(currentBoard);
        if (transpositionTable.ContainsKey(boardHash))
        {
            return transpositionTable[boardHash];
        }

        if (depth == 0)
        {
            float score = EvaluateBoard(currentBoard, isMaximizer);
            return score;
            
        }

        List<Move> possibleMoves = GetValidMoves(currentBoard);

        if (isMaximizer)
        {
            float maxEval = Mathf.NegativeInfinity;
            foreach (Move move in possibleMoves)
            {
                int[,,] tempBoard = PerformTempMove(move, currentBoard, 2); // Assuming 2 is AI player
                float eval = Minimax(tempBoard, depth - 1, false, alpha, beta);
                maxEval = Mathf.Max(maxEval, eval);
                alpha = Mathf.Max(alpha, eval);
                if (beta <= alpha)
                    break;
            }
            transpositionTable[boardHash] = maxEval;
            return maxEval;

        }
        else
        {
            float minEval = Mathf.Infinity;
            foreach (Move move in possibleMoves)
            {
                int[,,] tempBoard = PerformTempMove(move, currentBoard, 1); // Assuming 1 is opponent
                float eval = Minimax(tempBoard, depth - 1, true, alpha, beta);
                minEval = Mathf.Min(minEval, eval);
                beta = Mathf.Min(beta, eval);
                if (beta <= alpha)
                    break;
            }
            transpositionTable[boardHash] = minEval;
            return minEval;
        }
    }

    float EvaluateBoard(int[,,] currentBoard, bool isMaximizer)
    {
        float boardScore = 0;
        boardScore += z_axis_check(currentBoard, isMaximizer);
        boardScore += x_axis_check(currentBoard, isMaximizer);
        boardScore += x_z_axis_check(currentBoard, isMaximizer);
        boardScore += y_axis_check(currentBoard, isMaximizer);
        boardScore += y_z_axis_check(currentBoard, isMaximizer);
        boardScore += y_x_axis_check(currentBoard, isMaximizer);
        boardScore += x_y_z_axis_check(currentBoard, isMaximizer);
        return boardScore;
    }
   
    //2D Check
    float z_axis_check(int[,,] currentBoard, bool isMaximizer) //Horizontal Check
    {
        float score = 0;
        for (int layer = numLayers - 1; layer >= 0; layer--)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    
                    //Left to right
                    if (col < 1)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row, col + 1];
                        int c = currentBoard[layer, row, col + 2];
                        int d = currentBoard[layer, row, col + 3];
                        //WinCheck
                        score += WinCheck(a, b, c, d, isMaximizer);
                    }

                    //Right to Left
                    if (col > 2)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row, col - 1];
                        int c = currentBoard[layer, row, col - 2];
                        int d = currentBoard[layer, row, col - 3];

                        score += WinCheck(a, b, c, d, isMaximizer);
                    }

                }
                    
            }
        }
        return score;
    }

    float x_axis_check(int[,,] currentBoard, bool isMaximizer)//Vertical Check
    {
        float score = 0;
        for (int layer = numLayers - 1; layer >= 0; layer--)
        {
            for (int col = 0; col < numCols; col++)    
            {
                for (int row = 0; row < numRows; row++)
                {
                  
                    //Left to right
                    if (row < 1)
                    {
                       
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row + 1, col];
                        int c = currentBoard[layer, row + 2, col];
                        int d = currentBoard[layer, row + 3 , col];
                        //WinCheck
                        score += WinCheck(a, b, c, d, isMaximizer);


                    }

                    //Right to Left
                    if (row > 2)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row - 1, col];
                        int c = currentBoard[layer, row - 2, col];
                        int d = currentBoard[layer, row - 3, col];
                        score += WinCheck(a, b, c, d, isMaximizer);
                    }

                }

            }
        }
       
        return score;
    }

    float x_z_axis_check(int[,,] currentBoard, bool isMaximizer) //Diagonal Check
    {
        float score = 0;
        //Top to bottom
	    for (int layer = numLayers - 1; layer >= 0; layer--)
        {
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < numCols; col++)
                {
                    
                    //right to left
                    if (row < 1 && col < 1)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row + 1, col + 1];
                        int c = currentBoard[layer, row + 2, col + 2];
                        int d = currentBoard[layer, row + 3, col + 3];

                        score += WinCheck(a, b, c, d, isMaximizer);
                    }
                    //left to right
                    if (row < 1 && col > 2)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row + 1, col - 1];
                        int c = currentBoard[layer, row + 2, col - 2];
                        int d = currentBoard[layer, row + 3, col - 3];
                        score += WinCheck(a, b, c, d, isMaximizer);
                    }
                    
                }
            }
        }

        //Bottom to top 
        for (int layer = numLayers - 1; layer >= 0; layer--)
        {
            for (int row = numRows - 1; row >= 0; row--)
            {
                for (int col = 0; col < numCols; col++)
                {
                   

                    if (row > 2 && col < 1)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row - 1, col + 1];
                        int c = currentBoard[layer, row - 2, col + 2];
                        int d = currentBoard[layer, row - 3, col + 3];
                        score += WinCheck(a, b, c, d, isMaximizer);
                    }

                    //
                    if (row > 2 && col > 2)
                    {
                        int a = currentBoard[layer, row, col];
                        int b = currentBoard[layer, row - 1, col - 1];
                        int c = currentBoard[layer, row - 2, col - 2];
                        int d = currentBoard[layer, row - 3, col - 3];
                        score += WinCheck(a, b, c, d, isMaximizer);
                    }
                }
            }
        }
        return score;
                    
    }

    //3D check
    float y_axis_check(int[,,] currentBoard, bool isMaximizer)
    {
        float score = 0;
        for (int row = 0; row < numRows; row++)
        {
            for (int col = 0; col < numCols; col++)
            {
                
                int a = currentBoard[3, row, col];
                int b = currentBoard[2, row, col];
                int c = currentBoard[1, row, col];
                int d = currentBoard[0, row, col];
                score += WinCheck(a, b, c, d, isMaximizer);
            } 
        }
        
        return score;
    }   

    float y_z_axis_check(int[,,] currentBoard, bool isMaximizer)
    {
        float score = 0;
        for (int row = 0; row < numRows; row++)
        { 
            //left to right
            
		    //Bottom to Top
            int a = currentBoard[3, row, 0];
            int b = currentBoard[2, row, 1];
            int c = currentBoard[1, row, 2];
            int d = currentBoard[0, row, 3];
            score += WinCheck(a, b, c, d, isMaximizer);

		    //Top to bottom
            int e = currentBoard[0, row, 0];
            int f = currentBoard[1, row, 1];
            int g = currentBoard[2, row, 2];
            int h = currentBoard[3, row, 3];
            score += WinCheck(e, f, g, h, isMaximizer);

            //right to left
            
		    //Bottom to Top
            int i = currentBoard[3, row, 3];
            int j = currentBoard[2, row, 2];
            int k = currentBoard[1, row, 1];
            int l = currentBoard[0, row, 0];
            score += WinCheck(i, j, k, l, isMaximizer);

		    //Top to Bottom
            int m = currentBoard[0, row, 3];
            int n = currentBoard[1, row, 2];
            int o = currentBoard[2, row, 1];
            int p = currentBoard[3, row, 0];

            score += WinCheck(m, n, o, p, isMaximizer);
            
            
        }
        return score;
    }


    float y_x_axis_check(int[,,] currentBoard, bool isMaximizer)
    {
        float score = 0; 
       
        for (int col = 0; col < numCols; col++)
        {
                
            //Front to back 
            //Bottom to Top
            int a = currentBoard[3, 0, col];
            int b = currentBoard[2, 1, col];
            int c = currentBoard[1, 2, col];
            int d = currentBoard[0, 3, col];
            score += WinCheck(a, b, c, d, isMaximizer);

		    //Top to Bottom 
            int e = currentBoard[0, 0, col];
            int f = currentBoard[1, 1, col];
            int g = currentBoard[2, 2, col];
            int h = currentBoard[3, 3, col];
            score += WinCheck(e, f, g, h, isMaximizer);
            

            //back to front 
            //Bottom to Top
            int i = currentBoard[3, 3, col];
            int j = currentBoard[2, 2, col];
            int k = currentBoard[1, 1, col];
            int l = currentBoard[0, 0, col];
            score += WinCheck(i, j, k, l, isMaximizer);

            int m = currentBoard[0, 3, col];
            int n = currentBoard[1, 2, col];
            int o = currentBoard[2, 1, col];
            int p = currentBoard[3, 0, col];

            score += WinCheck(m, n, o, p, isMaximizer);
            
        }
        
        return score;
    }
    float x_y_z_axis_check(int[,,] currentBoard, bool isMaximizer)
    {
        float score = 0;
        //Bottom to Top
        // top left to bottom right 


        int a = currentBoard[3, 0, 0];
        int b = currentBoard[2, 1, 1];
        int c = currentBoard[1, 2, 2];
        int d = currentBoard[0, 3, 3];
        score += WinCheck(a, b, c, d, isMaximizer);

        //top right to bottom left
        int e = currentBoard[3, 0, 3];
        int f = currentBoard[2, 1, 2];
        int g = currentBoard[1, 2, 1];
        int h = currentBoard[0, 3, 0];
        score += WinCheck(e, f, g, h, isMaximizer);


        //bottom left to right
        int i = currentBoard[3, 3, 0];
        int j = currentBoard[2, 2, 1];
        int k = currentBoard[1, 1, 2];
        int l = currentBoard[0, 0, 3];
        score += WinCheck(i, j, k, l, isMaximizer);

        //bottom right to top left      
        int m = currentBoard[3, 3, 3];
        int n = currentBoard[2, 2, 2];
        int o = currentBoard[1, 1, 1];
        int p = currentBoard[0, 0, 0];
        score += WinCheck(m, n, o, p, isMaximizer);

        //Top to bottom
        //Top left to bottom right
        int q = currentBoard[0, 0, 0];
        int r = currentBoard[1, 1, 1];
        int s = currentBoard[2, 2, 2];
        int t = currentBoard[3, 3, 3];
        score += WinCheck(q, r, s, t, isMaximizer);

        //top right to bottom left
        int u = currentBoard[0, 0, 3];
        int v = currentBoard[1, 1, 2];
        int w = currentBoard[2, 2, 1];
        int x = currentBoard[3, 3, 0];
        score += WinCheck(u, v, w, x, isMaximizer);


        //bottom left to right
        int A = currentBoard[0, 3, 0];
        int B = currentBoard[1, 2, 1];
        int C = currentBoard[2, 1, 2];
        int D = currentBoard[3, 0, 3];
        score += WinCheck(A, B, C, D, isMaximizer);

        //bottom right to top left      
        int E = currentBoard[0, 3, 3];
        int F = currentBoard[1, 2, 2];
        int G = currentBoard[2, 1, 1];
        int H = currentBoard[3, 0, 0];
        score += WinCheck(E, F, G, H, isMaximizer);
        return score;
    }

    private float WinCheck(int a, int b, int c, int d, bool isMaximizer)
    {
        float score = 0;
        if ((a + b + c + d) == 0)
        {
            return 0;
        }
        if (a == b && a == c && a == d)
        {
            if (isMaximizer)
            {
                score += (a == 1) ? -1000 : 1000;
            }
            else //Minimizer
            {
                score += (a == 2) ? 1000 : -1000;
            }
        }

        //3 Streak - But can be a four
        if (a == b && a == c && d == 0)
        {
            if (isMaximizer)
            {
                score += (a == 1) ? -5 : 5;
            }
            else //Minimizer
            {
                score += (a == 2) ? 5 : -5;
            }
        }
        

        //2 Streak- But can be a four
        if (a == b && c == 0 && d == 0)
        {
            if (isMaximizer)
            {
                score += (a == 1) ? -1 : 1;
            }
            else //Minimizer
            {
                score += (a == 2) ? 1 : -1;
            }
        }
        return score;
    }
}
