using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BasicMod;

namespace MoreSalts
{
    class CardinalSaltBehaviour : SaltBehaviour
    {

        private Vector2 dir;
        private float rate;

        public CardinalSaltBehaviour(Vector2 _dir, float _rate)
        {
            dir = _dir;
            rate = _rate;
        }

        public override void OnCauldronDissolve()
        {
            MoreSalts.moveToCardinalsBySalt += dir * rate;
        }
    }
}
