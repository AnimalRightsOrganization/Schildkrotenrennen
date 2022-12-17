using System.IO;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System;

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

    public static string GetMD5HashFromFile(string filePath)
    {
        try
        {
            FileStream file = new FileStream(filePath, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);   //计算指定Stream 对象的哈希值  
            file.Close();

            StringBuilder Ac = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                Ac.Append(retVal[i].ToString("x2"));
            }
            return Ac.ToString();
        }
        catch (System.Exception ex)
        {
            throw new System.Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    public static string GetFileMD5(string filepath)
    {
        FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
        int bufferSize = 1048576;
        byte[] buff = new byte[bufferSize];
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        md5.Initialize();
        long offset = 0;
        while (offset < fs.Length)
        {
            long readSize = bufferSize;
            if (offset + readSize > fs.Length)
                readSize = fs.Length - offset;
            fs.Read(buff, 0, Convert.ToInt32(readSize));
            if (offset + readSize < fs.Length)
                md5.TransformBlock(buff, 0, Convert.ToInt32(readSize), buff, 0);
            else
                md5.TransformFinalBlock(buff, 0, Convert.ToInt32(readSize));
            offset += bufferSize;
        }
        if (offset >= fs.Length)
        {
            fs.Close();
            byte[] result = md5.Hash;
            md5.Clear();
            StringBuilder sb = new StringBuilder(32);
            for (int i = 0; i < result.Length; i++)
                sb.Append(result[i].ToString("X2"));
            return sb.ToString().ToLower();
        }
        else
        {
            fs.Close();
            return null;
        }
    }
}