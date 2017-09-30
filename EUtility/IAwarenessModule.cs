using Aimtec.SDK.Menu;

namespace EUtility
{
    interface IAwarenessModule
    {
        void Load();
        void Unload();
        Menu Config { get; set; }
    }
}
