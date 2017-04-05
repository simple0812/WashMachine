using WashMachine.Enums;

namespace WashMachine.Models
{

    public class DirectiveData
    {
        public int DeviceId { get; set; }
        public int DirectiveId { get; set; }
        public DirectiveTypeEnum DirectiveType { get; set; }
        public TargetDeviceTypeEnum DeviceType { get; set; }
        public int TimeInterval { get; set; }
        public string Hint { get; set; }

        public int DeviceStatus { get; set; }

        public override string ToString()
        {
            return
                $"DeviceId:{DeviceId},DirectiveId:{DirectiveId},DirectiveType:{DirectiveType},DeviceType:{DeviceType},TimeInterval:{TimeInterval},DeviceStatus:{DeviceStatus}";
        }
    }

    public class PumpDirectiveData : DirectiveData
    {
        public double Addition { get; set; }
        public double FlowRate { get; set; }
        public DirectionEnum Direction { get; set; }

        public override string ToString()
        {
            return
                base.ToString() + $",Addition:{Addition},FlowRate:{FlowRate},Direction:{Direction}";
        }
    }

    public class RockerDirectiveData : DirectiveData
    {
        public double Angle { get; set; }
        public double Speed { get; set; }
        public RockEnum RockMode { get; set; }

        public override string ToString()
        {
            if (DirectiveType == DirectiveTypeEnum.Running)
                return
                    $"摇床{DeviceId}---{DirectiveId}---速度：{Speed}";
            return
                base.ToString() + $",Angle:{Angle},Speed:{Speed},RockMode:{RockMode}";
        }
    }

    public class GasDirectiveData : DirectiveData
    {
        public double Flowrate { get; set; }

        public override string ToString()
        {
            if (DirectiveType == DirectiveTypeEnum.Running)
                return
                    $"气体传感器{DeviceId}---{DirectiveId}---浓度：{Flowrate}%";
            return
                base.ToString() + $",Flowrate:{Flowrate}";
        }
    }

    public class TemperatureDirectiveData : DirectiveData
    {
        public double Addition { get; set; }
        public  double Temperature { get; set; }

        public override string ToString()
        {
            if (DirectiveType == DirectiveTypeEnum.Running)
                return
                    $"温度传感器{DeviceId}---{DirectiveId}---温度1：{TimeInterval}℃,温度2：{Temperature}℃,温度3：{Addition}℃";
            return
                base.ToString() + $",Addition:{Addition},Temperature:{Temperature}";
        }
    }

    public class DirectiveResult
    {
        public bool Status { get; set; }
        public DirectiveTypeEnum SourceDirectiveType { get; set; }
        public DirectiveData Data { get; set; }
    }
}
