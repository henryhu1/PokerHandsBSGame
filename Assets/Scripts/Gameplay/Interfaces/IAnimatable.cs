using System.Collections;

public interface IAnimatable
{
    // TODO: migrate to Tweens
    // public IEnumerator DoAnimation();
    public void StartAnimation();
    public void StopAnimation();
}
