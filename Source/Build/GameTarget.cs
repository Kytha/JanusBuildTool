namespace JanusBuildTool
{
    public abstract class GameTarget : ProjectTarget
    {
        public override void Init()
        {
            base.Init();
            Modules.Add("Core");
        }
    }
}