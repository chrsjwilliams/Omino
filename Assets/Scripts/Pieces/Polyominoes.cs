using UnityEngine;

public abstract class Polyominoes : MonoBehaviour
{

    protected int variations;
    protected int width;
    protected int length;
    protected int[,,] piece;

    protected abstract void Start();

    public abstract void GenerateTemplate();
    public abstract void InitTemplate();
    public abstract void Create(int index);
}
