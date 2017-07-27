using Aimtec;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESeries.Helpers
{
    public class FarmHelper
    {
        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();

        public class LaneclearResult
        {
            public LaneclearResult(int hit, Vector3 cp)
            {
                this.numberOfMinionsHit = hit;
                this.CastPosition = cp;
            }

            public int numberOfMinionsHit = 0;
            public Vector3 CastPosition;
        }

        public static LaneclearResult GetCircularClearLocation(float range, float width, int minHit)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidSpellTarget(range));
            var positions = minions.Select(x => x.ServerPosition.To2D()).ToList();

            if (positions.Any())
            {
                return new LaneclearResult(1, positions.FirstOrDefault().To3D());
            }

            var positionCount = positions.Count;

            var lcount = Math.Max(positionCount, 4);

            if (positions.Count >= minHit)
            {
                Vector2 center;
                float radius;

                Mec.FindMinimalBoundingCircle(positions, out center, out radius);

                HashSet<LaneclearResult> results = new HashSet<LaneclearResult>();

                var hitMinions = minions.Where(x => x.Distance(center) <= 0.95f * radius);

                var count = hitMinions.Count();

                var result = new LaneclearResult(count, center.To3D());

                results.Add(result);

                for (int i = 0; i < lcount; i++)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        var positions2 = new Vector2[] { positions[i], positions[j] };

                        Mec.FindMinimalBoundingCircle(positions, out center, out radius);

                        hitMinions = minions.Where(x => x.Distance(center) <= 0.9f * radius);

                        count = hitMinions.Count();

                        if (count >= minHit)
                        {
                            results.Add(new LaneclearResult(count, center.To3D()));
                        }
                    }
                }

                return results.MaxBy(x => x.numberOfMinionsHit);
            }

            return null;
        }

        public static LaneclearResult GetLineClearLocation(float range, float width)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidSpellTarget(range));

            var positions = minions.Select(x => x.ServerPosition).ToList();

            var locations = new List<Vector3>();

            locations.AddRange(positions);

            var max = positions.Count();

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (positions[j] != positions[i])
                    {
                        locations.Add((positions[j] + positions[i]) / 2);
                    }
                }
            }

            HashSet<LaneclearResult> results = new HashSet<LaneclearResult>();

            foreach (var p in locations)
            {
                var rect = new Rectangle(Player.Position, p, width);

                var count = 0;

                foreach (var m in minions)
                {
                    if (rect.Contains(m.Position))
                    {
                        count++;
                    }
                }

                results.Add(new LaneclearResult(count, p));
            }

            var maxhit = results.MaxBy(x => x.numberOfMinionsHit);

            return maxhit;
        }

    }
}
