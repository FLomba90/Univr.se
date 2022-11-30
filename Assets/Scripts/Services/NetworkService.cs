using Abstractions;
using Abstractions.Enums;
using Controllers;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Settings;
using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using Zenject;
using static Assets.Scripts.Utils.Utils;

namespace Services
{
    public interface INetworkService
    {
        event Action<(bool, AppNetworkInternalError)> ConnectionResult;
        event Action<AppNetworkInternalError> Disconnected;
        event Action<Hashtable> RoomPropertiesUpdated;
        event Action<(int, Hashtable)> PlayerPropertiesUpdated;
        event Action<int> PlayerEnteredRoom;
        event Action<int> PlayerLeftRoom;
        event Action<EventData> NetworkEvent;

        void StartMatch();
        void StartGame();
        void LeaveGame();
        bool TryGetPlayerCustomProperty<T>(int playerID, object key, out T value);
        bool TryGetCustomProperty<T>(Hashtable properties, object key, out T value);
        bool TryGetPlayerFromId(int id, out Player player);
        void ClearPlayerCustomProperties()
        {
            Hashtable props = new Hashtable();
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        void SetPlayerCustomProperty(int playerId, object key, object value);
        void SetLocalPlayerCustomProperty(object key, object value);
        void SendEvent(byte eventCode, object[] data, ReceiverGroup receiverGroup, EventCaching caching);
        void SendEventToSpecificTargets(byte eventCode, object[] data, int[] targetPlayerIDs, EventCaching caching);
        int TurnGoInSynchronizedSceneObject(GameObject target, int networkID = -1);
    }
    public class NetworkService : ADisposablePunMonoBehaviour, INetworkService
    {
        #region resources

        protected override string LOG_TAG => nameof(NetworkService);

        #endregion resources

        #region data

        public event Action<(bool, AppNetworkInternalError)> ConnectionResult;
        public event Action<AppNetworkInternalError> Disconnected;
        public event Action<Hashtable> RoomPropertiesUpdated;
        public event Action<(int, Hashtable)> PlayerPropertiesUpdated;
        public event Action<int> PlayerEnteredRoom;
        public event Action<int> PlayerLeftRoom;
        public event Action<EventData> NetworkEvent;

        #endregion data

        #region dependency injection
        [Inject]
        readonly AppNetworkSettings m_appNetworkSettings;

        #endregion dependency injection

        #region monobehaviour callbacks
        #endregion monobehaviour callbacks

        #region abstractions
        public void StartMatch()
        {
            PhotonNetwork.LoadLevel(1);
        }
        public void ClearPlayerCustomProperties()
        {
            Hashtable props = new Hashtable();
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        public void SetPlayerCustomProperty(int playerId, object key, object value)
        {
            if (!PhotonNetwork.CurrentRoom.Players.ContainsKey(playerId)) { return; }
            Hashtable props = PhotonNetwork.CurrentRoom.Players[playerId].CustomProperties;
            if (props.ContainsKey(key))
            {
                props[key] = value.ToString();
            }
            else
            {
                props.Add(key, value.ToString());
            }
            PhotonNetwork.CurrentRoom.Players[playerId].SetCustomProperties(props);
        }
        public int TurnGoInSynchronizedSceneObject(GameObject target, int networkID = -1)
        {
            var pv = target.AddComponent<PhotonView>();
            if (networkID == -1)
            {
                PhotonNetwork.AllocateRoomViewID(pv);
            }
            else
            {
                pv.ViewID = networkID;
            }

            pv.OwnershipTransfer = OwnershipOption.Fixed;
            return pv.ViewID;
        }
        public void SetLocalPlayerCustomProperty(object key, object value)
        {
            Hashtable props = PhotonNetwork.LocalPlayer.CustomProperties;
            if (props.ContainsKey(key))
            {
                props[key] = value.ToString();
            }
            else
            {
                props.Add(key, value.ToString());
            }
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }
        public void SendEvent(byte eventCode, object[] data, ReceiverGroup receiverGroup, EventCaching caching)
        {
            if (PhotonNetwork.IsConnected)
            {
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    Receivers = receiverGroup,
                    CachingOption = caching,
                };

                PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
            }
        }
        public void SendEventToSpecificTargets(byte eventCode, object[] data, int[] targetPlayerIDs, EventCaching caching)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                TargetActors = targetPlayerIDs,
                Receivers = ReceiverGroup.Others,
                CachingOption = caching,
            };

            PhotonNetwork.RaiseEvent(eventCode, data, raiseEventOptions, SendOptions.SendReliable);
        }
        public bool TryGetPlayerFromId(int id, out Player player)
        {
            player = null;
            if (!PhotonNetwork.IsConnected || !PhotonNetwork.CurrentRoom.Players.ContainsKey(id)) {
                Debug.LogError($"Unknown player {id} on property get");
                return false; 
            }
            player = PhotonNetwork.CurrentRoom.Players[id];
            return true;
        }
        public void StartGame()
        {
            ConnectToServer();
        }
        public void LeaveGame()
        {
            DisconnectToServer();
        }
        public bool TryGetPlayerCustomProperty<T>(int playerID, object key, out T value)
        {
            value = default;
            if (!PhotonNetwork.IsConnected) { return false; }
            try
            {
                var player = PhotonNetwork.PlayerList.ToList().Find(x => x.ActorNumber == playerID);
                if (player != null && player.CustomProperties.TryGetValue(key, out var objectResult))
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter != null)
                    {
                        value = (T)converter.ConvertFrom(objectResult);
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }
        public bool TryGetCustomProperty<T>(Hashtable properties, object key, out T value)
        {
            value = default;
            try
            {
                if (properties.TryGetValue(key, out var objectResult))
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter != null)
                    {
                        value = (T)converter.ConvertFrom(objectResult);
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }
        #endregion abstractions

        #region callbacks
        public override void OnConnectedToMaster()
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnConnectedToMaster)}");
            
