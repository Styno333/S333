using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace S333
{
    public class FloatDataDrawer : MonoBehaviour
    {
        [SerializeField] private FloatDataSO _data;
        [SerializeField] private TMPro.TMP_Text _visual;

        private void OnValueChangedHandler(float val)
        {
            _visual.text = val.ToString();
        }

        private void OnEnable()
        {
            if (_data == null) return;

            OnValueChangedHandler(_data.RuntimeValue);
            _data.OnValueChanged += OnValueChangedHandler;
        }

        private void OnDisable()
        {
            if (_data == null) return;


            _data.OnValueChanged -= OnValueChangedHandler;
        }
    }
}

