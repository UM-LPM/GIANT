using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PhysicsOverlapResult
{
    public bool HasHit {  get; set; }
    public virtual Component[] GetColliders() {  return null; }
}

public class PhysicsOverlapResult3D : PhysicsOverlapResult
{
    public Collider[] HitColliders3D { get; set; }

    public override Component[] GetColliders()
    {
        return HitColliders3D;
    }
}

public class PhysicsOverlapResult2D : PhysicsOverlapResult
{
    public Collider2D[] HitColliders2D { get; set; }

    public override Component[] GetColliders()
    {
        return HitColliders2D;
    }
}