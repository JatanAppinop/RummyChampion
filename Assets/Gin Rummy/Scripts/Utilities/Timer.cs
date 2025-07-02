using System;
using DG.Tweening;

/// <summary>
/// Class runs delayed actions
/// </summary>
public class Timer
{
    Sequence sequence;

    public Timer(float time, Action finishAction)
    {
        RestartTimer(time, finishAction);
    }

    public void RestartTimer(float time, Action finishAction)
    {
        Kill();
        sequence = DOTween.Sequence().PrependInterval(time).AppendCallback(() => finishAction.RunAction());

    }

    public void Kill()
    {
        if(sequence != null)
            sequence.Kill();

    }
}
