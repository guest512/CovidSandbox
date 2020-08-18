using CovidSandbox.Model.Processors;

namespace CovidSandbox.Tests
{
    public class JHopkinsTestRowProcessor : JHopkinsRowProcessor
    {
        public JHopkinsTestRowProcessor():base("../Data/Misc")
        {
        }

    }
}