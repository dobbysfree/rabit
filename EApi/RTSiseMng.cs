using Akka.Actor;
using Rabit.Helpers;
using Rabit.Info;
using System.Linq;

namespace Rabit.Comm
{
    public class RTSiseMng : UntypedActor
    {
        #region Actor Receiver
        protected override void OnReceive(object obj)
        {

        }
        #endregion

        #region Request
        public RTSiseMng()
        {            
            for (int i = 0; i < Data.Items.Count; i++)
            {
                var im = Data.Items.ElementAt(i).Value;

                Context.ActorOf(Props.Create(() => new RTsise(im)), im.Code);
                Tool.Delay(300);
            }
            Conf.ILog.Information("Request RT Sise > " + Data.Items.Count);
        }
        #endregion
    }
}