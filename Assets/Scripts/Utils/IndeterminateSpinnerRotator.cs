using System;
using UnityEngine;

namespace Utils
{

    public class IndeterminateSpinnerRotator {

        #region data
        Transform Transform { get; set; }
        Transform[] Transforms { get; set; }
        float Steps { get; set; }
        DateTime? StartTime { get; set; }
        #endregion data

        #region logic
        public IndeterminateSpinnerRotator(Transform transform, float steps) {
            Transform = transform;
            Steps = steps;
            StartTime = null;
        }

        public IndeterminateSpinnerRotator(Transform[] transforms, float steps) {
            Transforms = transforms;
            Steps = steps;
            StartTime = null;
        }

        public void UpdateIndeterminateSpinner() {
            if (!StartTime.HasValue) {
                StartTime = DateTime.UtcNow;
            }

            var animationTime = (float)DateTime.UtcNow.Subtract(StartTime.Value).TotalSeconds;
            var currentStep = animationTime * Steps;
            var stepAmount = 360.0f / Steps;

            var roll = Mathf.Round(currentStep) * stepAmount;
            Transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -roll);
        }

        public void UpdateIndeterminateSpinners() {
            if (!StartTime.HasValue) {
                StartTime = DateTime.UtcNow;
            }

            var animationTime = (float)DateTime.UtcNow.Subtract(StartTime.Value).TotalSeconds;
            var currentStep = animationTime * Steps;
            var stepAmount = 360.0f / Steps;

            var roll = Mathf.Round(currentStep) * stepAmount;

            foreach (var transform in Transforms) {
                transform.localRotation = Quaternion.Euler(0.0f, 0.0f, -roll);
            }
        }

        public void Cancel() {
            StartTime = null;
        }

        #endregion logic
    }
}
