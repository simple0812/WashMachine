using System;
using System.Linq;

namespace WashMachine.Models
{
    //先判断该信息是以ok还是error结束的 如果是ok则开始解析每一行 如果是error则重发
    public class CnetScan
    {
        public string Operator { get; set; }
        public string MCC { get; set; }
        public string MNC { get; set; }
        public string Rxlev { get; set; }
        public int Cellid { get; set; }
        public string Arfcn { get; set; }
        public int Bsic { get; set; }
        public int Lac { get; set; }

        //Operator:"CHINA MOBILE",MCC:460,MNC:00,Rxlev:68,Cellid:53A5,Arfcn:94,Lac:501B,Bsic:19
        public CnetScan Resovle(string data)
        {
            var arr = data.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            var dic = arr.Select(each => each.Split(':')).Where(p => p.Length == 2).ToDictionary(p => p[0], p => p[1]);

            this.Operator = dic.ContainsKey("Operator") ? dic["Operator"].Replace("\"", "") : "";
            this.MCC = dic.ContainsKey("MCC") ? dic["MCC"] : "";
            this.MNC = dic.ContainsKey("MNC") ? dic["MNC"] : "";
            this.Rxlev = dic.ContainsKey("Rxlev") ? dic["Rxlev"] : "";
            this.Cellid = dic.ContainsKey("Cellid") ? Convert.ToInt32(dic["Cellid"], 16) : 0;
            this.Arfcn = dic.ContainsKey("Arfcn") ? dic["Arfcn"] : "";
            this.Bsic = dic.ContainsKey("Bsic") ? Convert.ToInt32(dic["Bsic"], 16) : 0;
            this.Lac = dic.ContainsKey("Lac") ? Convert.ToInt32(dic["Lac"], 16) : 0;

            return this;
        }
        //+CENG: 0,"460,00,501b,53a5,25,79"
        public CnetScan Resovlex(string data)
        {
            var arr = data.Split("\"".ToArray(), StringSplitOptions.RemoveEmptyEntries);

            if (arr.Length < 2) return this;
            var p = arr[1].Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            if (p.Length != 6) return this;
            int cid = Convert.ToInt32(p[3], 16);// int.TryParse(p[1], out cid) ? cid : -1;
            int lac = Convert.ToInt32(p[2], 16);  //int.TryParse(p[2], out lac) ? lac : -1;

            this.MCC = p[0];
            this.MNC = p[1];
            this.Cellid = cid;
            this.Lac = lac;

            return this;
        }
    }

    //获取sim卡信息
    /*
     1.发送 AT+CNETSCAN=1 设置
     2.发送 AT+CNETSCAN 获取信息，如果10分钟内没返回则再次发送
     */
}
