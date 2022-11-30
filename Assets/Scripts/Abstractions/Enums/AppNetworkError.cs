namespace Abstractions.Enums
{
    public enum AppNetworkError
    {
        None = 0,
        AllPlayersLeft = 1,
        OperationNotAllowedInCurrentState,
        InvalidOperation,
        InternalServerError,
        ServerFull,
        GameIdAlreadyExists,
        PluginReportedError,
        PluginMismatch,
        SlotError,
        GameFull,
        GameClosed,
        GameDoesNotExist,
        JoinFailedPeerAlreadyJoined,
        JoinFailedFoundInactiveJoiner,
        JoinFailedWithRejoinerNotFound,
        JoinFailedFoundExcludedUserId,
        JoinFailedFoundActiveJoiner,
        NoRandomMatchFound,
        Unknown,
    }
}

