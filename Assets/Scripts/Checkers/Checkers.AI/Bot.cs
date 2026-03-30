using System.Collections.Generic;

namespace Checkers.AI
{
    using Data;
    using static Utils;

    public struct BotReturnData
    {
        public int value;
        public Move move;
    }

    // Assume AI plays red
    public class Bot
    {
        public static long nodesVisited;

        public static BotReturnData Minimax(GameState state, int depth, PieceView.Color color, int alpha = int.MinValue, int beta = int.MaxValue, bool clear = true)
        {
            if (clear)
            {
                nodesVisited = 1;
            }
            else
            {
                nodesVisited++;
            }

            if (depth == 0 || state.IsGameOver)
                return new BotReturnData
                {
                    value = Evaluate(state),
                    move = null
                };

            List<Move> moves = GenerateMoves(state, color);

            // Max if it's the AI's turn
            if (state.aiTurn)
            {
                int max = int.MinValue;
                Move bestMove = null;

                foreach (Move m in moves)
                {
                    if (alpha > beta)
                    {
                        BotReturnData returnData = new()
                        {
                            value = max,
                            move = bestMove
                        };
                        return returnData;
                    }

                    GameState next = GameState.ApplyMove(state, m);
                    int score = Minimax(next, depth - 1, color, alpha, beta, false).value;
                    if (score > max)
                    {
                        max = score;
                        bestMove = m;
                    }
                    if (score > alpha)
                    {
                        alpha = score;
                    }
                }

                return new BotReturnData
                {
                    value = max,
                    move = bestMove
                };
            }
            // Min if human's turn
            else
            {
                int min = int.MaxValue;
                Move bestMove = null;

                foreach (Move m in moves)
                {
                    if (alpha > beta)
                    {
                        BotReturnData returnData = new()
                        {
                            value = min,
                            move = bestMove
                        };
                        return returnData;
                    }

                    GameState next = GameState.ApplyMove(state, m);
                    int score = Minimax(next, depth - 1, color, alpha, beta, false).value;
                    if (score < min)
                    {
                        min = score;
                        bestMove = m;
                    }
                    if (score < beta)
                    {
                        beta = score;
                    }
                }

                return new BotReturnData
                {
                    value = min,
                    move = bestMove
                };
            }
        }

