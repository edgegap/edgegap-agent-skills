using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace KartClash.Online
{
    /// <summary>
    /// Fallback while the matchmaker is flaky: deploy a server for this player directly.
    /// </summary>
    public class QuickPlay : MonoBehaviour
    {
        private const string ApiToken = "token xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
        private const string App = "kartclash";
        private const string Version = "v3";

        public void DeployFallback() => StartCoroutine(Deploy());

        private IEnumerator Deploy()
        {
            string body = "{\"app_name\":\"" + App + "\",\"version_name\":\"" + Version + "\",\"ip_list\":[]}";
            using var req = new UnityWebRequest("https://api.edgegap.com/v1/deploy", "POST");
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Authorization", ApiToken);
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();
            Debug.Log($"[QuickPlay] deploy: {req.responseCode} {req.downloadHandler.text}");
        }
    }
}
