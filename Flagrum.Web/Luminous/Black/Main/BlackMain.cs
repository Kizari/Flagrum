using Black.Entity;

namespace Black.Main
{
    //[UnityEditor.InitializeOnLoad]
    public static class BlackMain
    {
        /*static BlackMain()
        {
            Initialize();
        }*/

        public static void Initialize()
        {
            EntityModuleSetup.Setup();
        }
    }
}