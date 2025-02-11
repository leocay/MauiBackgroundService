using Android.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBackgroundService.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class MusicReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (context == null || intent == null) return;

            string action = intent.Action ?? "";
            if (action == "ACTION_PLAY_PAUSE")
            {
                Intent serviceIntent = new(context, typeof(MauiBackgroundMusic));
                serviceIntent.SetAction("ACTION_PLAY_PAUSE");
                context.StartService(serviceIntent);
            }
        }
    }
}
