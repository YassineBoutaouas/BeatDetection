# BeatDetection

The project was developed as an extension to the game [(G)raveyard](https://github.com/YassineBoutaouas/Graveyard) - a combat rhythm game. 
It provides a multi-part editor that allows for audio configuration.

## Runtime classes
- SoundElement : ScriptableObject - extends the built-in AudioClip. It stores a BPM as well as a List of SoundEvents and an animation curve to allow for easing in between two beats 
(e.g.: Could be used to widen the input window in a rhythm game - allows the user to press the button slighly before or after a beat).

- SoundEvent - (similar to animation event) - stores values that are passed through a method corresponding to the name that it stores. [See](https://docs.unity3d.com/ScriptReference/GameObject.SendMessage.html). 
Each SoundElement stores a timestamp in order for it to be invoked when the AudioClip.time is passed.

- SoundPlayer : MonoBehaviour - stores a reference to a SoundElement. The class iterates through the audio events and invokes the event whose time stamp is reached by the AudioClip.time. 
It also stores a float value corresponding to the SoundElement.AnimationCurve.Evaluate.

- RhythmReader - provides an algorithm to calculate the BPM of a SoundElement.AudioClip.

## Editors
- CreateSoundElementWindow : EditorWindow - allows to create a SoundElement.asset at a given path with a given name and an audio clip.

- SoundEditor : EditorWindow - contains common user interfaces with audio controls such as playback buttons, volume sliders and handles.

- SoundEditorWindow : SoundEditor - extends the sound editor interfaces and provides a container to add and remove sound events. It also provides an inspector to inspect and modify the values of a selected sound event.

- ConfigureRhythmWindow : SoundEditor - sound editor extensions that allow to configure the BPM of an audioclip with two different methods:
    1. allows to automatically calculate the BPM of a track using the RhythmReader.CalculateBPM
    2. provides an interface to record beats over a given time period - Tap system

### Directories
Runtime classes .../Assets/Scripts/Runtime

Editor classes .../Assets/Scripts/Editor

UXML and USS classes .../Assets/Scripts/Editor/UI

**To access the UXML and USS classes the project uses constant data paths. Changing the folder structure requires to change the associated paths.**