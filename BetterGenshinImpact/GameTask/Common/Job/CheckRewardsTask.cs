using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BetterGenshinImpact.Core.Recognition;
using BetterGenshinImpact.Core.Simulator;
using BetterGenshinImpact.Core.Simulator.Extensions;
using BetterGenshinImpact.GameTask.Common.GameText;
using BetterGenshinImpact.GameTask.Localization;
using BetterGenshinImpact.Helpers;
using BetterGenshinImpact.Service.Notification;
using BetterGenshinImpact.Service.Notification.Model.Enum;
using Microsoft.Extensions.Logging;
using static BetterGenshinImpact.GameTask.Common.TaskControl;


namespace BetterGenshinImpact.GameTask.Common.Job;

/// <summary>
/// 检查奖励并通知的任务
/// </summary>
public class CheckRewardsTask
{
    private readonly ILogger<CheckRewardsTask> _logger = App.GetLogger<CheckRewardsTask>();

    private readonly CommonJobTextRecognizer _textRecognizer;

    public CheckRewardsTask()
    {
        var matcher = App.GetService<IGameTextMatcher>()
                      ?? throw new InvalidOperationException("IGameTextMatcher is not registered.");
        _textRecognizer = new CommonJobTextRecognizer(matcher);
    }

    public string Name => "检查奖励并通知的任务";
    
    private static RecognitionObject GetConfirmRa()
    {
        using var screenArea = CaptureToRectArea();
        var x = (int)(screenArea.Width * 0.1);
        var y = (int)(screenArea.Height * 0.1);
        var width = (int)(screenArea.Width * 0.3);
        var height = (int)(screenArea.Height * 0.7);
        
        return RecognitionObject.Ocr(x, y, width, height);
    }

    public async Task Start(CancellationToken ct)
    {
        try
        {
            await new ReturnMainUiTask().Start(ct);
            
            _ = await OpenDailyCommissionsPage(ct);
            
            // OCR识别每日是否完成
            var done = await NewRetry.WaitForAction(() =>
            {
                using var screen = CaptureToRectArea();
                return screen.FindMulti(GetConfirmRa())
                    .Any(btn => _textRecognizer.IsDailyRewardClaimed(btn.Text));
            }, ct, 4, 500);
            if (done)
            {
                Logger.LogInformation("检查每日奖励结果：{Msg}", "今日奖励已领取");
                Notify.Event(NotificationEvent.DailyReward).Success("检查每日奖励：已领取");
            }
            else
            {
                Logger.LogWarning("检查每日奖励结果：{Msg}，请手动检查！", "未领取");
                Notify.Event(NotificationEvent.DailyReward).Error("检查到每日奖励未领取，请手动查看！");
            }
            await Delay(200, ct);
            await new ReturnMainUiTask().Start(ct);
        }
        catch (Exception e)
        {
            Logger.LogDebug(e, "检查奖励并通知的任务异常");
            Logger.LogError("检查奖励并通知的任务异常: {Msg}", e.Message);
        }
    }

    private async Task<bool> OpenDailyCommissionsPage(CancellationToken ct)
    {
        for (var attempt = 0; attempt < 4; attempt++)
        {
            if (ct.IsCancellationRequested)
            {
                return false;
            }

            Simulation.SendInput.SimulateAction(GIActions.OpenAdventurerHandbook);
            await Delay(1000, ct);

            using var screen = CaptureToRectArea();
            var dailyCommissions = screen.FindMulti(GetConfirmRa())
                .FirstOrDefault(btn => _textRecognizer.IsDailyCommissions(btn.Text));
            if (dailyCommissions is null)
            {
                continue;
            }

            dailyCommissions.Click();
            return true;
        }

        return false;
    }
}
