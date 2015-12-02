using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sphero.Communication;

namespace SWMS.Core
{
    public class SpheroManager
    {
        public static async Task<Sphero> GetSpheroAsync()
        {
            IEnumerable<SpheroInformation> spheros = await SpheroConnectionProvider.DiscoverSpheros();
            SpheroInformation spheroInfo = spheros.FirstOrDefault();

            if (spheroInfo == null)
            {
                return null;
            }

            SpheroConnection connection = await SpheroConnectionProvider.CreateConnection(spheroInfo);

            if (connection == null)
            {
                return null;
            }
            var spheroDevice = new Sphero(connection);
            return spheroDevice;
        }
    }
}