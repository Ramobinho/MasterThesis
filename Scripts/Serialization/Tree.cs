using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Tree
{
    Vector3 pos;
    public Tree(Vector3 pos) {
        this.pos = pos;
    }

    public Vector3 getPos() {
        return pos;
    }
}

