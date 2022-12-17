using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;

namespace HotFix
{
    public class Md5Utils
    {
        // 计算文件MD5/32位小写
        public static string getFileHash(string filePath)
        {
            try
            {
                FileStream fs = new FileStream(filePath, FileMode.Open);
                int length = (int)fs.Length;
                byte[] data = new byte[length];
                fs.Read(data, 0, length);
                fs.Close();

                //MD5 md5 = MD5.Create();
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] result = md5.ComputeHash(data);

                string fileMD5 = "";
                for (int i = 0; i < result.Length; i++)
                {
                    fileMD5 += result[i].ToString("x2"); //32位
                }
                Debug.WriteLine(fileMD5);
                return fileMD5;
            }
            catch (FileNotFoundException e)
            {
                Debug.WriteLine(e.Message);
                return string.Empty;
            }
        }

        public static string GetMD5String(string strWord)
        {
            string strRes = string.Empty;
            MD5 md5 = MD5.Create();
            byte[] fromData = System.Text.Encoding.UTF8.GetBytes(strWord);
            byte[] targetData = md5.ComputeHash(fromData);

            for (int i = 0; i < targetData.Length; i++)
            {
                strRes += targetData[i].ToString("x2");//x不补0,x2把0补齐,X为大写
            }
            return strRes;
        }
    }
}