using System;
using System.IO;
using System.Net;
class RouteProbe
{
    static int Main(string[] args)
    {
        try
        {
            ServicePointManager.SecurityProtocol=SecurityProtocolType.Tls12;
            var request=(HttpWebRequest)WebRequest.Create("https://api.ipify.org/");
            request.Proxy=null;request.Timeout=12000;request.ReadWriteTimeout=12000;request.AllowAutoRedirect=false;
            using(var response=(HttpWebResponse)request.GetResponse())using(var reader=new StreamReader(response.GetResponseStream()))
            {IPAddress ip;if(response.StatusCode!=HttpStatusCode.OK||!IPAddress.TryParse(reader.ReadToEnd().Trim(),out ip))return 2;File.WriteAllText(args[0],ip.ToString());}
            return 0;
        }
        catch {return 1;}
    }
}
