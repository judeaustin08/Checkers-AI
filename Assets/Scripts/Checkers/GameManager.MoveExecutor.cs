using UnityEngine;

using Checkers.Data;

namespace Checkers
{
    public partial class GameManager : MonoBehaviour
    {
        // Executes a move visually
        public static void ExecuteMove(Move m)
        {
            if (m == null) throw new InvalidMoveException("The provided move is null.");

            if (!pieces.ContainsKey(m.start)) throw new InvalidMoveException("Start coordinate is invalid.");

            GameObject piece = pieces[m.start];

            if (m.isCapture)
            {
                foreach (byte b in m.captures)
                {
                    if (!pieces.ContainsKey(b)) throw new InvalidMoveException("Capture coordinate is invalid.");

                    GameObject captured = pieces[b];
                    Destroy(captured);
                    pieces.Remove(b);
                }
            }

            // Change the position of the piece in data
            pieces.Remove(m.start);
            pieces.Add(m.end, piece);

            // Move the piece in the world
            PieceView view = piece.GetComponent<PieceView>();
            view.MoveTo(m.end);

            // Make the piece a king if it has reached the end
            int[] coords = Utils.ByteToInts(m.end);
            if (view.color == PieceView.Color.BLACK && coords[0] == BoardSize - 1)
            {
                view.King();
            }
            if (view.color == PieceView.Color.RED && coords[0] == 0)
            {
                view.King();
            }

            // Move the piece in the AI data
            state = GameState.ApplyMove(state, m);
        }
    }
}