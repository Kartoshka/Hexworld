using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class ThreadsafeCurve : ISerializationCallbackReceiver
{
    [SerializeField]
    private AnimationCurve _curve;
    private Dictionary<int, float> _precalculatedValues;

    //Returns the Y value of the curve at X = time. Time must be between 0 - 1.
    public float Evaluate(float time)
    {
        time = Mathf.Clamp01(time);
        return _precalculatedValues[Mathf.RoundToInt(time * 100)];
    }

    //Assign new animation curve
    public void SetCurve(AnimationCurve curve)
    {
        _curve = curve;
        RefreshValues();
    }

    //Refresh internal cache
    public void RefreshValues()
    {
        _precalculatedValues = new Dictionary<int, float>();

        if (_curve == null)
            return;

        for (int i = 0; i <= 100; i++)
            _precalculatedValues.Add(i, _curve.Evaluate(i / 100f));
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        RefreshValues();
    }
}