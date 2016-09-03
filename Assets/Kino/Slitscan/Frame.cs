using UnityEngine;

namespace Kino
{
    public partial class Slitscan
    {
        // Frame storage class
        class Frame
        {
            public RenderTexture yTexture;
            public RenderTexture cgTexture;
            public RenderTexture coTexture;

            public void Prepare(int width, int height)
            {
                if (yTexture != null)
                    if (yTexture.width != width || yTexture.height != height)
                        Release();

                if (yTexture == null)
                    yTexture = AllocateTemporaryRT(width, height);

                if (cgTexture == null)
                    cgTexture = AllocateTemporaryRT(width / 4, height / 4);

                if (coTexture == null)
                    coTexture = AllocateTemporaryRT(width / 4, height / 4);
            }

            public void Release()
            {
                if (yTexture != null)
                    RenderTexture.ReleaseTemporary(yTexture);

                if (cgTexture != null)
                    RenderTexture.ReleaseTemporary(cgTexture);

                if (coTexture != null)
                    RenderTexture.ReleaseTemporary(coTexture);

                yTexture = null;
                cgTexture = null;
                coTexture = null;
            }

            static RenderTexture AllocateTemporaryRT(int width, int height)
            {
                var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.R8);
                rt.filterMode = FilterMode.Bilinear;
                rt.wrapMode = TextureWrapMode.Clamp;
                return rt;
            }
        }
    }
}
