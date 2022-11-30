using Abstractions.Enums;
using Photon.Realtime;

namespace Assets.Scripts.Utils
{
    public static class Utils
    {
        public class AppNetworkInternalError
        {
            public string ErrorMessage;
            public AppNetworkError AppNetworkError = AppNetworkError.None;
            public DisconnectCause DisconnectError = DisconnectCause.None;
            public bool HasNetworkError => !AppNetworkError.Equals(AppNetworkError.None) && DisconnectError.Equals(DisconnectCause.None);
            public bool HasDisconnectionError => AppNetworkError.Equals(AppNetworkError.None) && !DisconnectError.Equals(DisconnectCause.None);
            public AppNetworkInternalError(AppNetworkError appCustomError, string errorMessage)
            {
                AppNetworkError = appCustomError;
                ErrorMessage = errorMessage;
            }
            public AppNetworkInternalError(DisconnectCause disconnectError, string errorMessage)
            {
                DisconnectError = disconnectError;
                ErrorMessage = errorMessage;
            }
        }
    }
}