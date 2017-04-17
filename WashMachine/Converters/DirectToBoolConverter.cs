using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using WashMachine.Enums;

namespace WashMachine.Converters
{
    class DirectToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var val = System.Convert.ToInt32(value);
            return val == (int)DirectionEnum.Out;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
