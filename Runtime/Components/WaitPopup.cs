using System.Collections;
using UnityEngine;
using TMPro;
using Jeomseon.Coroutine;
using UnityEngine.Serialization;

namespace Jeomseon.UI.Components
{
    public sealed class WaitPopup : BaseUI
    {
        [Header("Wait Text")]
        [SerializeField, FormerlySerializedAs("_waitText")]
        private TMP_Text waitText;

        protected override void EnableUI()
            => StartCoroutine(iEWaitEvent());

        private IEnumerator iEWaitEvent()
        {
            string getDotFromCount(int count) => (count % 3) switch
            {
                0 => ".",
                1 => "..",
                2 => "...",
                _ => string.Empty
            };

            int count = 0;

            while (true)
            {
                yield return CoroutineHelper.WaitForSeconds(1f);
                count++;
                waitText.text = $"Wait.. {getDotFromCount(count)}";
            }
        }
    }
}
