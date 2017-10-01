using Aimtec.SDK.Menu;

namespace Ewareness
{
    interface IAwarenessModule
    {
        void Load();
        void Unload();
        Menu Config { get; set; }
    }
}
