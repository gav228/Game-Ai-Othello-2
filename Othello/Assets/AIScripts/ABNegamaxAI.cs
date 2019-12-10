using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ABNegamaxAI : AIScript
{


    public override KeyValuePair<int, int> makeMove(List<KeyValuePair<int, int>> availableMoves, BoardSpace[][] currentBoard)
    {
        uint turnNumber = 0;
        if (this.color == BoardSpace.WHITE)
        {
            turnNumber = 1;
        }
        KeyValuePair<KeyValuePair<int, int>, int> negamaxResults = abNegamax(currentBoard, 7, 0, turnNumber, int.MinValue, int.MaxValue);
        return negamaxResults.Key;
    }

    // Needs to return the move and the score
    public KeyValuePair<KeyValuePair<int, int>, int> abNegamax(BoardSpace[][] currentBoard, int MaxDepth, int currentDepth, uint turnNumber, int alpha, int beta)
    {

        // Check if the game finished while we were recursing
        if (isGameOver(currentBoard) || currentDepth == MaxDepth)
        {
            return new KeyValuePair<KeyValuePair<int, int>, int>(new KeyValuePair<int, int>(-1, -1), evaluate(currentBoard, turnNumber));
        }

        // Other wise bubble up values from below
        KeyValuePair<int, int> bestMove = new KeyValuePair<int, int>(0, 0);
        int bestScore = int.MinValue;

        // go through each move
        List<KeyValuePair<int, int>> possible_moves = GetValidMoves(currentBoard, turnNumber);
        for (int i = 0; i < possible_moves.Count; i++)
        {
            // Get move
            KeyValuePair<int, int> move = possible_moves[i];

            // Initialize new board
            BoardSpace[][] newBoard = new BoardSpace[8][];
            for (int j = 0; j < 8; ++j)
            {
                newBoard[j] = new BoardSpace[8];
                for (int k = 0; k < 8; k++)
                {
                    newBoard[j][k] = currentBoard[j][k];
                }
            }

            // Do the move
            PlacePiece(newBoard, move.Value, move.Key, turnNumber);

            // Recurse
            KeyValuePair<KeyValuePair<int, int>, int> result = abNegamax(newBoard, MaxDepth, currentDepth + 1, turnNumber + 1, -beta, -Mathf.Max(alpha, bestScore));

            int currentScore = -result.Value;

            // Update the best score
            if (currentScore > bestScore)
            {
                bestScore = currentScore;
                bestMove = move;

                if(bestScore >= beta)
                {
                    return new KeyValuePair<KeyValuePair<int, int>, int>(bestMove, bestScore);
                }
            }

        }

        return new KeyValuePair<KeyValuePair<int, int>, int>(bestMove, bestScore);
    }

    // Checks if the game finished
    public bool isGameOver(BoardSpace[][] Board)
    {
        foreach (BoardSpace[] row in Board)
        {
            foreach (BoardSpace space in row)
            {
                if (space == BoardSpace.EMPTY)
                {
                    return false;
                }
            }

        }
        return true;
    }

    // Print the board for debug purposes
    public void debugPrintBoard(BoardSpace[][] Board)
    {
        string boardRow = "";
        foreach (BoardSpace[] row in Board)
        {
            foreach (BoardSpace space in row)
            {
                boardRow += space + "\t";
            }
            boardRow += "\n";

        }
        Debug.Log(boardRow);

    }

    // Static evaluation function with weights, corners are weighted more heavily
    public int evaluate(BoardSpace[][] Board, uint turnNumber)
    {
        BoardSpace ourColor = turnNumber % 2 == 0 ? BoardSpace.BLACK : BoardSpace.WHITE;
        int count = 0;
        foreach (BoardSpace[] row in Board)
        {
            foreach (BoardSpace space in row)
            {
                if (space == ourColor)
                {
                    count++;
                }
            }

        }
        foreach (BoardSpace[] row in Board)
        {
            foreach (BoardSpace space in row)
            {
                if (space != ourColor && space != BoardSpace.EMPTY)
                {
                    count--;
                }
            }

        }
        if (Board[0][0] == ourColor)
        {
            count += 20;
        }
        if (Board[7][7] == ourColor)
        {
            count += 20;
        }
        if (Board[0][7] == ourColor)
        {
            count += 20;
        }
        if (Board[7][0] == ourColor)
        {
            count += 20;
        }
        if (Board[0][0] != ourColor && Board[0][0] != BoardSpace.EMPTY)
        {
            count -= 20;
        }
        if (Board[7][7] != ourColor && Board[7][7] != BoardSpace.EMPTY)
        {
            count -= 20;
        }
        if (Board[0][7] != ourColor && Board[0][7] != BoardSpace.EMPTY)
        {
            count -= 20;
        }
        if (Board[7][0] != ourColor && Board[7][0] != BoardSpace.EMPTY)
        {
            count -= 20;
        }
        return count;
    }


    public void PlacePiece(BoardSpace[][] board, int x, int y, uint turnNumber)
    {
        //instantiate piece at position and add to that side's points

        if (turnNumber % 2 == 0)
        {

            board[y][x] = BoardSpace.BLACK;
        }
        else
        {
            board[y][x] = BoardSpace.WHITE;
        }


        List<KeyValuePair<int, int>> changedSpaces = GetPointsChangedFromMove(board, turnNumber, x, y);
        foreach (KeyValuePair<int, int> space in changedSpaces)
        {

            if (turnNumber % 2 == 0)
            {

                board[space.Key][space.Value] = BoardSpace.BLACK;
            }
            else
            {

                board[space.Key][space.Value] = BoardSpace.WHITE;
            }
        }

    }

    public static List<KeyValuePair<int, int>> GetPointsChangedFromMove(BoardSpace[][] board, uint turnNumber, int x, int y)
    {
        //determines how much a move changed the overall point value
        BoardSpace enemyColor = turnNumber % 2 == 0 ? BoardSpace.WHITE : BoardSpace.BLACK;
        BoardSpace ourColor = turnNumber % 2 == 0 ? BoardSpace.BLACK : BoardSpace.WHITE;
        if (board.Length != 8 || board[0].Length != 8 || y < 0 || y >= 8 || x < 0 || x >= 8 || board[y][x] != ourColor)
        {
            return null;
        }

        List<KeyValuePair<int, int>> changedSpaces = new List<KeyValuePair<int, int>>();

        for (int k = -1; k < 2; ++k)
        {
            for (int l = -1; l < 2; ++l)
            {
                if (!((k == 0 && l == 0) || k + y < 0 || k + y >= 8 || l + x < 0 || l + x >= 8) && board[k + y][l + x] == enemyColor)
                {
                    int multiplier = 2;
                    while (k * multiplier + y >= 0 && k * multiplier + y < 8 && l * multiplier + x >= 0 && l * multiplier + x < 8)
                    {
                        if (board[k * multiplier + y][l * multiplier + x] == BoardSpace.EMPTY)
                        {
                            break;
                        }
                        else if (board[k * multiplier + y][l * multiplier + x] == ourColor)
                        {
                            for (int i = multiplier - 1; i >= 1; --i)
                            {
                                changedSpaces.Add(new KeyValuePair<int, int>(k * i + y, l * i + x));
                            }
                            break;
                        }
                        ++multiplier;
                    }

                }
            }
        }

        return changedSpaces;
    }


    public static List<KeyValuePair<int, int>> GetValidMoves(BoardSpace[][] board, uint turnNumber)
    {
        if (board.Length != 8 || board[0].Length != 8)
        {
            return null;
        }
        //determines the places that either player can move
        List<KeyValuePair<int, int>> possibleMoves = new List<KeyValuePair<int, int>>();

        for (int i = 0; i < 8; ++i)
        {
            for (int j = 0; j < 8; ++j)
            {
                if (board[i][j] == BoardSpace.EMPTY)
                {
                    BoardSpace enemyColor = turnNumber % 2 == 0 ? BoardSpace.WHITE : BoardSpace.BLACK;
                    BoardSpace ourColor = turnNumber % 2 == 0 ? BoardSpace.BLACK : BoardSpace.WHITE;
                    for (int k = -1; k < 2; ++k)
                    {
                        for (int l = -1; l < 2; ++l)
                        {
                            if (!((k == 0 && l == 0) || k + i < 0 || k + i >= 8 || l + j < 0 || l + j >= 8) && board[k + i][l + j] == enemyColor)
                            {
                                int multiplier = 2;
                                while (k * multiplier + i >= 0 && k * multiplier + i < 8 && l * multiplier + j >= 0 && l * multiplier + j < 8)
                                {
                                    if (board[k * multiplier + i][l * multiplier + j] == BoardSpace.EMPTY)
                                    {
                                        break;
                                    }
                                    else if (board[k * multiplier + i][l * multiplier + j] == ourColor)
                                    {
                                        possibleMoves.Add(new KeyValuePair<int, int>(i, j));
                                        k = 2;
                                        l = 2;
                                        break;
                                    }
                                    ++multiplier;
                                }
                            }
                        }
                    }
                }
            }
        }

        return possibleMoves;
    }
}
