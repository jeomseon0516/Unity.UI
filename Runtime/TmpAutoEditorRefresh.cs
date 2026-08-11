using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Jeomseon.Attribute;
using UnityEngine.Serialization;

namespace Jeomseon.UI
{
    [ExecuteAlways]
    public class TmpAutoEditorRefresh : MonoBehaviour
    {
        [SerializeField, GetOrAddComponent, FormerlySerializedAs("_targetText")] private TextMeshProUGUI targetText;
        [SerializeField, GetOrAddComponent, FormerlySerializedAs("_contentToRefresh")] private RectTransform contentToRefresh;

        private string _lastValue;

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (targetText == null) return;

                if (_lastValue != targetText.text)
                {
                    _lastValue = targetText.text;
                    Refresh();
                }
            }
        }

        private void Refresh()
        {
            targetText.ForceMeshUpdate();

            LayoutRebuilder.ForceRebuildLayoutImmediate(targetText.rectTransform);

            if (contentToRefresh != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentToRefresh);

            Canvas.ForceUpdateCanvases();
        }
    }
}
