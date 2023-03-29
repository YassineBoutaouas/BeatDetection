#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SoundElements.Editor
{
    [CustomEditor(typeof(SoundPlayer), true)]
    public class SoundPlayerEditor : UnityEditor.Editor
    {
        private SoundPlayer _soundPlayer;

        private void OnEnable()
        {
            _soundPlayer = (SoundPlayer)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(GUILayout.Button("Calculate BPM"))
                _soundPlayer.CalculateBPM();
        }
    }
}
#endif