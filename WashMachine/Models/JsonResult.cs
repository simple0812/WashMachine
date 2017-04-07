using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WashMachine.Models
{
    public class JsonResult
    {
        //{"code":"error","message":"耗材编号0123456789, 请扫描设备","result":""}
        public string code { get; set; }
        public string message { get; set; }
        public object result { get; set; }
    }
}
