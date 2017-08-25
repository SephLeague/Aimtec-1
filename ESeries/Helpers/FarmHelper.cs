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

            var allPositions = minions.Select(x => x.ServerPosition.To2D()).ToList();

            if (minHit == 1 && allPositions.Any())
            {
                return new LaneclearResult(1, allPositions.FirstOrDefault().To3D());
            }

            var positionCount = allPositions.Count;

            var lcount = Math.Min(positionCount, 5);

            HashSet<LaneclearResult> results = new HashSet<LaneclearResult>();

            void CheckResult(Vector3 c)
            {
                var hitMinions = allPositions.Where(x => x.Distance(c) <= 0.95f * width);

                var count = hitMinions.Count();

                if (count >= minHit)
                {
                    var result = new LaneclearResult(count, c);
                    results.Add(result);
                }
            }

            if (allPositions.Count >= minHit)
            {
                Vector2 center;

                float radius;

                Mec.FindMinimalBoundingCircle(allPositions, out center, out radius);

                if (center.IsZero)
                {
                    return new LaneclearResult(0, Vector3.Zero);
                }

                CheckResult(center.To3D());

                for (int i = 0; i < lcount; i++)
                {
                    for (int j = 0; j < lcount; j++)
                    {    
                        var positions = new List<Vector2> { allPositions[i], allPositions[j] };

                        Mec.FindMinimalBoundingCircle(positions, out center, out radius);

                        CheckResult(center.To3D());
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
