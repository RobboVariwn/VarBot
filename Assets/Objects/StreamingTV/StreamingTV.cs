using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using Varwin.Public;

namespace Varwin.Types.StreamingTV_fde2ad18ba314409b14c718caa6375b9
{
    [Locale(SystemLanguage.English,"Streaming TV")]
    [Locale(SystemLanguage.Russian,"Потоковое ТВ")]
    public class StreamingTV : VarwinObject
    {
        private Texture2D texture;
        private Stream stream;
        private IEnumerator coroutine;
        private WebResponse resp;

        public Renderer Screen;

        void Start()
        {
            Screen.material = Instantiate<Material>(Screen.material);
        }
        
        [Getter("get_wrapper")]
        [Locale(SystemLanguage.English, "Get screen for sharing")]
        [Locale(SystemLanguage.Russian, "Получить экран для шеринга")]
        public Wrapper screen
        {
            get
            {
                return this.Wrapper();
            }
        }

        [Action("share_screen")]
        [Locale(SystemLanguage.English, "Share screen")]
        [Locale(SystemLanguage.Russian, "Поделиться экраном")]
        public void ShareScreen(Wrapper anotherScreen)
        {
            anotherScreen.GetGameObject().GetComponent<StreamingTV>().Screen.material = this.Screen.material;
        }

        [Action("stop_stream")]
        [Locale(SystemLanguage.English, "Stop streaming")]
        [Locale(SystemLanguage.Russian, "Остановить стрим")]
        public void StopStream()
        {
            if (stream != null)
                stream.Close();
            if (resp != null)
                resp.Close();
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        [Action("start_stream")]
        [Locale(SystemLanguage.English, "Start streaming")]
        [Locale(SystemLanguage.Russian, "Запустить стрим")]
        public void StartStream(string url)
        {
            StartCoroutine(StartStream_Coroutine(url));
        }

        private IEnumerator StartStream_Coroutine(string url)
        {
            texture = new Texture2D(2, 2);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            resp = req.GetResponse();
            stream = resp.GetResponseStream();
            coroutine = GetFrame();

            yield return coroutine;
        }

        private IEnumerator GetFrame()
        {
            Byte[] JpegData = new Byte[65536];

            while (true)
            {
                int bytesToRead = FindLength(stream);
                if (bytesToRead == -1)
                {
                    yield break;
                }

                int leftToRead = bytesToRead;

                while (leftToRead > 0)
                {
                    leftToRead -= stream.Read(JpegData, bytesToRead - leftToRead, leftToRead);
                    yield return null;
                }

                MemoryStream ms = new MemoryStream(JpegData, 0, bytesToRead, false, true);

                texture.LoadImage(ms.GetBuffer());

                Screen.material.mainTexture = texture;

                stream.ReadByte();
                stream.ReadByte();
            }
        }

        private int FindLength(Stream stream)
        {
            int b;
            string line = "";
            int result = -1;
            bool atEOL = false;

            while ((b = stream.ReadByte()) != -1)
            {
                if (b == 10) continue;
                if (b == 13)
                {
                    if (atEOL)
                    { 
                        stream.ReadByte();
                        return result;
                    }
                    if (line.StartsWith("Content-Length:"))
                    {
                        result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                    }
                    else
                    {
                        line = "";
                    }
                    atEOL = true;
                }
                else
                {
                    atEOL = false;
                    line += (char)b;
                }
            }
            return -1;
        }
    }
}
