using System;
using UnityEngine;

namespace Services
{
    public interface IUserInputService
    {
        event Action<KeyCode> OnKeyboardArrowKeyDown;
    }
    public class UserInputService: MonoBehaviour, IUserInputService
    {
        #region data

        public event Action<KeyCode> OnKeyboardArrowKeyDown;

        #endregion data

        #region monobehaviour callbacks
        void Update()
        {
            CheckForArrows();
        }

        void CheckForArrows()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
                OnKeyboardArrowKeyDown?.Invoke(KeyCode.UpArrow);
            if (Input.GetKeyDown(KeyCode.DownArrow))
                OnKeyboardArrowKeyDown?.Invoke(KeyCode.DownArrow);
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnKeyboardArrowKeyDown?.Invoke(KeyCode.LeftArrow);
            if (Input.GetKeyDown(KeyCode.RightArrow))
                OnKeyboardArrowKeyDown?.Invoke(KeyCode.RightArrow);
        }
        #endregion monobehaviour callbacks
    }
}
