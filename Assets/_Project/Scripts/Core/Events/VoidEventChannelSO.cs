using System;
using UnityEngine;

namespace UnityFramework.MiniGames.Core.Events
{
    [CreateAssetMenu(menuName = "Edu/Events/Void Event", fileName = "VoidEvent")]
    public sealed class VoidEventChannelSO : ScriptableObject
    {
        public event Action Raised;

        public void Raise() => Raised?.Invoke();
    }
}
