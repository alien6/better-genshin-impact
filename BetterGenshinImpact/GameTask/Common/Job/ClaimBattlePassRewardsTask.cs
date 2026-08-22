using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.GameTask.Common.BgiVision;
using BetterGenshinImpact.GameTask.Common.Element.Assets;
using BetterGenshinImpact.GameTask.Common.GameText;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.GameTask.Model.Area;
using BetterGenshinImpact.Helpers;
using BetterGenshinImpact.Helpers.Extensions;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;
using static BetterGenshinImpact.GameTask.Common.TaskControl;

namespace BetterGenshinImpact.GameTask.Common.Job;

/// <summary>
/// 一键领取纪行
/// </summary>
public class ClaimBattlePassRewardsTask
{
    private readonly ReturnMainUiTask _returnMainUiTask = new();

    private readonly CommonJobTextRecognizer _textRecognizer;

    private bool _manualSelectionReminderLogged;

    public ClaimBattlePassRewardsTask()
    {
        var matcher = App.GetService<IGameTextMatcher>()
                      ?? throw new InvalidOperationException("IGameTextMatcher is not registered.");
        _textRecognizer = new CommonJobTextRecognizer(matcher);
    }

    public async Task Start(CancellationToken ct)
    {
        try
        {
            await DoOnce(ct);
        }
        catch (Exception e)
        {
            Logger.LogDebug(e, "领取纪行奖励异常");
            Logger.LogError("领取纪行奖励异常: {Msg}", e.Message);
        }
    }

    public async Task DoOnce(CancellationToken ct)
    {
        await _returnMainUiTask.Start(ct);

        await Delay(200, ct);
        TaskContext.Instance().PostMessageSimulator.SimulateAction(GIActions.OpenBattlePassScreen); // F4 开纪行

        // 先领取纪行点数，避免一进入纪行就因可选奖励弹窗阻塞后续流程
        await Delay(1000, ct);
        GameCaptureRegion.GameRegion1080PPosClick(960, 45); // 点中间
        await Delay(500, ct);
        await ClaimAll(ct);

        // 最后再回到奖励页领取，若存在手动选择奖励则仅记录日志提醒
        await Delay(2500, ct); // 等待升级动画
        // 还可能存在领取到原石的情况
        using (var ra = CaptureToRectArea())
        {
            using var primogem = ra.Find(ElementRecognition.Get("Primogem", ra));
            if (primogem.IsExist())
            {
                TaskContext.Instance().PostMessageSimulator.KeyPress(User32.VK.VK_ESCAPE);
            }
        }
        GameCaptureRegion.GameRegion1080PPosClick(858, 45);
        await Delay(1500, ct);
        await ClaimAll(ct);

        // 关闭
        await _returnMainUiTask.Start(ct);
    }

    /// <summary>
    /// 一键领取
    /// </summary>
    /// <returns></returns>
    private async Task<bool> ClaimAll(CancellationToken ct)
    {
        using var ra = CaptureToRectArea();
        var ocrList = ra.FindMulti(RecognitionObject.Ocr(ra.ToRect().CutRightBottom(0.3, 0.2)));
        var hasClaimAll = _textRecognizer.IsClaimAll(ocrList.Select(txt => txt.Text));
        var wt = hasClaimAll
            ? ocrList.FirstOrDefault(txt => _textRecognizer.IsClaim(txt.Text))
              ?? ocrList.FirstOrDefault(txt => _textRecognizer.IsAll(txt.Text))
            : null;
        if (wt != null)
        {
            wt.Click();
            Logger.LogInformation("纪行：{Text}", "一键领取");
            await Delay(1000, ct);
            using var ra2 = CaptureToRectArea();
            if (IsManualSelectionDialog(ra2))
            {
                LogManualSelectionReminder();
                return true;
            }

            if (ra2.Find(ElementRecognition.Get("Primogem", ra2)).IsExist())
            {
                TaskContext.Instance().PostMessageSimulator.KeyPress(User32.VK.VK_ESCAPE);
            }

            return true;
        }
        else
        {
            Logger.LogInformation("纪行：{Text}", "无需领取");
            return false;
        }
    }

    private static bool IsManualSelectionDialog(ImageRegion region)
    {
        if (Bv.IsInPromptDialog(region))
        {
            return true;
        }

        var hasCancelButton = HasDialogButton(region, ElementRecognition.Get("BtnBlackCancel", region))
                              || HasDialogButton(region, ElementRecognition.Get("BtnWhiteCancel", region));
        if (!hasCancelButton)
        {
            return false;
        }

        return HasDialogButton(region, ElementRecognition.Get("BtnBlackConfirm", region))
               || HasDialogButton(region, ElementRecognition.Get("BtnWhiteConfirm", region));
    }

    private static bool HasDialogButton(ImageRegion region, RecognitionObject recognitionObject)
    {
        using var buttonRegion = region.Find(recognitionObject);
        return buttonRegion.IsExist();
    }

    private void LogManualSelectionReminder()
    {
        if (_manualSelectionReminderLogged)
        {
            return;
        }

        _manualSelectionReminderLogged = true;
        Logger.LogWarning("纪行：检测到需手动选择的奖励，请手动处理");
    }
}
