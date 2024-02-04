using UnityEngine;

public class ContainerModel
{
    private int _identifier;
    public int identifier
    {
        get => _identifier;
        set { _identifier = value; }
    }
    private double _support2D = 0;
    public double support2D
    {
        get => _support2D;
        set { _support2D = value; }
    }
    private double _support3D = 0;
    public double support3D
    {
        get => _support3D;
        set { _support3D = value; }
    }

    private bool _overrideUtility = false; 
    public bool overrideUtility
    {
        get => _overrideUtility;
        set { _overrideUtility = value; }
    }

    private double _utility;
    public double utility
    {
        get => _utility;
        set { _utility = value; }
    }
    private double _visibility;
    public double visibility
    {
        get => _visibility;
        set { _visibility = value; }
    }
    private double _touchSupport;
    public double touchSupport
    {
        get => _touchSupport;
        set { _touchSupport = value; }
    }
    private double _backgroundComplexity;
    public double backgroundComplexity
    {
        get => _backgroundComplexity;
        set { _backgroundComplexity = value; }
    }

    private double _associatedObjUtility; 
    public double associatedObjUtility
    {
        get => _associatedObjUtility; 
        set { _associatedObjUtility = value;  }
    }

    private double _hSize;
    public double hSize
    {
        get => _hSize;
        set { _hSize = value; }
    }
    private double _vSize;
    public double vSize
    {
        get => _vSize;
        set { _vSize = value; }
    }

    private double _x, _xAbs;
    public double x
    {
        get => _x;
        set
        {
            _x = value;
            _xAbs = Mathf.Abs((float)value);
        }
    }
    public double xAbs
    {
        get => _xAbs;
    }

    private double _y, _yAbs;
    public double y
    {
        get => _y;
        set
        {
            _y = value;
            _yAbs = Mathf.Abs((float)value);
        }
    }
    public double yAbs
    {
        get => _yAbs;
    }

    private double _z, _zAbs;
    public double z
    {
        get => _z;
        set
        {
            _z = value;
            _zAbs = Mathf.Abs((float)value);
        }
    }
    public double zAbs
    {
        get => _zAbs;
    }

    private double _dist;
    public double dist
    {
        get => _dist;
        set { _dist = value; }
    }

    private double _fromForward;
    public double fromForward
    {
        get => _fromForward;
        set { _fromForward = value; }
    }

    private double[] _objects;
    public double[] objects
    {
        get => _objects;
        set { _objects = value; }
    }
    
}
