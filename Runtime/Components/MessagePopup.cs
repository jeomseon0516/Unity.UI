using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

namespace Jeomseon.Unity.UI.Components
{
    public sealed class MessagePopup : BaseUI
    {
        [Header("Button")]
        [SerializeField, FormerlySerializedAs("_okButton")]
        private Button okButton;

        [Header("Text")]
        [SerializeField, FormerlySerializedAs("_warningText")]
        private TMP_Text warningText;
        [SerializeField, FormerlySerializedAs("_titleText")]
        private TMP_Text titleText;

        public string WarningText
        {
            get => warningText.text;
            set => warningText.text = value;
        }

        public string TitleText
        {
            get => titleText.text;
            set => titleText.text = value;
        }

        protected override void EnableUI() { }

        private void Start()
        {
            okButton.onClick.AddListener(() => UIManager.Instance.CloseUI(this));
        }
    }
}
