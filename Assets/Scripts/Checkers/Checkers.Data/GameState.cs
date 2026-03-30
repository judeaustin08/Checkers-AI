using System.Collections.Generic;

using static Checkers.Utils;

namespace Checkers.Data
{
    public class GameState
    {
        // The board is represented as a 2D array of chars.
        // 0b00 = black piece
        // 0b01 = black king
        // 0b10 = red piece
        // 0b11 = red king
        // chars are used instead of ints to save space.
        public Dictionary<byte, char> board = new();

        public bool aiTurn;
        public bool IsGameOver => GameOver(this);

        public GameState(Dictionary<byte, char> board, bool aiTurn)
        {
            this.board = board;
            this.aiTurn = aiTurn;
        }

        public static GameState ApplyMove(GameState state, Move m)
        {
            Dictionary<byte, char> outBoard = new(state.board);

            outBoard.Remove(m.start);
            outBoard[m.end] = state.board[m.start];

            if (m.isCapture)
            {
                foreach (byte b in m.captures)
                {
                    outBoard.Remove(b);
                }
            }

            // Make piece a king if it has reached the end
            int[] coords = ByteToInts(m.end);
            if (outBoard[m.end] == 0b00 && coords[0] == GameManager.BoardSize - 1)
            {
                outBoard[m.end] = (char)0b01;
            }
            if (outBoard[m.end] == 0b10 && coords[0] == 0)
            {
                outBoard[m.end] = (char)0b11;
            }

            return new(outBoard, !state.aiTurn);
        }

        public static bool GameOver(GameState state)
        {
            int numB = 0;
            int numR = 0;

            foreach (byte key in state.board.Keys)
            {
                if (IsBlack(state.board[key]))
                {
                    numB++;
                }
                if (IsRed(state.board[key]))
                {
                    numR++;
                }
            }

            return numB == 0 || numR == 0;
        }
    }
}
