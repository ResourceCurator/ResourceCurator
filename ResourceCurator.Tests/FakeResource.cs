using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceCurator.Tests
{
    public class FakeIResource:IResource
    {
        public string Name { get; internal set; }
        public string ProducerHash { get; internal set; }
        public object UntypedValue { get; internal set; }
    }
}
