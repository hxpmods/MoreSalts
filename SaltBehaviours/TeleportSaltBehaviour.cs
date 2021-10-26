using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BasicMod;

namespace MoreSalts
{
    class TeleportSaltBehaviour : SaltBehaviour
    {

        private float rate;

        public TeleportSaltBehaviour( float _rate)
        {
            rate = _rate;
        }

        public override void OnCauldronDissolve()
        {
           SaltHelper.changePathToTeleport +=  rate;
        }
    }
}
