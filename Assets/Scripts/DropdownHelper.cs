using UnityEngine;
using TMPro;

namespace Checkers {
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DropdownHelper : MonoBehaviour
    {
        TMP_Dropdown _ddn;

        void Awake()
        {
            _ddn = GetComponent<TMP_Dropdown>();
        }

        public string GetSelectedFromDropdown()
        {
            return _ddn.options[_ddn.value].text;
        }

        public void SetSearchDepthFromSelected()
        {
            string name = GetSelectedFromDropdown();
            int depth = int.Parse(name);
            GameManager.instance.SetSearchDepth(depth);
        }
    }
}
