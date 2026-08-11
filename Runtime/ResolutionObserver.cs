using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Jeomseon.Singleton;
using UnityEngine.Serialization;

namespace Jeomseon.UI
{
    public sealed class ResolutionObserver : Singleton<ResolutionObserver>
    {
        [Header("Resolution Changed Event")]
        [SerializeField, FormerlySerializedAs("_onChangedResolution")]
        private UnityEvent<Vector2> onChangedResolution;

        private Vector2Int _lastResolution = Vector2Int.zero;

        protected override void Init()
        {
            onChangedResolution ??= new();
        }

        private void Update()
        {
            if (_lastResolution.x != Screen.width || _lastResolution.y != Screen.height)
            {
                onChangedResolution.Invoke(new(Screen.width, Screen.height));
            }

            _lastResolution = new(Screen.width, Screen.height);
        }

        public void AddListenerOnChangedResolution(UnityAction<Vector2> callback)
            => onChangedResolution.AddListener(callback);

        public void RemoveListenerOnResolutionChanged(UnityAction<Vector2> callback)
            => onChangedResolution.RemoveListener(callback);
    }
}
