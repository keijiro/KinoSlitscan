using UnityEngine;
using System.Collections.Generic;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Slitscan : MonoBehaviour
    {
        #region Frame storage class

        class Frame
        {
            public RenderTexture lumaTexture;
            public RenderTexture chromaTexture;

            public void Prepare(int width, int height)
            {
                if (lumaTexture != null)
                    if (lumaTexture.width != width || lumaTexture.height != height)
                        Release();

                if (lumaTexture == null)
                {
                    lumaTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
                    lumaTexture.filterMode = FilterMode.Point;
                    lumaTexture.wrapMode = TextureWrapMode.Clamp;
                }

                if (chromaTexture == null)
                {
                    chromaTexture = RenderTexture.GetTemporary(width/2, height/2, 0, RenderTextureFormat.R8);
                    chromaTexture.filterMode = FilterMode.Point;
                    chromaTexture.wrapMode = TextureWrapMode.Clamp;
                }
            }

            public void Release()
            {
                if (lumaTexture != null)
                    RenderTexture.ReleaseTemporary(lumaTexture);

                if (chromaTexture != null)
                    RenderTexture.ReleaseTemporary(chromaTexture);

                lumaTexture = null;
                chromaTexture = null;
            }
        }

        #endregion

        #region Private properties

        [SerializeField, Range(16, 128)] int _slices = 128;

        [SerializeField] Mesh _mesh;
        [SerializeField] Shader _shader;

        Material _material;

        Frame[] _history;
        int _lastFrame = -1;

        RenderBuffer[] _mrt;

        #endregion

        #region Private functions

        void AppendFrame(RenderTexture source)
        {
            _lastFrame = (_lastFrame + 1) % _slices;

            var frame = _history[_lastFrame];
            frame.Prepare(source.width, source.height);

            Graphics.Blit(source, frame.lumaTexture, _material, 0);
            Graphics.Blit(source, frame.chromaTexture, _material, 1);
        }

        Frame GetFrameRelative(int offset)
        {
            var i = (_lastFrame + offset + _slices) % _slices;
            return _history[i];
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find("Hidden/Kino/Slitscan"));
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (_history == null)
            {
                _history = new Frame[_slices];
                for (var i = 0; i < _slices; i++)
                    _history[i] = new Frame();
            }

            if (_mrt == null)
                _mrt = new RenderBuffer[2];
        }

        void OnDisable()
        {
            foreach (var frame in _history)
                frame.Release();
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
            AppendFrame(source);

            RenderTexture.active = destination;

            var sliceWidth = 4.0f / _slices;
            _material.SetFloat("_SliceScale", sliceWidth);

            for (var i = 0; i < _slices; i += 4)
            {
                var frame0 = GetFrameRelative(i + 0);
                var frame1 = GetFrameRelative(i + 1);
                var frame2 = GetFrameRelative(i + 2);
                var frame3 = GetFrameRelative(i + 3);

                _material.SetTexture("_MainTex", source);

                _material.SetTexture("_LumaTexture0", frame0.lumaTexture);
                _material.SetTexture("_LumaTexture1", frame1.lumaTexture);
                _material.SetTexture("_LumaTexture2", frame2.lumaTexture);
                _material.SetTexture("_LumaTexture3", frame3.lumaTexture);

                _material.SetTexture("_ChromaTexture0", frame0.chromaTexture);
                _material.SetTexture("_ChromaTexture1", frame1.chromaTexture);
                _material.SetTexture("_ChromaTexture2", frame2.chromaTexture);
                _material.SetTexture("_ChromaTexture3", frame3.chromaTexture);

                _material.SetFloat("_SliceOffset", 2.0f * i / _slices + sliceWidth - 1);
                _material.SetPass(2);

                Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
            }
        }

        #endregion
    }
}
