using Aimtec.SDK.Menu;

namespace Ewareness
{
    interface IAwarenessModule
    {
        void Load(Menu rootMenu);
        void Unload();
        Menu Config { get; set; }
    }
}