        public static bool CaptureIsPossible(GameState state, PieceView.Color color)
        {
            List<Move> moves = GenerateMoves(state, color);

            foreach (Move move in moves)
            {
                if (move.isCapture)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Move> GenerateMoves(GameState state, PieceView.Color color)
        {
            List<Move> output = new();

            bool containsCaptures = false;

            foreach (byte key in state.board.Keys)
            {
                if (color == PieceView.Color.RED && IsRed(state.board[key]))
                {
                    List<Move> moves = GenerateMovesForPiece(state, key);
                    if (!containsCaptures)
                    {
                        foreach (Move move in moves)
                        {
                            if (move.isCapture)
                            {
                                containsCaptures = true;
                            }

                            break;
                        }
                    }
                    output.AddRange(moves);
                }
                if (color == PieceView.Color.BLACK && IsBlack(state.board[key]))
                {
                    List<Move> moves = GenerateMovesForPiece(state, key);
                    if (!containsCaptures)
                    {
                        foreach (Move move in moves)
                        {
                            if (move.isCapture)
                            {
                                containsCaptures = true;
                            }

                            break;
                        }
                    }
                    output.AddRange(moves);
                }
            }

            if (containsCaptures)
            {
                for (int i = output.Count - 1; i >= 0; i--)
                {
                    if (!output[i].isCapture)
                    {
                        output.RemoveAt(i);
                    }
                }
            }

            return output;
        }

        public static List<Move> GenerateMovesForPiece(GameState state, byte key)
        {
            List<Move> output = new();

            // List captures first because they will have the better scores
            output.AddRange(GenerateCapturesForPiece(state, key));

            // If there are captures, return early so that the only moves are captures
            if (output.Count > 0) {
                return output;
            }

            int[] coordinates = ByteToInts(key);
            char piece = state.board[key];

            int[,] directions = { };

            if (piece == 0b00)
            {
                directions = new int[,]
                {
                    { 1, 1 },
                    { 1, -1 }
                };
            }
            else if (piece == 0b10)
            {
                directions = new int[,]
                {
                    { -1, 1 },
                    { -1, -1 }
                };
            }
            else if (piece == 0b01 || piece == 0b11)
            {
                directions = new int[,]
                {
                    { 1, 1 },
                    { 1, -1 },
                    { -1, 1 },
                    { -1, -1 }
                };
            }

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int[] dir = new int[] { directions[i, 0], directions[i, 1] };

                // Local coordinates
                int[] lc = AddAll(coordinates, dir);    // Local Coordinates
                // If coordinate is invalid, move to the next option
                if (lc[0] < 0 || lc[0] >= GameManager.BoardSize || lc[1] < 0 || lc[1] >= GameManager.BoardSize)
                {
                    continue;
                }
                byte lcb = IntsToByte(lc[0], lc[1]);    // Local Coordinates Byte

                if (!state.board.ContainsKey(lcb))
                {
                    Move m = new(key, lcb);

                    output.Add(m);
                }
            }

            return output;
        }

        public static List<Move> GenerateCapturesForPiece(GameState state, byte key)
        {
            List<Move> output = new();

            int[] coordinates = ByteToInts(key);
            char piece = state.board[key];

            int[,] directions = { };

            if (piece == 0b00)
            {
                directions = new int[,]
                {
                    { 1, 1 },
                    { 1, -1 }
                };
            }
            else if (piece == 0b10)
            {
                directions = new int[,]
                {
                    { -1, 1 },
                    { -1, -1 }
                };
            }
            else if (piece == 0b01 || piece == 0b11)
            {
                directions = new int[,]
                {
                    { 1, 1 },
                    { 1, -1 },
                    { -1, 1 },
                    { -1, -1 }
                };
            }

            for (int i = 0; i < directions.GetLength(0); i++)
            {
                int[] dir = new int[] { directions[i, 0], directions[i, 1] };

                // Local coordinates
                int[] lc = AddAll(coordinates, dir);    // Local Coordinates
                byte lcb = IntsToByte(lc[0], lc[1]);    // Local Coordinates Byte

                if (state.board.ContainsKey(lcb))
                {
                    int[] nlc = AddAll(lc, dir);                // New Local Coordinates
                    // If coordinate is invalid, move to the next option
                    if (nlc[0] < 0 || nlc[0] >= GameManager.BoardSize || nlc[1] < 0 || nlc[1] >= GameManager.BoardSize)
                    {
                        continue;
                    }
                    byte nlcb = IntsToByte(nlc[0], nlc[1]);     // New Local Coordinates Byte

                    if (!state.board.ContainsKey(nlcb) && IsOpponent(piece, state.board[lcb]))
                    {
                        Move m = new(key, nlcb);
                        m.isCapture = true;
                        m.captures.Add(lcb);

                        GameState next = GameState.ApplyMove(state, m);

                        // List secondary moves first because they will have higher scores
                        List<Move> secondaryMoves = GenerateCapturesForPiece(next, nlcb);

                        foreach (Move sm in secondaryMoves)
                        {
                            Move extendedMove = new Move(key, sm.end);
                            extendedMove.isCapture = true;
                            extendedMove.captures.AddRange(m.captures);
                            extendedMove.captures.AddRange(sm.captures);

                            output.Add(extendedMove);
                        }

                        output.Add(m);
                    }
                }
            }

            return output;
        }

        public static int Evaluate(GameState state)
        {
            int score = 0;

            // +3 + i for each AI piece (+10 for king), -3 + i for each Human piece (-10 for king) where i is the 8 - the distance to becoming a king
            // +1 for each AI piece on the edge, -1 for each Human piece on the edge (edge pieces cannot be captured)
            for (int r = 0; r < GameManager.BoardSize; r++)
            {
                for (int c = 0; c < GameManager.BoardSize; c++)
                {
                    byte coordinate = IntsToByte(r, c);

                    bool containsBlack = false, containsRed = false;

                    if (state.board.ContainsKey(coordinate) && IsRed(state.board[coordinate]))
                    {
                        if (IsKing(state.board[coordinate]))
                        {
                            score += 20;
                        }
                        else
                        {
                            score += 10 - r;
                        }

                        if (r == 0 || r == GameManager.BoardSize - 1 || c == 0 || c == GameManager.BoardSize - 1)
                        {
                            score += 1;
                        }

                        containsRed = true;
                    }

                    // If loss for red, skew heavily in black's direction
                    if (!containsRed)
                    {
                        score -= 100;
                    }

                    if (state.board.ContainsKey(coordinate) && IsBlack(state.board[coordinate]))
                    {
                        if (IsKing(state.board[coordinate]))
                        {
                            score -= 20;
                        }
                        else
                        {
                            score -= 3 + r;
                        }

                        if (r == 0 || r == GameManager.BoardSize - 1 || c == 0 || c == GameManager.BoardSize - 1)
                        {
                            score -= 1;
                        }

                        containsBlack = true;
                    }

                    // If loss for black, skew heavily in red's direction
                    if (!containsBlack)
                    {
                        score += 100;
                    }
                }
            }

            return score;
        }
    }
}