using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomInput : MonoBehaviour {
    [Tooltip ("Repeat time when key held down for a short while")]
    public float repeatSpeed = .2f;
    public float delayToFirstRepeat = .5f;

    List<KeyCode> registeredKeys = new List<KeyCode> ();
    Dictionary<KeyCode, KeyState> keys = new Dictionary<KeyCode, KeyState> ();

    public static CustomInput instance;

    private void Awake () {
        instance = this;
        if (FindObjectsOfType<CustomInput>().Length > 1) {
            Debug.LogError("Multiple instances of CustomInput");
        }
    }

    public bool GetKeyPress (KeyCode key) {
        Debug.Assert (registeredKeys.Contains (key), "Key not registered");

        if (Input.GetKey (key)) {
            KeyState state = keys[key];
            if (Input.GetKeyDown (key)) {
                state.lastPhysicalPressTime = Time.time;
                return true;
            }

            float timeSinceLastPhysicalPress = Time.time - state.lastPhysicalPressTime;

            if (timeSinceLastPhysicalPress > delayToFirstRepeat) {
                float timeSinceLastVirtualPress = Time.time - state.lastVirtualPressTime;

                if (timeSinceLastVirtualPress > repeatSpeed) {
                    state.lastVirtualPressTime = Time.time;
                    return true;
                }

            }
        }
        return false;
    }

    public void RegisterKey (KeyCode key) {
        if (!registeredKeys.Contains (key)) {
            registeredKeys.Add (key);
            KeyState state = new KeyState (key);
            keys.Add (key, state);
        }
    }

    public class KeyState {
        public readonly KeyCode key;
        public float lastVirtualPressTime;
        public float lastPhysicalPressTime;

        public KeyState (KeyCode key) {
            this.key = key;
        }

    }
}