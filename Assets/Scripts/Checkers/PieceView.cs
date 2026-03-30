using UnityEngine;

namespace Checkers
{
    public class PieceView : MonoBehaviour
    {
        public enum Color
        {
            RED,
            BLACK
        }
        public Color color;
        public bool isKing;
        public GameObject kingMarker;

        public bool selected = false;
        private bool wasSelected = false;

        bool WasSelectedThisFrame => selected && selected != wasSelected;

        bool WasDeselectedThisFrame => !selected && selected != wasSelected;

        Coroutine riseAnimationInstance;

        public byte coordinate;

        // Call this function to move a piece to a new coordinate
        public void MoveTo(byte coordinate)
        {
            Vector3 originalPosition = transform.position;

            StartCoroutine(Utils.LerpAsync(0, 1, GameManager.MoveTime, (float t) =>
            {
                transform.position = Vector3.Lerp(
                    originalPosition,
                    Utils.ByteToWorldSpace(coordinate, GameManager.Spacing),
                    t
                );
            }));

            this.coordinate = coordinate;
        }

        void Awake()
        {
            kingMarker.SetActive(false);
        }

        void Update()
        {
            if (WasSelectedThisFrame)
            {
                if (riseAnimationInstance != null)
                {
                    StopCoroutine(riseAnimationInstance);
                }

                riseAnimationInstance = StartCoroutine(Utils.LerpAsync(transform.position.y, 1, 0.5f, nv =>
                {
                    transform.position = new(
                        transform.position.x,
                        nv,
                        transform.position.z
                    );
                }));
            }

            if (WasDeselectedThisFrame)
            {
                if (riseAnimationInstance != null)
                {
                    StopCoroutine(riseAnimationInstance);
                }
                
                riseAnimationInstance = StartCoroutine(Utils.LerpAsync(transform.position.y, 0, 0.5f, nv =>
                {
                    transform.position = new(
                        transform.position.x,
                        nv,
                        transform.position.z
                    );
                }));
            }

            wasSelected = selected;
            selected = false;
        }

        public void King()
        {
            isKing = true;
            kingMarker.SetActive(true);
        }
    }
}