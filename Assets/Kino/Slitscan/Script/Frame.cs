using UnityEngine;

namespace Kino
{
    public partial class Slitscan
    {
        //
        // Frame storage class
        //
        // Compresses and stores a frame image. The memory footprint of the
        // image is reduced to 9 bit/pixel with using the chroma subsampling
        // technique. If the frame buffer is using 1920 x 1080 x 32 bpp,
        // the footprint is reduced from 7.91 MiB to 2.22 MiB.
        //
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
