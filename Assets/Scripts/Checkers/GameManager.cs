using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Checkers
{
    using AI;
    using Data;
    using TMPro;
    using UnityEngine.SceneManagement;

    public partial class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        // Static variables
        public float spacing = 5f;
        public static float Spacing => instance.spacing;
        public int boardSize = 8;
        public static int BoardSize => instance.boardSize;

        // The time in seconds it takes for a piece to move from one position to another
        public float moveTime = 1f;
        public static float MoveTime => instance.moveTime;

        public GameObject marker;
        public static GameObject Marker => instance.marker;


        // Instance variables
        InputActions input;

        List<GameObject> moveMarkers = new();

        public GameObject blackPiecePrefab;
        public GameObject redPiecePrefab;

        public bool versusAI = false;
        public int searchDepth = 6;

        public static Dictionary<byte, GameObject> pieces;

        public static GameState state;

        public GameObject startingUI;
        public GameObject gameOverUI;
        public TMP_Text winText;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            input = new();
        }

        void OnEnable()
        {
            input.Enable();
        }

        void OnDisable()
        {
            input.Disable();
        }

        void Start()
        {
            gameOverUI.SetActive(false);
        }

        public void StartGame()
        {
            startingUI.SetActive(false);
            InitializeBoard();
            InitializeData();
            StartCoroutine(TakeTurn(false));
        }

        public void SetSearchDepth(int i)
        {
            searchDepth = i;
        }

        public void SetVersusAI(bool b)
        {
            versusAI = b;
        }

        /// <summary>
        /// Places all starting pieces on the board
        /// </summary>
        public void InitializeBoard()
        {
            pieces = new();

            Debug.Log("Placing black pieces");
            for (int i = 0; i < 3; i++)
            {
                PlaceRow(i, blackPiecePrefab);
            }

            Debug.Log("Placing red pieces");
            for (int i = BoardSize - 1; i >= BoardSize - 3; i--)
            {
                PlaceRow(i, redPiecePrefab);
            }
        }

        /// <summary>
        /// Places a single row of starting pieces
        /// </summary>
        /// <param name="row"></param>
        /// <param name="prefab"></param>
        public void PlaceRow(int row, GameObject prefab)
        {
            for (int i = 0; i < BoardSize; i += 2)
            {
                byte coordinate = Utils.IntsToByte(row, i + row % 2);
                PlacePiece(coordinate, prefab);
            }
        }

        /// <summary>
        /// Places a single starting piece at a specified coordinate
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="prefab"></param>
        public void PlacePiece(byte coordinate, GameObject prefab)
        {
            Vector3 position = Utils.ByteToWorldSpace(coordinate, Spacing);
            GameObject go = Instantiate(prefab, position, Quaternion.identity);
            go.GetComponent<PieceView>().coordinate = coordinate;
            pieces.Add(coordinate, go);
        }

        /// <summary>
        /// Represents the board in the GameState as chars.
        /// Must be called AFTER InitializeBoard()
        /// </summary>
        public void InitializeData()
        {
            Dictionary<byte, char> board = new();

            foreach (byte coordinate in pieces.Keys)
            {
                char pieceChar;
                PieceView piece = pieces[coordinate].GetComponent<PieceView>();

                if (piece.color == PieceView.Color.RED)
                {
                    if (piece.isKing) pieceChar = (char)0b11;
                    else pieceChar = (char)0b10;
                }
                else
                {
                    if (piece.isKing) pieceChar = (char)0b01;
                    else pieceChar = (char)0b00;
                }

                board.Add(coordinate, pieceChar);
            }

            // Assume human's turn is first
            state = new(board, false);
        }

        IEnumerator TakeTurn(bool ai)
        {
            Debug.Log($"Taking turn. AI = {ai}");

            if (state.IsGameOver)
            {
                GameOver();
            }

            if (ai)
            {
                yield return AISelectTurn(PieceView.Color.RED);
            }
            else
            {
                if (versusAI)
                {
                    yield return AISelectTurn(PieceView.Color.BLACK);
                }
                else
                {
                    yield return PlayerSelectTurn(PieceView.Color.BLACK);
                }
            }

            yield return new WaitForSeconds(MoveTime);

            yield return TakeTurn(!ai);
        }

        IEnumerator PlayerSelectTurn(PieceView.Color color)
        {
            bool selectingTurn = true;
            PieceView selectedPiece = null;
            Move move = new(0x00, 0x00);

            while (selectingTurn)
            {
                // A ray pointing from the camera to the worldspace position where the mouse clicked
                Ray ray = Camera.main.ScreenPointToRay(input.Player.MousePosition.ReadValue<Vector2>());
                RaycastHit hit;
                bool rc_success = Physics.Raycast(ray, out hit);

                if (rc_success && hit.collider.gameObject.TryGetComponent(out PieceView piece) && piece.color == PieceView.Color.BLACK)
                {
                    piece.selected = true;
                }

                if (input.Player.Select.WasPressedThisFrame())
                {
                    // Check move markers before they're destroyed
                    if (rc_success && hit.collider.gameObject.TryGetComponent(out MoveMarker marker))
                    {
                        selectingTurn = false;
                        move = marker.move;
                    }

                    // Deselect the current piece
                    selectedPiece = null;
                    foreach (GameObject obj in moveMarkers)
                    {
                        Destroy(obj);
                    }

                    if (rc_success && hit.collider.gameObject.TryGetComponent(out PieceView p) && p.color == color)
                    {
                        selectedPiece = p;

                        List<Move> moves = Bot.GenerateMovesForPiece(state, selectedPiece.coordinate);

                        if (Bot.CaptureIsPossible(state, color))
                        {
                            Debug.Log("Capture is possible");
                            for (int i = moves.Count - 1; i >= 0; i--)
                            {
                                if (!moves[i].isCapture)
                                {
                                    moves.RemoveAt(i);
                                }
                            }
                        }

                        foreach (Move m in moves)
                        {
                            moveMarkers.Add(m.CreateFinalPositionMarker());
                        }
                    }
                }

                if (selectedPiece != null)
                {
                    selectedPiece.selected = true;
                }

                yield return null;
            }

            ExecuteMove(move);
        }

        IEnumerator AISelectTurn(PieceView.Color color)
        {
            BotReturnData data = Bot.Minimax(state, searchDepth, color);
            Move bestMove = data.move;
            // If stalemate
            if (bestMove == null)
            {
                GameOver();
                yield return null;
            }
            ExecuteMove(bestMove);
            Debug.Log($"Nodes visited = {Bot.nodesVisited}");
            Debug.Log($"Best value: {data.value}");
        }

        void GameOver()
        {
            Debug.Log("Game over");
            StopAllCoroutines();

            // Count each type of piece
            int blackCount = 0, redCount = 0;
            foreach (byte b in pieces.Keys)
            {
                if (pieces[b].GetComponent<PieceView>().color == PieceView.Color.BLACK) blackCount++;
                if (pieces[b].GetComponent<PieceView>().color == PieceView.Color.RED) redCount++;
            }

            if (blackCount > 0 && redCount == 0)
            {
                winText.text = "Black wins!";
            }
            else if (blackCount == 0 && redCount > 0)
            {
                winText.text = "Red wins!";
            }
            else
            {
                winText.text = "Draw";
            }

            gameOverUI.SetActive(true);
        }

        public void Restart()
        {
            SceneManager.LoadScene(0);
        }
    }
}