namespace Magician.Geo;
using Core.Maps;
using Magician.Core;

public class Implicit : NodeMeshed, IRelation
{
    double[] range;
    double[] resolution;
    IRelation relation;
    public Implicit(IRelation r, double x, double y, double z, double scale, params (double, double)[] rangeResolutionPairs) : base(x, y, z)
    {
        relation = r;
        if (rangeResolutionPairs.Length != Ins)
            throw Scribe.Error($"Number of range-resolution pairs did not match length of inputs ({Ins})");
        range      = rangeResolutionPairs.Select(t => t.Item1).ToArray();
        resolution = rangeResolutionPairs.Select(t => t.Item2).ToArray();
        CreateMesh();
    }

    public IVar Cache => relation.Cache;

    public int Ins => relation.Ins;

    public IVal Evaluate(params double[] args)
    {
        return relation.Evaluate(args);
    }

    public void CreateMesh()
    {
        faces = Mesh.Square((int)(resolution[0]/range[0]), (int)(resolution[1]/range[1]));
    }
}