using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BililiveRecorder.UWP
{
    internal class ValueConverterGroup : List<IValueConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, language));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    internal class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value.Equals(true) ? parameter : null;
        }
    }

    internal class BooleanInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(bool)value;
        }
    }

    internal class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is bool v)
            {
                return v ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public static class Converter
    {
        public static string ConvertStateToString(bool isMonitoring, bool isRecording)
        {
            int i = (isMonitoring ? 1 : 0) + (isRecording ? 2 : 0);
            switch (i)
            {
                case 0:
                    return "闲置中";
                case 1:
                    return "监控中";
                case 2:
                case 3:
                    return "录制中";
                default:
                    return string.Empty;
            }
        }

        public static string FormatSpeed(double speed)
        {
            return string.Format("0.## KiB/s", speed);
        }

        public static string FormatSpeedRate(double rate)
        {
            return string.Format("0.## %", rate);
        }

        public static bool ConvertEnumToBool(Enum @enum,Enum @enum2)
        {
            return @enum.Equals(@enum2);
        }

        public static Enum ConvertBoolToEnum(bool? b,Enum @enum)
        {
            return b is true ? @enum : null;
        }

        public static bool ConvertNotEnumToBool(Enum @enum, Enum @enum2)
        {
            return !@enum.Equals(@enum2);
        }
    }
}
