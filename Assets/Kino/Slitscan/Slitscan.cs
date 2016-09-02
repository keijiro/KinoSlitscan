using UnityEngine;
using System.Collections.Generic;

namespace Kino
{
    [RequireComponent(typeof(Camera))]
    public class Slitscan : MonoBehaviour
    {
        #region Public properties and methods

        #endregion

        #region Private properties

        [SerializeField] Shader _shader;

        Material _material;

        RenderTexture _buffer0;
        RenderTexture _buffer1;
        RenderTexture _buffer2;
        RenderTexture _buffer3;

        int _frameCount;

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            var shader = Shader.Find("Hidden/Kino/Slitscan");
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;
        }

        void OnDisable()
        {
            if (_buffer0 != null) RenderTexture.ReleaseTemporary(_buffer0);
            if (_buffer1 != null) RenderTexture.ReleaseTemporary(_buffer1);
            if (_buffer2 != null) RenderTexture.ReleaseTemporary(_buffer2);
            if (_buffer3 != null) RenderTexture.ReleaseTemporary(_buffer3);

            _buffer0 = _buffer1 = _buffer2 = _buffer3 = null;
        }

        void OnDestroy()
        {
            if (Application.isPlaying)
                Destroy(_material);
            else
                DestroyImmediate(_material);

            _material = null;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_frameCount == 32)
            {
                var temp = _buffer3;
                _buffer3 = _buffer2;
                _buffer2 = _buffer1;
                _buffer1 = _buffer0;

                if (temp == null)
                    _buffer0 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default);
                else
                    _buffer0 = temp;

                _buffer0.filterMode = FilterMode.Point;
                _buffer0.wrapMode = TextureWrapMode.Clamp;

                _frameCount = 0;
            }

            var oldBuffer = _buffer0;
            _buffer0 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.Default);

            _buffer0.filterMode = FilterMode.Point;
            _buffer0.wrapMode = TextureWrapMode.Clamp;

            _material.SetTexture("_CurrentFrame", source);
            _material.SetTexture("_SourceTexture", oldBuffer);
            _material.SetFloat("_BitOffset", _frameCount);

            Graphics.Blit(null, _buffer0, _material, 0);
            RenderTexture.ReleaseTemporary(oldBuffer);

            _material.SetTexture("_Texture0", _buffer0);
            _material.SetTexture("_Texture1", _buffer1);
            _material.SetTexture("_Texture2", _buffer2);
            _material.SetTexture("_Texture3", _buffer3);

            Graphics.Blit(null, destination, _material, 1);

            _frameCount++;
        }

        #endregion
    }
}
