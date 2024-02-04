using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElementModel
{
    private int _identifier;
    public int identifier
    {
        get => _identifier;
        set { _identifier = value; } 
    }
    private double _type2D = 0;
    public double type2D
    {
        get => _type2D;
        set { _type2D = value;  }
    }
    private double _type3D = 0;
    public double type3D
    {
        get => _type3D;
        set { _type3D = value;  }
    }

    private double _utility;
    public double utility
    {
        get => _utility;
        set { _utility = value; }
    }
    private double _visReq;
    public double visReq
    {
        get => _visReq;
        set { _visReq = value; }
    }
    private double _touchReq;
    public double touchReq
    {
        get => _touchReq;
        set { _touchReq = value; }
    }
    private double _backgroundTol;
    public double backgroundTol
    {
        get => _backgroundTol;
        set { _backgroundTol = value; }
    }

    private double _hSize;
    public double hSize
    {
        get => _hSize;
        set { _hSize = value; }
    }
    private int _hVoxels;
    public int hVoxels
    {
        get => _hVoxels;
        set { _hVoxels = value; }
    }
    private double _vSize;
    public double vSize
    {
        get => _vSize;
        set { _vSize = value; }
    }
    private int _vVoxels;
    public int vVoxels
    {
        get => _vVoxels;
        set { _vVoxels = value; }
    }
    private double _minScale;
    public double minScale
    {
        get => _minScale;
        set { _minScale = value; }
    }
    private double _maxScale;
    public double maxScale
    {
        get => _maxScale;
        set { _maxScale = value; }
    }

    private double _optimizedScale;
    public double optimizedScale
    {
        get => _optimizedScale;
        set { _optimizedScale = value; }
    }
    private Vector3 _scale;
    public Vector3 scale
    {
        get => _scale;
        set { _scale = value; }
    }

    private double _x, _xAbs;
    public double x
    {
        get => _x;
        set {
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
        set {
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
        set {
            _z = value;
            _zAbs = Mathf.Abs((float)value);
        }
    }
    public double zAbs
    {
        get => _zAbs;
    }

    private Vector3 _forward;
    public Vector3 forward
    {
        get => _forward;
        set
        {
            _forward = value;
        }
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

    private double[] _anchors;
    public double[] anchors
    {
        get => _anchors;
        set { _anchors = value; }
    }

    private double[] _avoidances;
    public double[] avoidances
    {
        get => _avoidances;
        set { _avoidances = value; }
    }
    
}
