using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BasicMod;

namespace MoreSalts
{
    class ExperienceSaltBehaviour : SaltBehaviour
    {
        float distanceToCoverOnDissolve = 0.05f;
        public override void OnCauldronDissolve()
        {
            MoreSalts.moveToNearestExperienceBySalt += distanceToCoverOnDissolve;
        }
    }
}
