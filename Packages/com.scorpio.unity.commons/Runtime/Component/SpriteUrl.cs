using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using Scorpio.Unity.Commons;
public class SpriteUrl : MonoBehaviour {
    private int m_Index;                    //当前图片的设置索引
	private Texture2D m_Texture;            //图片资源
    private UnityWebRequest m_Request;      //网络请求
	public void SetUrl(string url) {
        SendMessage("DownloadTexture", url, SendMessageOptions.DontRequireReceiver);
	}
    IEnumerator DownloadTexture(string url) {
        var index = ++m_Index;
        var imageCache = EngineUtil.ImageCachePath + FileUtil.GetMD5FromString(url);
        FreeRequest();
        if (FileUtil.FileExist(imageCache)) {
            SetSprite(FileUtil.GetFileBuffer(imageCache));
        } else {
            using(m_Request = new UnityWebRequest(url)) {
                m_Request.downloadHandler = new DownloadHandlerBuffer();
                yield return m_Request;
                if (index != m_Index) {
                    yield break;
                }
                if (m_Request.isNetworkError || m_Request.isHttpError || m_Request.responseCode != 200) {
                    logger.error("SetSpriteUrl Request is error url : " + m_Request.error);
                    yield break;
                }
                var bytes = m_Request.downloadHandler.data;
                FileUtil.CreateFile(imageCache, bytes);
                SetSprite(bytes);
            }
        }
        yield break;
    }
    void FreeRequest() {
        if (m_Request != null && !m_Request.isDone) {
            m_Request.Abort();
            m_Request.Dispose();
            m_Request = null;
        }
    }
    void FreeTexture() {
        if (m_Texture != null) {
            Destroy(m_Texture);
            m_Texture = null;
        }
    }
    void OnDestroy() {
        if (Application.isPlaying) {
            FreeRequest();
            FreeTexture();
        }
    }
    void SetSprite(byte[] bytes) {
        try {
            FreeTexture();
            m_Texture = new Texture2D(1, 1, TextureFormat.RGBA4444, false);
            m_Texture.LoadImage(bytes);
            EngineUtil.SetSprite(this, Sprite.Create(m_Texture, new Rect(0, 0, m_Texture.width, m_Texture.height), Vector2.zero));
        } catch (System.Exception e) {
            logger.error("SetSpriteUrl is error : " + e.ToString());
        }
    }
}

