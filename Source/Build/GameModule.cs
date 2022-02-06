namespace JanusBuildTool
{
    public abstract class GameModule : Module
    {
        public override void SetUp(BuildOptions options)
        {
            base.SetUp(options);
            options.PublicDependencies.Add("Core");
        }
    }
}