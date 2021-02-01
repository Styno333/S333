using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace S333
{
    [CreateAssetMenu(fileName = "New FloatData", menuName = "S333/FloatData")]
    public class FloatDataSO : ScriptableObject, ISerializationCallbackReceiver
    {
        public float InitialValue;

        private float _runTimeValue;
        public float RuntimeValue
        {
            get => _runTimeValue;
            set
            {
                _runTimeValue = value;
                OnValueChanged?.Invoke(_runTimeValue);
            }
        }

        public UnityAction<float> OnValueChanged;

        public void OnAfterDeserialize()
        {
            _runTimeValue = InitialValue;
        }

        public void OnBeforeSerialize()
        {
            
        }
    }
}

