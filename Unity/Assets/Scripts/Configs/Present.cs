public class Present
{
    public string server; //服务器连接
    public string web; //官网
    public string ab_url; //AB包地址
    public string version; //服务器资源版本
    public string notice; //公告
    public Present()
    {
        server = "moegijinka.cn"; //本地host配了域名
        web = "moegijinka.cn";
        ab_url = $"http://app.moegijinka.cn/{UnityEngine.Application.productName}/res";
    }
}