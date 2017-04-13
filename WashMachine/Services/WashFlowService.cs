using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WashMachine.Libs;
using WashMachine.Models;

namespace WashMachine.Services
{
    public class WashFlowService
    {
        public static readonly WashFlowService Instance = new WashFlowService();

       
        public bool Save(WashFlow washFlow, out string err)
        {
            err = "";
            using (var db = new MyDbContext())
            {
                var p = db.WashFlows.Count();
               
                if (washFlow.Id > 0)
                {
                    db.WashFlows.Update(washFlow);
                    db.SaveChanges();
                    return true;
                }

                if (p >= 20)
                {
                    err = "最多只能保存20条记录，请删除不常用记录后在保存";
                    return false;
                }

                if (db.WashFlows.Count(x => x.Name == washFlow.Name) > 0)
                {
                    err = "名称已存在，请换个名称";
                    return false;
                }

                db.WashFlows.Add(washFlow);
                db.SaveChanges();
                return true;
            }
        }

        public bool Remove(WashFlow flow)
        {
            using (var db = new MyDbContext())
            {
                db.WashFlows.Remove(flow);
                return true;
            }
        }

        public IList<WashFlow> GetList()
        {
            using (var db = new MyDbContext())
            {
                return db.WashFlows.ToList();
            }
        } 
    }
}
