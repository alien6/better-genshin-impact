using BetterGenshinImpact.Helpers;
using Microsoft.ClearScript;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace BetterGenshinImpact.GameTask.AutoFishing
{
    public class AutoFishingTaskParam
    {
        public AutoFishingTaskParam(int wholeProcessTimeoutSeconds, int throwRodTimeOutTimeoutSeconds, FishingTimePolicy fishingTimePolicy, bool saveScreenshotOnKeyTick, CultureInfo? cultureInfo, IStringLocalizer<AutoFishingTask>? stringLocalizer)
        {
            WholeProcessTimeoutSeconds = wholeProcessTimeoutSeconds;
            ThrowRodTimeOutTimeoutSeconds = throwRodTimeOutTimeoutSeconds;
            FishingTimePolicy = fishingTimePolicy;
            SaveScreenshotOnKeyTick = saveScreenshotOnKeyTick;
            GameCultureInfo = cultureInfo ?? new CultureInfo(TaskContext.Instance().Config.OtherConfig.GameCultureInfoName);
        }


        public int WholeProcessTimeoutSeconds { get; set; }
        public int ThrowRodTimeOutTimeoutSeconds { get; set; }
        public FishingTimePolicy FishingTimePolicy { get; set; }
        public bool SaveScreenshotOnKeyTick { get; set; }
        public CultureInfo GameCultureInfo { get; }

        /// <summary>
        /// 从JS请求参数构建任务参数
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static AutoFishingTaskParam BuildFromSoloTaskConfig(object? config)
        {
            if (config == null)
            {
                return BuildFromConfig(TaskContext.Instance().Config.AutoFishingConfig);
            }

            var autoFishingConfig = TaskContext.Instance().Config.AutoFishingConfig;

            var jsObject = (ScriptObject)config;
            var wholeProcessTimeoutSeconds = ScriptObjectConverter.GetValue(jsObject, "wholeProcessTimeoutSeconds", autoFishingConfig.WholeProcessTimeoutSeconds);
            var throwRodTimeOutTimeoutSeconds = ScriptObjectConverter.GetValue(jsObject, "throwRodTimeOutTimeoutSeconds", autoFishingConfig.AutoThrowRodTimeOut);
            var fishingTimePolicy = (FishingTimePolicy)ScriptObjectConverter.GetValue(jsObject, "fishingTimePolicy", (int)autoFishingConfig.FishingTimePolicy);
            var saveScreenshotOnKeyTick = ScriptObjectConverter.GetValue(jsObject, "saveScreenshotOnKeyTick", false);

            return new AutoFishingTaskParam(wholeProcessTimeoutSeconds, throwRodTimeOutTimeoutSeconds, fishingTimePolicy, saveScreenshotOnKeyTick, null, null);
        }

        /// <summary>
        /// 从配置文件构建任务参数
        /// </summary>
        /// <param name="config"></param>
        /// <param name="saveScreenshotOnKeyTick"></param>
        /// <returns></returns>
        public static AutoFishingTaskParam BuildFromConfig(AutoFishingConfig config, bool saveScreenshotOnKeyTick = false)
        {
            CultureInfo cultureInfo = new CultureInfo(TaskContext.Instance().Config.OtherConfig.GameCultureInfoName);
            return new AutoFishingTaskParam(config.WholeProcessTimeoutSeconds, config.AutoThrowRodTimeOut, config.FishingTimePolicy, saveScreenshotOnKeyTick, cultureInfo, null);
        }
    }
}
