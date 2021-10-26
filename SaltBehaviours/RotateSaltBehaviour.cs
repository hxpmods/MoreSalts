using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BasicMod;

namespace MoreSalts
{
    class RotateSaltBehaviour : SaltBehaviour
    {

        private float rate;

        public RotateSaltBehaviour( float _rate)
        {
            rate = _rate;
        }

        public override void OnCauldronDissolve()
        {
           SaltHelper.rotatePathBySalt +=  rate;
        }
    }
}
