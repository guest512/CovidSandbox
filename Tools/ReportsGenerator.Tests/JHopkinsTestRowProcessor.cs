using ReportsGenerator.Model.Processors;

namespace ReportsGenerator.Tests
{
    public class JHopkinsTestRowProcessor : JHopkinsRowProcessor
    {
        public JHopkinsTestRowProcessor():base("../Data/Misc", new NullLogger())
        {
        }

    }
}