namespace Magician.Demos.Tests;

using Magician.Core.Caster;
using Magician.Geo;

public class TreeCache : Spell
{
    public override void Loop()
    {
        //throw new NotImplementedException();
    }

    public override void PreLoop()
    {
        // Test capability to cache simple Nodes
        Node myStar = Create.Star(5, 55, 118);
        Origin["star"] = myStar;
        
        // Test capability to cache Nodes with faces
        //Node myCube = Create.Cube(0, 0, 0, 50);
        //Origin["cube"] = myCube;
    }

    Node RandNode(Node? parent=null)
    {
        return new Node(0, 0, 0).Parented(parent ?? Ref.Origin);
    }
}