using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

namespace Skirmish.Net
{
    public class PlayFabLogin : MonoBehaviour
    {
        public static string EntityId;
        public static string EntityType;
        public static string PlayFabId;
        public static bool LoggedIn;

        private void Start()
        {
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
            {
                CustomId = SystemInfo.deviceUniqueIdentifier,
                CreateAccount = true,
            }, result =>
            {
                EntityId = result.EntityToken.Entity.Id;
                EntityType = result.EntityToken.Entity.Type;
                PlayFabId = result.PlayFabId;
                LoggedIn = true;
                Debug.Log($"Logged in as {PlayFabId}");
            }, error => Debug.LogError(error.GenerateErrorReport()));
        }
    }
}
