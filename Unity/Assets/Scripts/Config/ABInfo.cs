using System.Collections.Generic;

[System.Serializable]
public class ABInfo
{
    public ABInfo()
    {
        filePath = string.Empty;
        md5 = string.Empty;
        depend = new string[0];
    }
    public ABInfo(string _filePath, string _md5, string[] _depend)
    {
        this.filePath = _filePath;
        this.md5 = _md5;
        this.depend = _depend;
    }
    public string filePath;
    public string md5;
    public string[] depend;
}
//assets.bytes
[System.Serializable]
public class AssetsBytes
{
    public AssetsBytes() //没有构造函数无法反序列化
    {
        this.res_version = 1;
        this.ABInfoList =  new ABInfo[0];
    }
    public AssetsBytes(int _res_version, List<ABInfo> _list)
    {
        this.res_version = _res_version;
        this.ABInfoList = _list.ToArray();
    }
    public int res_version;
    public ABInfo[] ABInfoList;
}