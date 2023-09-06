using Microsoft.EntityFrameworkCore;
using TFGv3Net7.Data;



namespace TFGv3Net7.Tests
{
    public class TestTfgPrimeroContext : TfgPrimeroContext
    {
        public TestTfgPrimeroContext(DbContextOptions<TfgPrimeroContext> options)
            : base(options) { }
    }


}