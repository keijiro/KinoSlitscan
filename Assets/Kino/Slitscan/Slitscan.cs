using UnityEngine;
using System.Collections.Generic;

namespace Kino
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class Slitscan : MonoBehaviour
    {
        #region Private properties

        [SerializeField, Range(16, 128)] int _slices = 128;

        [SerializeField] Mesh _mesh;
        [SerializeField] Shader _shader;

        Material _material;

        Queue<RenderTexture> _history;

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            if (_material == null)
            {
                _material = new Material(Shader.Find("Hidden/Kino/Slitscan"));
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            _history = new Queue<RenderTexture>();

            for (var i = 0; i < _slices; i++)
                _history.Enqueue(null);
        }

        void OnDisable()
        {
            while (_history.Count > 0)
                RenderTexture.ReleaseTemporary(_history.Dequeue());

            _history = null;
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
            var rt = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(source, rt);
            _history.Enqueue(rt);

            RenderTexture.active = destination;

            var textures = _history.ToArray();
            System.Array.Reverse(textures);

            var sliceWidth = 8.0f / _slices;
            _material.SetFloat("_SliceScale", sliceWidth);

            for (var i = 0; i < _slices; i += 8)
            {
                _material.SetTexture("_Texture0", textures[i + 0]);
                _material.SetTexture("_Texture1", textures[i + 1]);
                _material.SetTexture("_Texture2", textures[i + 2]);
                _material.SetTexture("_Texture3", textures[i + 3]);
                _material.SetTexture("_Texture4", textures[i + 4]);
                _material.SetTexture("_Texture5", textures[i + 5]);
                _material.SetTexture("_Texture6", textures[i + 6]);
                _material.SetTexture("_Texture7", textures[i + 7]);
                _material.SetFloat("_SliceOffset", 2.0f * i / _slices + sliceWidth - 1);
                _material.SetPass(0);
                Graphics.DrawMeshNow(_mesh, Matrix4x4.identity);
            }

            while (_history.Count > _slices)
                RenderTexture.ReleaseTemporary(_history.Dequeue());
        }

        #endregion
    }
}
