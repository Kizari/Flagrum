using Ebony.Base.Serialization;
using System.Collections.Generic;

namespace SQEX.Ebony.Framework.Entity
{
    public class EntityPackageCreatingResult
    {
        public IList<ExternalPointerInfo> ExternalPointerInfos { get; } = new List<ExternalPointerInfo>();
        private IList<string> DependTypeNames { get; } = new List<string>();
    }
}
