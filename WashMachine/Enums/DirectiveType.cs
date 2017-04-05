using System;
using System.Linq;
using System.Reflection;

namespace WashMachine.Enums
{
    public enum DirectiveTypeEnum
    {
        [DirectiveMeta(12, 7)]
        TryStart,

        [DirectiveMeta(7, 7)]
        TryPause,

        [DirectiveMeta(7, 7)]
        Close,

        [DirectiveMeta(7, 9)]
        Idle,

        [DirectiveMeta(7, 13)]
        Running,

        [DirectiveMeta(7, 12)]
        Pausing
    }


    public class DirectiveMetaAttribute : Attribute
    {
        public int DirectiveLength { get; set; }
        public int FeedbackLength { get; set; }

        public DirectiveMetaAttribute(int len, int flen)
        {
            DirectiveLength = len;
            FeedbackLength = flen;
        }
    }

    public static class DirectiveTypeEnumEx
    {
        public static int GetDirectiveLength(this DirectiveTypeEnum dm)
        {
            var enumType = dm.GetType();

            var name = Enum.GetName(enumType, dm);
            if (null == name) return 0;

            var fi = enumType.GetField(name);
            if (null == fi) return 0;

            var des = (DirectiveMetaAttribute[])fi.GetCustomAttributes(typeof(DirectiveMetaAttribute), true);
            return des.FirstOrDefault()?.DirectiveLength ?? 0;
        }

        public static int GetFeedbackLength(this DirectiveTypeEnum dm)
        {
            var len = 0;
            switch (dm)
            {
                case DirectiveTypeEnum.TryStart:
                    len = 7;
                    break;
                case DirectiveTypeEnum.TryPause:
                    len = 7;
                    break;
                case DirectiveTypeEnum.Close:
                    len = 7;
                    break;
                case DirectiveTypeEnum.Idle:
                    len = 9;
                    break;
                case DirectiveTypeEnum.Running:
                    len = 13;
                    break;
                case DirectiveTypeEnum.Pausing:
                    len = 12;
                    break;
                default:break;
            }
            return len;
//            var enumType = dm.GetType();
//
//            var name = Enum.GetName(enumType, dm);
//            if (null == name) return 0;
//
//            var fi = enumType.GetField(name);
//            if (null == fi) return 0;
//
//            var des = (DirectiveMetaAttribute[])fi.GetCustomAttributes(typeof(DirectiveMetaAttribute), true);
//            return des.FirstOrDefault()?.FeedbackLength ?? 0;
        }

    }
}
