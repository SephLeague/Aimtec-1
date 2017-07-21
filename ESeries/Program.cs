using Aimtec.SDK.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aimtec;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ESeries.Abstractions;
using Aimtec.SDK.Events;

namespace ESeries
{
    class Program
    {
        public static Menu RootMenu { get; set; }

        private static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();


        static void Main(string[] args)
        {
            GameEvents.GameStart += GameEvents_GameStart;
        }

        private static void GameEvents_GameStart()
        {
            var typec = Type.GetType("ESeries.Champions." + Player.ChampionName);

            if (typec == null)
            {
                Console.WriteLine("[ESeries] Champion not supported.");
                return;
            }

            var p = (AChampion) DynamicInitializer.NewInstance(typec);
        }

        public class DynamicInitializer
        {
            public static V NewInstance<V>() where V : class
            {
                return ObjectGenerator(typeof(V)) as V;
            }

            private static object ObjectGenerator(Type type)
            {
                var target = type.GetConstructor(Type.EmptyTypes);
                var dynamic = new DynamicMethod(string.Empty,
                              type,
                              new Type[0],
                              target.DeclaringType);
                var il = dynamic.GetILGenerator();
                il.DeclareLocal(target.DeclaringType);
                il.Emit(OpCodes.Newobj, target);
                il.Emit(OpCodes.Stloc_0);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ret);

                var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
                return method();
            }

            public static object NewInstance(Type type)
            {
                return ObjectGenerator(type);
            }
        }

    }
}
