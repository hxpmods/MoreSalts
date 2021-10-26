using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BasicMod;

namespace MoreSalts
{
    class ScaleSaltBehaviour : SaltBehaviour
    {

        private bool makeBigger;
        private float rate;

        public ScaleSaltBehaviour( float _rate)
        {
            rate = _rate;
        }

        public override void OnCauldronDissolve()
        {
           SaltHelper.scalePathBySalt +=  rate;
        }
    }
}
