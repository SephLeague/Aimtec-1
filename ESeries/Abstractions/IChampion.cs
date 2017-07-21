using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aimtec;
using Aimtec.SDK;

using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.Menu;

namespace ESeries.Abstractions
{
    interface IChampion
    {
        Spell Q { get; set; }
        Spell W { get; set; }
        Spell E { get; set; }
        Spell R { get; set; }

        Menu Config { get; set; }
    }
}
