using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WashMachine.Models;

namespace WashMachine.Services
{
    public class WashFlowService
    {
        public static readonly WashFlowService Instance = new WashFlowService();

        public bool Save(WashFlow washFlow, out string err)
        {
            err = "";
            using (var conn = DbHelper.GetDbConnection())
            {
                var p = conn.Table<WashFlow>().Count();
               
                if (washFlow.Id > 0)
                {
                    conn.InsertOrReplace(washFlow);
                    return true;
                }

                if (p >= 20)
                {
                    err = "最多只能保存20条记录，请删除不常用记录后在保存";
                    return false;
                }

                if (conn.Table<WashFlow>().Count(x => x.Name == washFlow.Name) > 0)
                {
                    err = "名称已存在，请换个名称";
                    return false;
                }

                conn.Insert(washFlow);
                return true;
            }
        }

        public bool Remove(WashFlow flow)
        {
            using (var conn = DbHelper.GetDbConnection())
            {
                return conn.Delete<WashFlow>(flow.Id) > 0;
            }
        }

        public IList<WashFlow> GetList()
        {
            using (var conn = DbHelper.GetDbConnection())
            {
                return conn.Table<WashFlow>().ToList();
            }
        } 
    }
}
