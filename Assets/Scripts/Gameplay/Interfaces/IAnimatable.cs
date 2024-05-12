using System.Collections;

public interface IAnimatable
{
    public IEnumerator DoAnimation();
    public void StartAnimation();
    public void StopAnimation();
}
