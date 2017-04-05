using WashMachine.Enums;

namespace WashMachine.Protocols.V485_1
{
    internal static class ResolverFactory
    {
        public static IFeedbackResolver Create(TargetDeviceTypeEnum deviceType)
        {
            IFeedbackResolver resolver = new PumpFeedbackResolver();
            switch (deviceType)
            {
                case TargetDeviceTypeEnum.Pump:
                    resolver = new PumpFeedbackResolver();
                    break;
                case TargetDeviceTypeEnum.Rocker:
                    resolver = new RockerFeedbackResolver();
                    break;
                case TargetDeviceTypeEnum.Thermometer:
                    resolver = new ThemometerFeedbackResolver();
                    break;
                case TargetDeviceTypeEnum.Gas:
                    resolver = new GasFeedbackResolver();
                    break;
                default:
                    break;
            }

            return resolver;
        }
    }
}
