using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Plugin
{
    class MumbleClient : MumbleConnection
    {
        public void Connect()
        {
            base.Connect();

            SendVersion("LeChuck Mumble Plugin");
            SendAuthenticate();
        }
    }
}
