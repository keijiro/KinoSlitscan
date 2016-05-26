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

        [SerializeField]
        Shader _shader;

        Material _material;

        Queue<RenderTexture> _history;

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            var shader = Shader.Find("Hidden/Kino/Slitscan");
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;

            _history = new Queue<RenderTexture>();
        }

        void OnDisable()
        {
            DestroyImmediate(_material);
            _material = null;

            while (_history.Count > 0)
                RenderTexture.ReleaseTemporary(_history.Dequeue());
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            var rt = RenderTexture.GetTemporary(source.width, source.height);
            Graphics.Blit(source, rt);
            _history.Enqueue(rt);

            var ox = 0.0f;
            foreach (var t in _history)
            {
                _material.SetFloat("_Origin", ox);
                _material.SetFloat("_Height", 1.0f / _history.Count);
                Graphics.Blit(t, destination, _material, 0);
                ox += 1.0f / _history.Count;
            }

            while (_history.Count > 120)
                RenderTexture.ReleaseTemporary(_history.Dequeue());
        }

        #endregion
    }
}
