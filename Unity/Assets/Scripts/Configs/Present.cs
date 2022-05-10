public class Present
{
    public string server; //服务器连接
    public string web; //官网
    public string ab_url; //AB包地址
    public string version; //服务器资源版本
    public string notice; //公告
    public Present()
    {
        server = "192.168.1.101"; //本地调试用
        //server = "moegijinka.cn";
        web = "http://localhost";
        ab_url = $"http://localhost/{UnityEngine.Application.productName}/download";
    }
}