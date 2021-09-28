using UnityEngine;
using UnityEngine.UI;
using Utility.References;

namespace UI
{
    [RequireComponent(typeof(Slider))]
    public class SliderFill : MonoBehaviour
    {
        public FloatReference Variable;
        public FloatReference Min;
        public FloatReference Max;

        private Slider _slider;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void Update()
        {
            _slider.value = Variable;
            _slider.minValue = Min;
            _slider.maxValue = Max;
        }
    }
}