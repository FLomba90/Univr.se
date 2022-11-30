using UnityEngine;

namespace Abstractions.Enums
{
    public enum SideSelectionCellPosition
    {
        None = 0,
        Left = 1,
        Right
    }

    public static class SideSelectionCellPositionExtensions
    {
        public static SideSelectionCellPosition GetSideSelectionCellPositionFromUserInput(this SideSelectionCellPosition currentPos, KeyCode code)
        {
            switch (currentPos)
            {
                case SideSelectionCellPosition.Left:
                    switch (code)
                    {
                        case KeyCode.LeftArrow:
                            return SideSelectionCellPosition.Left;
                        case KeyCode.RightArrow:
                            return SideSelectionCellPosition.None;
                    }
                    break;
                case SideSelectionCellPosition.None:
                    switch (code)
                    {
                        case KeyCode.LeftArrow:
                            return SideSelectionCellPosition.Left;
                        case KeyCode.RightArrow:
                            return SideSelectionCellPosition.Right;
                    }
                    break;
                case SideSelectionCellPosition.Right:
                    switch (code)
                    {
                        case KeyCode.LeftArrow:
                            return SideSelectionCellPosition.None;
                        case KeyCode.RightArrow:
                            return SideSelectionCellPosition.Right;
                    }
                    break;
            }
            return SideSelectionCellPosition.None;
        }
    }
}

