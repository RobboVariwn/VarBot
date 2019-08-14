using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Varwin.Data;

namespace Varwin.WWW
{
    [SuppressMessage("ReSharper", "SpecifyACultureInStringConversionExplicitly")]
    [SuppressMessage("ReSharper", "RedundantAssignment")]
    public class RequestDownLoad : Request
    {
        #region PRIVATE VARS

        private BaseLoader _loader;
        private readonly string _stringFormat;
        private bool _isDownloading;
        private string _fileName;
        private readonly string _downLoadPath;
        private uint _contentLength;
        private int _n;
        private int _read;
        private int _bufer = 512;
        private NetworkStream _networkStream;
        private FileStream _fileStream;
        private Socket _client;
        private readonly List<string> _uris;
        public ResponseDownLoad Response;

        private readonly LoaderAdapter.ProgressUpdate _onLoadingUpdate;

        #endregion

        /// <summary>
        /// Download large file request
        /// </summary>
        /// <param name="uris"></param>
        /// <param name="downLoadPath">Local download path</param>
        /// <param name="onLoadingUpdate"></param>
        /// <param name="loader">Text, to show progress</param>
        /// <param name="stringFormat">String format for progress {0}:File name {1}:Downloaded bytes {2}: File length</param>
        public RequestDownLoad(List<string> uris, string downLoadPath, LoaderAdapter.ProgressUpdate onLoadingUpdate,
            BaseLoader loader = null, string stringFormat = null)
        {
            Uri = uris.ToString();
            _uris = uris;
            _loader = loader;
            _stringFormat = stringFormat;
            _downLoadPath = downLoadPath;
            _onLoadingUpdate = onLoadingUpdate;
            OnUpdate += Update;

            Response = new ResponseDownLoad
            {
                LocalFilesPathes = new List<string>(), DownLoadedData = new Dictionary<string, List<byte>>()
            };
            RequestManager.AddRequest(this);
        }

        public RequestDownLoad(string uri, string downLoadPath, BaseLoader loader = null,
            string stringFormat = null)
        {
            Uri = uri;
            _uris = new List<string> {uri};
            _loader = loader;
            _stringFormat = stringFormat;
            _downLoadPath = downLoadPath;
            OnUpdate += Update;

            Response = new ResponseDownLoad
            {
                LocalFilesPathes = new List<string>(), DownLoadedData = new Dictionary<string, List<byte>>()
            };
            RequestManager.AddRequest(this);
        }

        protected override IEnumerator SendRequest()
        {
            foreach (string uri in _uris)
            {
                _fileName = Path.GetFileName(uri);
                StartDownload(Settings.Instance().ApiHost + uri);

                while (_isDownloading)
                {
                    yield return false;
                }
            }

            ((IRequest) this).OnResponseDone(Response);

            yield return true;
        }

        #region DOWNLOAD METHODS

        private void Update()
        {
            if (!_isDownloading)
            {
                return;
            }

            byte[] buffer = new byte[_bufer * 1024];

            if (_n < _contentLength)
            {
                if (!Response.DownLoadedData.ContainsKey(_fileName))
                {
                    Response.DownLoadedData.Add(_fileName, new List<byte>());
                }

                if (_networkStream.DataAvailable)
                {
                    _read = _networkStream.Read(buffer, 0, buffer.Length);
                    _n += _read;
                    _fileStream.Write(buffer, 0, _read);

                    for (int i = 0; i < _read; i++)
                    {
                        Response.DownLoadedData[_fileName].Add(buffer[i]);
                    }
                }

                //if (NLoggerSettings.LogDebug) Debug.Log("Downloaded: " + _n + " of " + _contentLength + " bytes ...");

                string sn = Math.Round((decimal) _n / 1048576, 0).ToString();
                string slenghth = Math.Round((decimal) _contentLength / 1048576, 1).ToString();

                if (_loader != null)
                {
                    _loader.FeedBackText = string.Format(_stringFormat,
                        _fileName,
                        sn,
                        slenghth);
                }

                _onLoadingUpdate?.Invoke(_n / (float) _contentLength);
            }
            else
            {
                _isDownloading = false;

                if (_loader != null)
                {
                    _loader.FeedBackText = "Download complete!";
                }

                LogManager.GetCurrentClassLogger().Debug("Файл" + _fileName + "загружен");

                _fileStream.Flush();
                _fileStream.Close();
                _client.Close();
                Response.LocalFilesPathes.Add(Path.Combine(_downLoadPath, _fileName));
            }
        }


        public void StartDownload(string uri)
        {
            if (_isDownloading)
            {
                return;
            }

            _isDownloading = true;

            _contentLength = 0;
            _n = 0;
            _read = 0;

            Uri myUri = new Uri(uri);
            string host = myUri.Host;
            int port = myUri.Port;

            string query = "GET " +
                           uri.Replace(" ", "%20") +
                           " HTTP/1.1\r\n" +
                           "Host: " +
                           host +
                           "\r\n" +
                           "Port: " +
                           port +
                           "\r\n" +
                           "User-Agent: undefined\r\n" +
                           "Connection: close\r\n" +
                           "\r\n";

            Debug.Log(query);

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            _client.Connect(host, port);

            _networkStream = new NetworkStream(_client);

            var bytes = Encoding.Default.GetBytes(query);
            _networkStream.Write(bytes, 0, bytes.Length);

            var bReader = new BinaryReader(_networkStream, Encoding.Default);

            string response = "";
            string line;
            char c;

            do
            {
                line = "";
                c = '\u0000';

                while (true)
                {
                    c = bReader.ReadChar();

                    if (c == '\r')
                    {
                        break;
                    }

                    line += c;
                }

                c = bReader.ReadChar();
                response += line + "\r\n";
            } while (line.Length > 0);

            Debug.Log(response);

            Regex reContentLength = new Regex(@"(?<=Content-Length:\s)\d+", RegexOptions.IgnoreCase);
            _contentLength = uint.Parse(reContentLength.Match(response).Value);
            string path = Path.Combine(_downLoadPath, _fileName);
            _fileStream = new FileStream(path, FileMode.Create);
        }
    }

    #endregion
}