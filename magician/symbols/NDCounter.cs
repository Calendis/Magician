namespace Magician.Symbols;

public class NDCounter
{

    double[] mins;
    double[] maxs;
    double counterMax = 1;
    public double Max => counterMax;
    int[] vals;
    public int Val {get; private set;}
    public double Get(int n) => vals[n] * ress[n];// + mins[n];
    public int[] Positional => vals;
    double[] ress;
    bool done = false;
    public bool Done => done;
    public int Dims {get; set;}
    public double AxisLen(int axis)
    {
            if (axis >= maxs.Length)
            {
                return 0;
            }
        return maxs[axis] / ress[axis];
    }

    public NDCounter(params (double, double, double)[] ranges)
    {
        Dims = ranges.Length;
        ress = new double[Dims];
        mins = new double[Dims]; maxs = new double[Dims]; vals = new int[Dims];

        int i = 0;
        foreach ((double, double, double) t in ranges)
        {
            mins[i] = t.Item1;
            vals[i] = 0;
            maxs[i] = t.Item2;
            ress[i] = t.Item3;
            counterMax *= (maxs[i] - mins[i]) / ress[i] + 1;
            i++;
        }
        counterMax = (int)counterMax;
        Scribe.Warn(counterMax);
    }

    public bool Increment()
    {
        bool foundAvailableSlot = false;
        int slot = 0;
        int carry = 0;
        while (!foundAvailableSlot)
        {
            if (vals[slot] < maxs[slot]/ress[slot] - 1)
            {
                foundAvailableSlot = true;
                vals[slot]++;
                for (int i = 0; i < carry; i++)
                {
                    vals[slot-carry] = 0;
                }
            }
            else
            {
                vals[slot] = 0;
                carry++;
            }
            
            slot++;
            if (slot == vals.Length)
            {
                foundAvailableSlot = true;
            }
        }
        //Scribe.List(vals);
        
        Val ++;
        if (Val >= counterMax)
        {
            done = true;
            return true;
        }

        return false;
    }
}