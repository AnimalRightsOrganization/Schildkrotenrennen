using System.IO;
using System.Net;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace Client
{
    public class UI_CheckUpdate : MonoBehaviour
    {
        internal static event System.Action OnUnZipCompletedEvent; //解压完成事件
        internal static event System.Action OnDownloadCompleteEvent; //下载完成事件

        private Slider m_progressSlider;
        private Text m_progressText;
        private List<ABInfo> downloadList;
        private int fileCount = 0;

        void Awake()
        {
            m_progressSlider = transform.Find("Slider").GetComponent<Slider>();
            m_progressText = transform.Find("Slider").Find("Text").GetComponent<Text>();
            downloadList = new List<ABInfo>();
            fileCount = 0;
        }

        void Update()
        {
            float progress = (float)fileCount / (float)downloadList.Count;
            m_progressText.text = $"{(progress * 100).ToString("F0")}%";
            m_progressSlider.value = fileCount;
        }

        static IEnumerator BeginDownLoad(string downloadfileName, string desFileName)
        {
            Debug.Log($"BeginDownLoad: {downloadfileName}\nTo: {desFileName}");
            if (downloadfileName.Contains("http") == false)
            {
                downloadfileName = $"http://{downloadfileName}";
            }
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(downloadfileName);
            request.Timeout = 5000;
            WebResponse response = request.GetResponse();
            using (FileStream fs = new FileStream(desFileName, FileMode.Create))
            using (Stream netStream = response.GetResponseStream())
            {
                int packLength = 1024 * 20;
                long countLength = response.ContentLength;
                byte[] nbytes = new byte[packLength];
                int nReadSize = 0;
                nReadSize = netStream.Read(nbytes, 0, packLength);
                while (nReadSize > 0)
                {
                    fs.Write(nbytes, 0, nReadSize);
                    nReadSize = netStream.Read(nbytes, 0, packLength);

                    double dDownloadedLength = fs.Length * 1.0 / (1024 * 1024);
                    double dTotalLength = countLength * 1.0 / (1024 * 1024);
                    string ss = string.Format("已下载 {0:F3}M / {1:F3}M", dDownloadedLength, dTotalLength);
                    //Debug.Log(ss);
                    yield return false;
                }
                netStream.Close();
                fs.Close();
                if (OnDownloadCompleteEvent != null)
                {
                    Debug.Log("download  finished");
                    OnDownloadCompleteEvent.Invoke();
                }
            }
        }

        public IEnumerator StartCheck(System.Action onComplete)
        {
            string cloudPath = Path.Combine(ConstValue.AB_WebURL, "assets.bytes");
            string localPath = Path.Combine(ConstValue.AB_AppPath, "assets.bytes");

            // 1. 下载远程的(assets.bytes)
            ABInfo[] cloudInfos = new ABInfo[] { };
            List<string> cloudList = new List<string>();
            WWW www = new WWW(cloudPath);
            while (!www.isDone) { }
            yield return www;
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(cloudPath);
                Debug.LogError(www.error);
                yield break;
            }
            if (www.isDone)
            {
                var r_assets_bytes = JsonConvert.DeserializeObject<AssetsBytes>(www.text);
                cloudInfos = r_assets_bytes.ABInfoList;
                www.Dispose();
                for (int i = 0; i < cloudInfos.Length; i++)
                {
                    cloudList.Add(cloudInfos[i].md5);
                }
            }
            Debug.Log("远程：" + cloudList.Count);


            // 2. 读取本地(assets.bytes) //不需要改成逐一分析本地文件MD5
            List<string> localList = new List<string>();
            for (int i = 0; i < cloudList.Count; i++)
            {
                string _localPath = Path.Combine(ConstValue.AB_AppPath, cloudList[i] + ".unity3d");
                bool _exist = File.Exists(_localPath);
                string _md5 = string.Empty;
                if (_exist)
                {
                    _md5 = Md5Utils.GetFileMD5(_localPath);
                    //Debug.Log(cloudList[i] + "\n计算MD5:   " + _md5);
                }
                if (_exist && _md5 == cloudInfos[i].md5)
                {
                    localList.Add(cloudInfos[i].md5);
                }
            }
            Debug.Log("本地：" + localList.Count);


            // 3. 比较差异，创建下载列表(downloadList)
            var diff = cloudList.Except(localList).ToArray();
            downloadList = new List<ABInfo>();
            for (int i = 0; i < diff.Length; i++)
            {
                var ab = cloudInfos.Where(x => x.md5 == diff[i]).ToList()[0];
                downloadList.Add(ab);
            }
            Debug.Log("需要更新：" + downloadList.Count);
            m_progressSlider.minValue = 0;
            m_progressSlider.maxValue = downloadList.Count;

            // 4. 追条：确认文件存在 -> 对比md5 -> 下载
            fileCount = 0;
            for (int i = 0; i < diff.Length; i++)
            {
                string abUrl = Path.Combine(ConstValue.AB_WebURL, diff[i] + ".unity3d");
                string abDstPath = Path.Combine(ConstValue.AB_AppPath, diff[i] + ".unity3d");
                yield return BeginDownLoad(abUrl, abDstPath);
                fileCount++;
            }

            // 5. 下载完成后更新assets.bytes
            if (downloadList.Count > 0)
            {
                yield return BeginDownLoad(cloudPath, localPath);
                yield return new WaitForSeconds(1);
            }
            Debug.Log("<color=green>更新完成</color>");

            // 6. 显示下一级界面
            onComplete?.Invoke();
            //Destroy(gameObject);
            //gameObject.SetActive(false);
        }
    }
}