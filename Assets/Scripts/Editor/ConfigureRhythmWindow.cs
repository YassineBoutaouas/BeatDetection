#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SoundElements.Editor
{
    public class ConfigureRhythmWindow : SoundEditor
    {
        #region Static members
        public static void OpenWindow(SerializedObject obj, SoundElement soundElement, VisualElement waveFormContainer, float[] waves, float[] samples, int sampleSize, Rect pos)
        {
            ConfigureRhythmWindow wnd = CreateWindow<ConfigureRhythmWindow>("Rhythm Editor");


            wnd.CreateSoundObject();
            wnd._soundElement = soundElement;
            wnd._audioSource.clip = soundElement.AudioClip;

            wnd.SetPaths("Assets/Scripts/Editor/UI/ConfigureRhythmEditor.uxml", "Assets/Scripts/Editor/UI/ConfigureRhythm_Style.uss");

            wnd.position = pos;
            wnd._serializedObject = obj;
            wnd.Focus();

            wnd.InitFields();

            #region Waveform Container Cloning
            Texture2D waveFormTex = new Texture2D(wnd._waveFormResolution.x, wnd._waveFormResolution.y);
            Graphics.CopyTexture(waveFormContainer.style.backgroundImage.value.texture, waveFormTex);

            wnd._waveFormContainer.style.width = waveFormContainer.style.width;

            Background bg = waveFormContainer.style.backgroundImage.value.texture;
            wnd._waveFormContainer.style.backgroundImage = bg;
            wnd._waveFormContainer.pickingMode = PickingMode.Ignore;
            #endregion
        }
        #endregion

        private float _upperBound;
        private float _lowerBound;

        protected override void InitFields()
        {
            base.InitFields();
            _audioField.value = _soundElement.Name;

            SetEvents();
        }
    }
}
#endif