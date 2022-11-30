using System;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "AppResources", menuName = "App/Resources", order = 1)]
    [Serializable]
    public class AppResources : ScriptableObject
    {
        public MatchmackingResources Matchmaking;
        public GameResources GameResources;
        public SharedUiResources Ui;
    }
    [Serializable]
    public class MatchmackingResources
    {
        public string ConnectTitle;
        public string ConnectDescription;
        public string ConnectButtonText;
        public string ErrorTitle;
        public string DefaultErrorDescription;
        public string CloseButtonText;
        public string MatchCompleted;
        public string TeamIsWinner;
        public string TeamIsLoser;
        public string ErrorButtonText;
        public string ConfirmTeamSelectionButtonText;
        public string UndoTeamSelectionButtonText;
    }   
    [Serializable]
    public class GameResources
    {
        public string Team1Name;
        public Sprite Team1Icon;
        public string Team2Name;
        public Sprite Team2Icon;
    }
    [Serializable]
    public class SharedUiResources
    {
        public Color Pink;
        public Color Blue;
        public Color Error;
        public Sprite ControllerIcon;
        public Sprite ControllerPlaceholderIcon;
        public Sprite LeftArrowIcon;
        public Sprite RightArrowIcon;
    }
}