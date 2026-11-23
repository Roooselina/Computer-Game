// The previous whole-scene illustration workflow is retired. Keep the entry point safe for old tooling.
namespace EW.EditorTools
{
    public static class IllustratedArtSetup
    {
        public static void Apply(Ctx context) { }
        public static string Rebuild() => PixelArtRestore.Restore();
    }
}
