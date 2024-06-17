using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// For Clients
public class Playfield : MonoBehaviour
{ // 0 = no coin, 1 = player1, 2 = player 2
	public static Playfield instance;
	const int rows = 4;
	const int columns = 4; 
	const int layers = 4;
	const int winlength = 4;

    int[,,] board = new int[layers, rows, columns]
    {
	// Layer 0 (top)
		{
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}

        },

	// Layer 1
		{
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}

        },

	// Layer 2
		{
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0}

        },

	// Layer 3
		{
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},
            {0, 0, 0, 0},

        }

    };

    private void Awake()
	{
		instance = this;
        // DontDestroyOnLoad(gameObject);
    }


    public int ValidMove(int row, int column)
	{
		for (int layer = layers - 1 ; layer >= 0; layer--)
		{
			if (board[layer, row, column] == 0) //checking if (x,y) coordinate on that layer is free is free
			{
				return layer;
			}
		}
		Debug.Log("No Valid Move");
		return -1;
	}

    

	public void DropBlock(int layer, int row, int column, int player)
	{
		board[layer, row, column] = player;
        
		print(DebugBoard());

		if(!DBManager.online)
        {
            GameManager.instance.WinCondition(WinCheck());
        }
		 
	}

    #region LocalGame
        
    bool WinCheck()
    {
        if (x_axis_Check() || z_axis_Check() || z_x_axis_Check() || y_axis_Check() || x_y_axis_Check() || z_y_axis_Check() || x_y_z_axis_Check())
        {
            return true;
            
        }
        return false;
    }

    //2D Checks - each layer (start form the fourth to increase efficency)
    bool x_axis_Check() //horizontal 
    {
        for (int i = layers - 1; i >= 0; i--)
        {
            for (int j = 0; j < rows; j++)
            {
                
                if (board[i, j, 0] > 0)
                {
                    int a = board[i, j, 0];
                    int b = board[i, j, 1];
                    int c = board[i, j, 2];
                    int d = board[i, j, 3];
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }

                }
                
            }
        }
        return false;

    }

    bool z_axis_Check() //Vertical
    {
        for (int i = layers - 1; i >= 0; i--)
        {
            for (int k = 0; k < columns; k++)
            {
                
                if (board[i, 0, k] > 0)
                {
                    int a = board[i, 0, k];
                    int b = board[i, 1, k];
                    int c = board[i, 2, k];
                    int d = board[i, 3, k];
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }

                }
                
            }
        }

        return false;

    }

    bool z_x_axis_Check() //diagonal
    {
        
        for (int i = layers - 1; i >= 0; i--)
        {
            //Right to left Check
            if (board[i, 0, 0] > 0)
            {
                int a = board[i, 0, 0];
                int b = board[i, 1, 1];
                int c = board[i, 2, 2];
                int d = board[i, 3, 3];
                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }
            }

            if (board[i,0,3] > 0)
            {
                int a = board[i, 0, 3];
                int b = board[i, 1, 2];
                int c = board[i, 2, 1];
                int d = board[i, 3, 0];
                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }
            }
        }

        return false;
    }



    //3D multi-layer involvement

    bool y_axis_Check() //diagonal
    {
        for (int i = layers - 1; i >= 3; i--)
        {
            for (int j = 0; j < rows; j++)
            {
                for (int k = 0; k < columns; k++)
                {
                    if (board[i, j, k] > 0)
                    {
                        int a = board[i, j, k];
                        int b = board[i - 1, j, k];
                        int c = board[i - 2, j, k];
                        int d = board[i - 3, j, k];
                        if (a == b && a == c && a == d)
                        {
                            Debug.Log("Win" + a);
                            return true;

                        }

                    }
                }
            }
        }
        return false;
    }

    bool z_y_axis_Check()
    {
        
        for (int j = 0; j < rows; j++)
        {
            
            if (board[3, j, 0] > 0)
            {
                int a = board[3, j, 0];
                int b = board[2, j, 1];
                int c = board[1, j, 2];
                int d = board[0, j, 3];
                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }

            }

            if (board[3, j, 3] > 0)
            {
                int a = board[3, j, 3];
                int b = board[2, j, 2];
                int c = board[1, j, 1];
                int d = board[0, j, 0];
                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }

            }
                
        
        }
        return false;

    }

    bool x_y_axis_Check()
    {
        
        for (int k = 0; k < columns; k++)
        {
            if (board[3, 0, k] > 0)
            {
                int a = board[3, 0, k];
                int b = board[2, 1, k];
                int c = board[1, 2, k];
                int d = board[0, 3, k];
                
                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }
            }

            if (board[3, 3, k] > 0)
            {
                int a = board[3, 3, k];
                int b = board[2, 2, k];
                int c = board[1, 1, k];
                int d = board[0, 0, k];

                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }
            }
        }
        return false;
    }

    bool x_y_z_axis_Check() //through all layers
    {
        if (board[3,0,0] > 0)
        {
            int a = board[3, 0, 0];
            int b = board[2, 1, 1];
            int c = board[1, 2, 2];
            int d = board[0, 3, 3];

            if (a == b && a == c && a == d)
            {
                Debug.Log("Win" + a);
                return true;

            }

        }

        if (board[3,0,3] > 0)
        {
            int a = board[3, 0, 3];
            int b = board[2, 1, 2];
            int c = board[1, 2, 1];
            int d = board[0, 3, 0];

            if (a == b && a == c && a == d)
            {
                Debug.Log("Win" + a);
                return true;

            }

        }

        if (board[3,3,0] > 0)
        {
            int a = board[3, 3, 0];
            int b = board[2, 2, 1];
            int c = board[1, 1, 2];
            int d = board[0, 0, 3];

            if (a == b && a == c && a == d)
            {
                Debug.Log("Win" + a);
                return true;

            }

        }

        if (board[3,3,3] > 0)
        {
            int a = board[3, 3, 3];
            int b = board[2, 2, 2];
            int c = board[1, 1, 1];
            int d = board[0, 0, 0];

            if (a == b && a == c && a == d)
            {
                Debug.Log("Win" + a);
                return true;

            }

        }
        return false;
    }

    public int[,,] CurrentPlayfield()
	{
		int[,,] current = new int[layers, rows, columns];
		System.Array.Copy(board, current, board.Length);
		return current;
	}
    #endregion
    string DebugBoard()
    {
        string s = "";
        string seperator = ",";
        string boarder = "|";

        for (int i = 0; i < 4; i++) //layers
        {

            for (int j = 0; j < 4; j++) //rows
            {
                s += boarder;

                for (int k = 0; k < 4; k++) //columns
                {
                    s += board[i, j, k];
                    if (k != 3)
                    {
                        s += seperator;
                    }
                    else
                    {
                        s += boarder;
                    }
                }
                s += "\n";

            }
            s += "\n";
        }
        return s;
    }

    // resets the client UI board
    public void Reset_Board()
    {
        for (int i = 0; i < 4; i++) //layers
        {

            for (int j = 0; j < 4; j++) //rows
            {

                for (int k = 0; k < 4; k++) //columns
                {
                    board[i,j,k] = 0;
                }
            }
        }
    }

}
