using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwinCAT.Ads;

namespace KpHiteBeckHoff.Model
{
    public class AmsRouterSettings
    {
        public RouterSettings AmsRouter { get; set; }
    }

    public class RouterSettings
    {
        private RouteConfig[] _routes = new RouteConfig[0];

        private string _localName = string.Empty;

        private AmsNetId _localNetId = AmsNetId.Empty;

        private string _loopbackIP = "";

        private int _loopbackPort = 48898;

        private IPConfig[] _loopbackExternals = new IPConfig[0];

        private string _loopbackExternalsSubnet = string.Empty;

        private int _tcpPort = 48898;

        //
        // 摘要:
        //     Gets or sets the list of remote Routes/Connections.
        //
        // 值:
        //     The remote connections.
        public RouteConfig[] RemoteConnections
        {
            get
            {
                return _routes;
            }
            set
            {
                _routes = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the NetID if the local System.
        //
        // 值:
        //     The net identifier.
        public AmsNetId NetId
        {
            get
            {
                return _localNetId;
            }
            set
            {
                _localNetId = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the name of the local System.
        //
        // 值:
        //     The name.
        public string Name
        {
            get
            {
                return _localName;
            }
            set
            {
                _localName = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the IPAddress used for local communication (Loopback ip).
        //
        // 值:
        //     The loopback ip.
        public string LoopbackIP
        {
            get
            {
                return _loopbackIP;
            }
            set
            {
                _loopbackIP = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the TCP Port that is used for the loopback.
        //
        // 值:
        //     The loopback port.
        public int LoopbackPort
        {
            get
            {
                return _loopbackPort;
            }
            set
            {
                _loopbackPort = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the loopback externals.
        //
        // 值:
        //     The loopback externals.
        //
        // 言论：
        //     The Loopback externals are IPAddresses, that are allowed to use the Loopback
        //     connection.
        public IPConfig[] LoopbackExternalIPs
        {
            get
            {
                return _loopbackExternals;
            }
            set
            {
                _loopbackExternals = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the loopback externals subnet.
        //
        // 值:
        //     The loopback externals subnet.
        //
        // 言论：
        //     This is an alternative approach to set the allowed 'LoopbackIPs' for loopback
        //     communication. In docker/virtual enviroments often a whole subnet will be spanned
        public string LoopbackExternalSubnet
        {
            get
            {
                return _loopbackExternalsSubnet;
            }
            set
            {
                _loopbackExternalsSubnet = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the TCP port used for external communication.
        //
        // 值:
        //     The TCP port.
        public int TcpPort
        {
            get
            {
                return _tcpPort;
            }
            set
            {
                _tcpPort = value;
            }
        }
    }

    public class IPConfig
    {
        private string _address = string.Empty;

        public string IP
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
            }
        }
    }

    public class RouteConfig
    {
        private string _name = string.Empty;

        private string _address = string.Empty;

        private AmsNetId _netId = AmsNetId.Empty;

        private string _type = string.Empty;

        //
        // 摘要:
        //     Gets or sets the name of the Route
        //
        // 值:
        //     The name.
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the address of the Route (HostName or IPAddress).
        //
        // 值:
        //     The address.
        public string Address
        {
            get
            {
                return _address;
            }
            set
            {
                _address = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the NetID of the route
        //
        // 值:
        //     The net identifier.
        public AmsNetId NetId
        {
            get
            {
                return _netId;
            }
            set
            {
                _netId = value;
            }
        }

        //
        // 摘要:
        //     Gets or sets the type of the route.
        //
        // 值:
        //     The type.
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }
    }
}