            base.OnConnectedToMaster();
            JoinGameLobby();
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnDisconnected)}: {cause}");

            base.OnDisconnected(cause);
            Disconnected?.Invoke(new AppNetworkInternalError(cause, cause.ToString()));
        }
        public override void OnJoinedLobby()
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnJoinedLobby)}");

            base.OnLeftLobby();
            JoinRandomRoom();
        }
        public override void OnLeftLobby()
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnLeftLobby)}");

            base.OnLeftLobby();
            PhotonNetwork.Disconnect();
        }
        public override void OnEvent(EventData photonEvent)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnEvent)}");

            base.OnEvent(photonEvent);
            NetworkEvent?.Invoke(photonEvent);
        }
        public override void OnLeftRoom()
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnLeftRoom)}");

            base.OnLeftRoom();
            PhotonNetwork.Disconnect();
        }
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnJoinedRoom)}");

            base.OnPlayerLeftRoom(otherPlayer);
            PlayerLeftRoom?.Invoke(otherPlayer.ActorNumber);
        }
        public override void OnJoinedRoom()
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnJoinedRoom)}");

            base.OnJoinedRoom();
            ConnectionResult?.Invoke((true, null));
        }
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnJoinRoomFailed)}");

            base.OnJoinRoomFailed(returnCode, message);
            var internalError = GetAppErrorFromPunErrorCode(returnCode);
            ConnectionResult?.Invoke((false, new AppNetworkInternalError(internalError, m_appNetworkSettings.GetAppNetworkErrorMessage(internalError))));
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnJoinRandomFailed)}");

            base.OnJoinRandomFailed(returnCode, message);
            CreateAndJoinRoom();
        }
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnPlayerEnteredRoom)}");

            base.OnPlayerEnteredRoom(newPlayer);
            PlayerEnteredRoom?.Invoke(newPlayer.ActorNumber);
        }
        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnPlayerPropertiesUpdate)}");

            base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
            PlayerPropertiesUpdated?.Invoke((targetPlayer.ActorNumber, changedProps));
        }
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            Debug.Log($"{LOG_TAG}.{nameof(OnRoomPropertiesUpdate)}");

            base.OnRoomPropertiesUpdate(propertiesThatChanged);
            RoomPropertiesUpdated?.Invoke(propertiesThatChanged);
        }

        #endregion callbacks

        #region logic

        void ConnectToServer()
        {
            PhotonNetwork.LocalPlayer.NickName = m_appNetworkSettings.Pun.UserPrefix + new System.Random().Next(m_appNetworkSettings.Pun.MinRandom, m_appNetworkSettings.Pun.MaxRandom);
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.ConnectUsingSettings();
        }
        void DisconnectToServer()
        {
            if (PhotonNetwork.IsConnected)
            {
                PhotonNetwork.Disconnect();
            }
            else
            {
                Disconnected?.Invoke(null);
            }
        }
        void JoinGameLobby()
        {
            PhotonNetwork.JoinLobby(new TypedLobby(m_appNetworkSettings.Pun.Lobby, LobbyType.Default));
        }
        void JoinRandomRoom()
        {
            PhotonNetwork.JoinRandomRoom(null, (byte)m_appNetworkSettings.Pun.MaxPlayers, MatchmakingMode.FillRoom, PhotonNetwork.CurrentLobby, null);
        }
        void CreateAndJoinRoom()
        {
            string roomName = m_appNetworkSettings.Pun.RoomPrefix + new System.Random().Next(m_appNetworkSettings.Pun.MinRandom, m_appNetworkSettings.Pun.MaxRandom);

            RoomOptions options = new RoomOptions
            {
                IsOpen = true,
                IsVisible = true,
                MaxPlayers = (byte)m_appNetworkSettings.Pun.MaxPlayers
            };

            PhotonNetwork.CreateRoom(roomName, options, PhotonNetwork.CurrentLobby);
        }

        AppNetworkError GetAppErrorFromPunErrorCode(int errorCode)
        {
            if (errorCode.Equals(-3)) return AppNetworkError.OperationNotAllowedInCurrentState;
            if (errorCode.Equals(-2)) return AppNetworkError.InvalidOperation;
            if (errorCode.Equals(-1)) return AppNetworkError.InternalServerError;
            if (errorCode.Equals(32762)) return AppNetworkError.ServerFull;
            if (errorCode.Equals(32766)) return AppNetworkError.GameIdAlreadyExists;
            if (errorCode.Equals(32752)) return AppNetworkError.PluginReportedError;
            if (errorCode.Equals(32751)) return AppNetworkError.PluginMismatch;
            if (errorCode.Equals(32742)) return AppNetworkError.SlotError;
            if (errorCode.Equals(32765)) return AppNetworkError.GameFull;
            if (errorCode.Equals(32764)) return AppNetworkError.GameClosed;
            if (errorCode.Equals(32758)) return AppNetworkError.GameDoesNotExist;
            if (errorCode.Equals(32750)) return AppNetworkError.JoinFailedPeerAlreadyJoined;
            if (errorCode.Equals(32749)) return AppNetworkError.JoinFailedFoundInactiveJoiner;
            if (errorCode.Equals(32748)) return AppNetworkError.JoinFailedWithRejoinerNotFound;
            if (errorCode.Equals(32747)) return AppNetworkError.JoinFailedFoundExcludedUserId;
            if (errorCode.Equals(32746)) return AppNetworkError.JoinFailedFoundActiveJoiner;
            return AppNetworkError.Unknown;
        }
        #endregion logic
    }
}