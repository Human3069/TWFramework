using System.Collections.Generic;
using UnityEngine;

namespace _KMH_Framework
{
    [System.Serializable]
    public class KeySetting
    {
        [SerializeField]
        internal string name;
        [SerializeField]
        internal KeyCode keyCode;

        [Space(10)]
        [ReadOnly]
        [SerializeField]
        internal bool _isInput;
        internal bool isInput
        {
            get
            {
                return _isInput;
            }
            set
            {
                if (_isInput != value)
                {
                    _isInput = value;

                    Invoke_OnValueChanged(value);
                }
            }
        }

        public delegate void ValueChanged(bool _value);
        public event ValueChanged OnValueChanged;

        protected internal void Invoke_OnValueChanged(bool _value)
        {
            if (OnValueChanged != null)
            {
                OnValueChanged(_value);
            }
        }
    }

    public class KeyInputManager : MonoBehaviour
    {
        private const string LOG_FORMAT = "<color=white><b>[KeyInputManager]</b></color> {0}";

        protected static KeyInputManager _instance;
        public static KeyInputManager Instance
        {
            get
            {
                return _instance;
            }
            protected set
            {
                _instance = value;
            }
        }

        public KeySetting[] KeySettings;

        public enum SettingState
        {
            None,
            WaitForInput
        }

        [ReadOnly]
        [SerializeField]
        protected SettingState _settingState = SettingState.None;
        public SettingState _SettingState
        {
            get
            {
                return _settingState;
            }
            set
            {
                _settingState = value;

                Invoke_OnSettingStateChanged();
            }
        }

        public delegate void SettingStateChanged(SettingState _value, KeyCode _keyCode, int index);
        public event SettingStateChanged OnSettingStateChanged;

        protected virtual void Invoke_OnSettingStateChanged()
        {
            if (OnSettingStateChanged != null)
            {
                OnSettingStateChanged(_SettingState, keyCodeParam, ui_Id);
            }
        }

        protected int ui_Id = -1;
        protected KeyCode keyCodeParam = KeyCode.None;

        public Dictionary<string, KeySetting> KeyData = new Dictionary<string, KeySetting>();

        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarningFormat(LOG_FORMAT, "<color=white><b>Instance Overlapped</b></color> While On Awake()");
                Destroy(this);
                return;
            }

            for (int i = 0; i < KeySettings.Length; i++)
            {
                KeyData.Add(KeySettings[i].name, KeySettings[i]);
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
        }

        protected virtual void Update()
        {
            for (int i = 0; i < KeySettings.Length; i++)
            {
                if (_SettingState == SettingState.None)
                {
                    if (Input.GetKey(KeySettings[i].keyCode) == true)
                    {
                        KeySettings[i].isInput = true;
                    }
                    else
                    {
                        KeySettings[i].isInput = false;
                    }
                }
            }
        }

        protected virtual void OnGUI()
        {
            // this Calls Once While Get KeyCode
            if (_SettingState == SettingState.WaitForInput)
            {
                Event _event = Event.current;
                if (_event.keyCode != KeyCode.None)
                {
                    Debug.LogFormat(LOG_FORMAT, "Input Key : <color=green><b>" + _event.keyCode + "</b></color>");

                    /*
                    if (CheckKeyCodesOverlap(ui_Id) == true)
                    {
                        Debug.LogWarningFormat(LOG_FORMAT, "New KeyCode Has Overlap! ui_Id : <color=green><b>" + ui_Id + "</b></color>");
                    }
                    else
                    {
                        keyCodeParam = _event.keyCode;
                        _SettingState = SettingState.None;
                    }
                    */

                    keyCodeParam = _event.keyCode;
                    _SettingState = SettingState.None;
                }
            }
        }


        public virtual void OnClickEnteredKeyButton(int id)
        {
            Debug.LogFormat(LOG_FORMAT, "OnClick<color=white><b>EnteredKey</b></color>Button(), id : " + id);

            if (_SettingState == SettingState.None)
            {
                ui_Id = id;
                _SettingState = SettingState.WaitForInput;

                // Control By OnGUI
            }
            else
            {
                //
            }
        }

        /*
        protected virtual bool CheckKeyCodesOverlap(int id)
        {
            Debug.LogFormat(LOG_FORMAT, "Check<color=white><b>KeyCodes</b></color>Overlap(), id : <color=green><b>" + id + "</b></color>");

            for(int i = 0; i < KeySettings.Length; i++)
            {
                if (i == id)
                {
                    continue;
                }
                else
                {
                    if (KeySettings[i].keyCode == KeySettings[id].keyCode)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        */
    }
}