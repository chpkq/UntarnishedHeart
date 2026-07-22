using Dalamud.Interface.Windowing;
using UntarnishedHeart.Execution.Common;
using UntarnishedHeart.Execution.ExecuteAction;
using UntarnishedHeart.Execution.Managers;
using UntarnishedHeart.Execution.Preset;
using UntarnishedHeart.Execution.Preset.Enums;

namespace UntarnishedHeart.Windows;

public class RouteCurrentPresetWindow() : Window($"当前预设步骤###{Plugin.PLUGIN_NAME}-RouteCurrentPresetWindow", ImGuiWindowFlags.AlwaysAutoResize)
{
    public override void Draw()
    {
        var routeExecutor = ExecutionManager.RouteExecutor;
        if (routeExecutor is not { IsRunning: true })
        {
            IsOpen = false;
            return;
        }

        var presetExecutor = routeExecutor.CurrentExecutor;
        if (presetExecutor is not { Completion.IsCompleted: false, ExecutorPreset: not null })
        {
            ImGui.TextDisabled("等待路线执行预设步骤");
            return;
        }

        var preset = presetExecutor.ExecutorPreset;
        var cursor = presetExecutor.Progress.RuntimeCursor;

        ImGui.TextColored(KnownColor.LightSkyBlue.ToUInt(), preset.Name);
        ImGui.TextDisabled($"共 {preset.Steps.Count} 个步骤");
        ImGui.Separator();

        using var child = ImRaii.Child("RouteCurrentPresetSteps", new(420f * GlobalUIScale, 360f * GlobalUIScale), true);
        if (!child)
            return;

        foreach (var (step, stepIndex) in preset.Steps.Select(static (value, index) => (value, index)))
        {
            var isRunning = cursor.HasStep && cursor.StepIndex == stepIndex;
            if (isRunning)
                ImGui.TextColored(KnownColor.LimeGreen.ToUInt(), $"> {stepIndex + 1}. {step.Name}");
            else
                ImGui.TextUnformatted($"  {stepIndex + 1}. {step.Name}");

            DrawPhase(step.EnterActions, PresetStepPhase.Enter, cursor, stepIndex, "进入");
            DrawPhase(step.BodyActions,  PresetStepPhase.Body,  cursor, stepIndex, "进行");
            DrawPhase(step.ExitActions,  PresetStepPhase.Exit,  cursor, stepIndex, "离开");
            ImGui.Spacing();
        }
    }

    private static void DrawPhase
    (
        List<ExecuteActionBase>    actions,
        PresetStepPhase            phase,
        ExecuteActionRuntimeCursor cursor,
        int                        stepIndex,
        string                     phaseName
    )
    {
        if (actions.Count == 0)
            return;

        var isRunning = cursor.HasPhase && cursor.StepIndex == stepIndex && cursor.Phase == phase;
        ImGui.TextDisabled($"    {phaseName}阶段 ({actions.Count} 个动作){(isRunning ? " ←" : string.Empty)}");

        foreach (var (action, actionIndex) in actions.Select(static (value, index) => (value, index)))
        {
            var isActionRunning = cursor.HasAction && cursor.StepIndex == stepIndex && cursor.Phase == phase && cursor.ActionIndex == actionIndex;
            if (isActionRunning)
                ImGui.TextColored(KnownColor.LimeGreen.ToUInt(), $"      > {action.Name}");
            else
                ImGui.TextDisabled($"        {action.Name}");
        }
    }
}
