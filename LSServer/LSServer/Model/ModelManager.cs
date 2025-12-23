using LSServer.Object;
using LSServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSServer.Model
{
    internal class ModelManager : Singleton<ModelManager>
    {
        public GameModel game;

        public void Init()
        {
            game = new GameModel();
        }
    }
}
