using ReportsGenerator.Model.Processors;
using ReportsGenerator.Utils;

namespace ReportsGenerator.Tests
{
    public class JHopkinsTestRowProcessor : JHopkinsRowProcessor
    {
        public JHopkinsTestRowProcessor() : base("../Data/Misc", new NullLogger())
        {
        }
    }
}