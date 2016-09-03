using UnityEngine;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public partial class Slitscan : MonoBehaviour
    {
        #region Editable properties

        [SerializeField, Range(16, 128)] int _slices = 128;

        #endregion

        #region Private members

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

            Graphics.Blit(source, frame.yTexture, _material, 0);

            _mrt[0] = frame.cgTexture.colorBuffer;
            _mrt[1] = frame.coTexture.colorBuffer;
            Graphics.SetRenderTarget(_mrt, frame.cgTexture.depthBuffer);
            Graphics.Blit(source, _material, 1);
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

                _material.SetTexture("_YTexture0", frame0.yTexture);
                _material.SetTexture("_YTexture1", frame1.yTexture);
                _material.SetTexture("_YTexture2", frame2.yTexture);
                _material.SetTexture("_YTexture3", frame3.yTexture);

                _material.SetTexture("_CgTexture0", frame0.cgTexture);
                _material.SetTexture("_CgTexture1", frame1.cgTexture);
                _material.SetTexture("_CgTexture2", frame2.cgTexture);
                _material.SetTexture("_CgTexture3", frame3.cgTexture);

                _material.SetTexture("_CoTexture0", frame0.coTexture);
                _material.SetTexture("_CoTexture1", frame1.coTexture);
                _material.SetTexture("_CoTexture2", frame2.coTexture);
                _material.SetTexture("_CoTexture3", frame3.coTexture);

                _material.SetFloat("_SliceOffset", 2.0f * i / _slices + sliceWidth - 1);
                _material.SetPass(2);

                Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
            }
        }

        #endregion
    }
}
