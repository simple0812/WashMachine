using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WashMachine.Libs;
using WashMachine.Models;

namespace WashMachine.Services
{
    public class WashRecordService
    {
        public static readonly WashRecordService Instance = new WashRecordService();


        public WashRecord GetBy(WashFlow flow, DateTime startTime, DateTime endTime)
        {
            return new WashRecord()
            {
                Name = flow.Name,
                ConcentrateSpeed = flow.ConcentrateSpeed,
                ConcentrateVolume = flow.ConcentrateVolume,
                ConcentrateTimes = flow.ConcentrateTimes,
                CollectSpeed = flow.CollectSpeed,
                CollectTimes = flow.CollectTimes,
                CollectVolume = flow.CollectVolume,
                WashSpeed = flow.WashSpeed,
                WashVolume = flow.WashVolume,
                StartTime = startTime,
                EndTime = endTime
            };
        }

        public bool Save(WashRecord record)
        {
            using (var db = new MyDbContext())
            {
                db.WashRecords.Add(record);
                db.SaveChanges();
                return true;
            }
        }

        public bool Save(WashFlow flow, DateTime startTime, DateTime endTime)
        {
            var record = GetBy(flow, startTime, endTime);
            using (var db = new MyDbContext())
            {
                db.WashRecords.Add(record);
                db.SaveChanges();
                return true;
            }
        }

        public bool Remove(WashRecord record)
        {
            using (var db = new MyDbContext())
            {
                db.WashRecords.Remove(record);
                db.SaveChanges();
                return true;
            }
        }

        public IList<WashRecord> GetList()
        {
            using (var db = new MyDbContext())
            {
                return db.WashRecords.ToList();
            }
        }
    }
}
