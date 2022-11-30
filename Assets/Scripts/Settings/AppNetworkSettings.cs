using Abstractions.Enums;
using Photon.Realtime;
using System;
using UnityEngine;

namespace Settings
{
    [CreateAssetMenu(fileName = "AppNetworkSettings", menuName = "App/NetworkResources", order = 3)]
    [Serializable]
    public class AppNetworkSettings : ScriptableObject
    {
        public Pun Pun;
        public Game Game;
        public Events Events;

        public string GetAppNetworkErrorMessage(AppNetworkError error)
        {
            switch (error)
            {
                case AppNetworkError.AllPlayersLeft:
                    return Game.ErrorMessage_AllPlayersGone;
                    // todo for remaining cases
            }
            return "";
        }
        public string GetAppNetworkErrorMessage(DisconnectCause error)
        {
            switch (error)
            {
                    // todo for all cases
            }
            return "Something went wrong. Please check your network...";
        }
    }

    [Serializable]
    public class Pun
    {
        public string Lobby = "FedericoLombardelli_TheNemesisTest";
        public string UserPrefix = "punUser_";
        public string RoomPrefix = "punRoom_";
        public int MaxPlayers = 2;
        public int MaxRandom = 10000;
        public int MinRandom = 1;
    }
    [Serializable]
    public class Game
    {
        public int MaxScore = 3;
        public string PlayerCustomPropKey_CanPlay = "PlayerCanPlay";
        public string PlayerCustomPropKey_SelectionSide = "PlayerSelectionSide";
        public string PlayerCustomPropKey_SelectedTeam = "PlayerSelectedTeam";
        [Header("App Network Errors")]
        public string ErrorMessage_AllPlayersGone = "All other players left";
        [Header("Paths")]
        public string NetworkPlayerManagerPath = "PUN/NetworkPlayerManager";
        public string NetworkAvatar1Path = "PUN/NetworkAvatar1";
        public string NetworkAvatar2Path = "PUN/NetworkAvatar2";
    }
    [Serializable]
    public class Events
    {
        public byte NetworkEvent_LaunchGame = 1;
        public byte NetworkEvent_InstantiatePlayerManagerOnOtherClients= 2;
        public byte NetworkEvent_InstantiateGameItems = 3;
        public byte NetworkEvent_UpdateScore = 4;
        public byte NetworkEvent_ResetGame = 5;
        public byte NetworkEvent_MatchCompleted = 6;
    }
}