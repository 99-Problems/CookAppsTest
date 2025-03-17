using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessControl : MonoBehaviour
{
    public Vignette _vignette;
    public Volume _volume;
    public Camera _cam;
    public float _value = 2f;


    private void Start()
    {
        _volume.profile.TryGet(out _vignette);
    }


    [Button]
    public void VignetteControl(float _value)
    {
        if(_vignette)
            _vignette.intensity.value = _value;

        Debug.Log(_value + $"{_vignette != null}");
    }

    public IEnumerator VignetteControl()
    {

        var curTime = 0f;
        while(curTime <= 0.2f)
        {
            curTime += Time.deltaTime;
            if(_value * curTime * 5f < 1)
            {
                VignetteControl(Mathf.Clamp01(_value * curTime*5f));
            }
            else
            {
                VignetteControl(Mathf.Clamp01(1- curTime * 5f));
            }

            yield return null;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
