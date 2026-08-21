using System.Collections.Generic;
using UnityEngine;

namespace Jeomseon.Unity.UI.Channels
{
    [CreateAssetMenu(
        fileName = "UICatalog",
        menuName = "Jeomseon/UI/UI Catalog")]
    public sealed class UICatalog : ScriptableObject
    {
        [SerializeField] private List<UIScreenEntry> entries = new();

        public IReadOnlyList<UIScreenEntry> Entries => entries;
    }
}
