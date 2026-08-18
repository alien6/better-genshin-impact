using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Localization;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.Core.Simulator;
using BetterGenshinImpact.GameTask.AutoSkip;
using BetterGenshinImpact.GameTask.Common.BgiVision;
using BetterGenshinImpact.GameTask.Model.Area;
using OpenCvSharp;
using Vanara.PInvoke;
using static BetterGenshinImpact.GameTask.Common.TaskControl;
using System.Text.RegularExpressions;
using BetterGenshinImpact.Core.Config;
using Microsoft.Extensions.Logging;
using BetterGenshinImpact.Core.Recognition.OpenCv;

namespace BetterGenshinImpact.GameTask.Common.Job;

public partial class ChooseTalkOptionTask
{
    private readonly ILogger<ChooseTalkOptionTask> _logger = App.GetLogger<ChooseTalkOptionTask>();

    private static RecognitionObject GetOptionIconRecognitionObject(ImageRegion region)
    {
        return RecognitionAssets.Get("AutoSkip", "OptionIcon", region.Width, region.Height);
    }

    public string Name => "持续对话并选择目标选项";

    /// <summary>
    /// 单个界面单个选项选择
    /// </summary>
    public async Task<TalkOptionRes> SingleSelectText(string option, CancellationToken ct, int skipTimes = 10, bool isOrange = false)
    {
        var acceptedOptions = LegacyScriptTextResolver.GetAll(option, GetConfiguredGameCulture());

        if (!await Bv.WaitAndSkipForTalkUi(ct, 10))
        {
            Logger.LogError("选项选择：{Text}", "当前界面不在对话选项界面");
            return TalkOptionRes.NotFound;
        }

        await Task.Delay(500, ct);

        bool firstOcrOption = true;
        for (var i = 0; i < skipTimes; i++)
        {
            using var region = CaptureToRectArea();
            var optionRegions = RecognizeOption(region, ct);
            if (optionRegions == null)
            {
                TaskContext.Instance().PostMessageSimulator.KeyPressBackground(User32.VK.VK_SPACE);
                await Delay(500, ct);
                continue;
            }
            else
            {
                if (firstOcrOption)
                {
                    await Delay(1000, ct);
                    firstOcrOption = false;
                    continue;
                }
            }

            foreach (var optionRa in optionRegions)
            {
                if (string.IsNullOrWhiteSpace(optionRa.Text))
                {
                    continue;
                }

                if (acceptedOptions.Any(expected =>
                        optionRa.Text.Contains(expected, StringComparison.OrdinalIgnoreCase)))
                {
                    if (isOrange)
                    {
                        if (!IsOrangeOption(region.DeriveCrop(optionRa.ToRect()).SrcMat))
                        {
                            return TalkOptionRes.FoundButNotOrange;
                        }
                    }

                    ClickOcrRegion(optionRa);
                    await Task.Delay(300, ct);
                    return TalkOptionRes.FoundAndClick;
                }
            }
        }

        return TalkOptionRes.NotFound;
    }

    public async Task SelectLastOptionOnce(CancellationToken ct)
    {
        using var region = CaptureToRectArea();
        if (Bv.IsInTalkUi(region))
        {
            var chatOptionResultList = region.FindMulti(GetOptionIconRecognitionObject(region));
            chatOptionResultList = [.. chatOptionResultList.OrderByDescending(r => r.Y)];
            if (chatOptionResultList.Count > 0)
            {
                ClickOcrRegion(chatOptionResultList[0]);
                await Task.Delay(200, ct);
            }
        }
    }

    public async Task SelectLastOptionUntilEnd(CancellationToken ct, Func<ImageRegion, bool>? endAction = null, int retry = 2400)
    {
        for (var i = 0; i < retry; i++)
        {
            using var region = CaptureToRectArea();
            if (Bv.IsInTalkUi(region))
            {
                var chatOptionResultList = region.FindMulti(GetOptionIconRecognitionObject(region));
                chatOptionResultList = [.. chatOptionResultList.OrderByDescending(r => r.Y)];
                if (chatOptionResultList.Count > 0)
                {
                    ClickOcrRegion(chatOptionResultList[0]);
                }
                else
                {
                    TaskContext.Instance().PostMessageSimulator.KeyPressBackground(User32.VK.VK_SPACE);
                }
            }
            else if (Bv.IsInMainUi(region))
            {
                break;
            }
            else if (endAction != null && endAction(region))
            {
                break;
            }
            await Task.Delay(200, ct);
        }
    }

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex EnOrNumRegex();

    /// <summary>
    /// 识别当前对话界面的所有选项
    /// </summary>
    public List<Region>? RecognizeOption(ImageRegion region, CancellationToken ct)
    {
        var assetScale = TaskContext.Instance().SystemInfo.AssetScale;

        var chatOptionResultList = region.FindMulti(GetOptionIconRecognitionObject(region));
        if (chatOptionResultList.Count > 0)
        {
            chatOptionResultList = [.. chatOptionResultList.OrderByDescending(r => r.Y)];

            var lowest = chatOptionResultList[0];
            var ocrRect = new Rect((int)(lowest.X + lowest.Width + 8 * assetScale), region.Height / 8,
                (int)(535 * assetScale), (int)(lowest.Y + lowest.Height + 30 * assetScale - region.Height / 12d));
            var ocrResList = region.FindMulti(RecognitionObject.Ocr(ocrRect));

            var rs = new List<Region>();
            ocrResList = [.. ocrResList.OrderBy(r => r.Y)];
            for (var i = 0; i < ocrResList.Count; i++)
            {
                var item = ocrResList[i];
                if (string.IsNullOrEmpty(item.Text) || (item.Text.Length < 5 && EnOrNumRegex().IsMatch(item.Text)))
                {
                    continue;
                }

                if (i != ocrResList.Count - 1)
                {
                    if (ocrResList[i + 1].Y - ocrResList[i].Y > 150)
                    {
                        Debug.WriteLine($"存在Y轴偏差过大的结果，忽略:{item.Text}");
                        continue;
                    }
                }

                rs.Add(item);
            }

            // Preserve historical behavior: the existing implementation returns
            // the OCR result list rather than the filtered helper list above.
            return ocrResList;
        }

        return null;
    }

    private static CultureInfo GetConfiguredGameCulture()
    {
        var cultureName = TaskContext.Instance().Config.OtherConfig.GameCultureInfoName;
        if (string.IsNullOrWhiteSpace(cultureName))
        {
            return CultureInfo.GetCultureInfo("zh-Hans");
        }

        try
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
        catch (CultureNotFoundException)
        {
            return CultureInfo.GetCultureInfo("zh-Hans");
        }
    }

    private void ClickOcrRegion(Region region)
    {
        region.Click();
        AutoSkipLog(region.Text);
    }

    private void AutoSkipLog(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _logger.LogInformation("对话选项：{Text}", text);
        }
    }

    private bool IsOrangeOption(Mat textMat)
    {
        Scalar lowerOrange = new Scalar(10, 150, 150);
        Scalar upperOrange = new Scalar(25, 255, 255);
        var mask = OpenCvCommonHelper.InRangeHsv(textMat, lowerOrange, upperOrange);
        int highConfidencePixels = Cv2.CountNonZero(mask);
        double rate = highConfidencePixels * 1.0 / (mask.Width * mask.Height);
        Debug.WriteLine($"识别到橙色文字区域占比:{rate}");
        _logger.LogInformation($"识别到橙色文字区域占比:{rate}");
        return rate > 0.1;
    }
}

public enum TalkOptionRes
{
    NotFound,
    FoundButNotOrange,
    FoundAndClick,
}
