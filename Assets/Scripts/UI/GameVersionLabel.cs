using TMPro;
using UnityEngine;

namespace AnarPerPortes
{
    [AddComponentMenu("Anar per Portes/UI/Game Version Label")]
    public sealed class GameVersionLabel : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<TMP_Text>().text = "v" + Application.version;
        }
    }
}
