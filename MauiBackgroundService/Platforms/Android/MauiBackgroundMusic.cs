using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiBackgroundService.Platforms.Android
{

    [Service(Exported = false, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class MauiBackgroundMusic : Service
    {
        private MediaPlayer? _mediaPlayer;
        private const string CHANNEL_ID = "music_service_channel";
        private const int NOTIFICATION_ID = 1001;
        private bool _isPlaying = false;
        public override void OnCreate()
        {
            base.OnCreate();
            _mediaPlayer = MediaPlayer.Create(this, Resource.Raw.sample_music);
            _mediaPlayer.Looping = true;

            // Gọi StartForeground ngay khi Service bắt đầu
            StartForeground(NOTIFICATION_ID, BuildNotification());
        }
        //public override void OnTaskRemoved(Intent? rootIntent)
        //{
        //    StopSelf(); // Dừng service khi ứng dụng bị tắt
        //    base.OnTaskRemoved(rootIntent);
        //}
        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == "ACTION_PLAY_PAUSE")
            {
                TogglePlayback();
            }
            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            _mediaPlayer?.Stop();
            _mediaPlayer?.Release();
            _mediaPlayer = null;
            _isPlaying = false;
            base.OnDestroy();
        }
        private void TogglePlayback()
        {
            if (_isPlaying)
            {
                _mediaPlayer?.Pause();
                _isPlaying = false;
            }
            else
            {
                _mediaPlayer?.Start();
                _isPlaying = true;
            }

            // Cập nhật Notification khi Play/Pause thay đổi
            UpdateNotification();
        }
        private void UpdateNotification()
        {
            NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Notify(NOTIFICATION_ID, BuildNotification());
        }
        public override IBinder? OnBind(Intent? intent) => null;

        //private Notification BuildNotification()
        //{
        //    var notificationIntent = new Intent(this, typeof(MainActivity));
        //    var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

        //    return new NotificationCompat.Builder(this, CHANNEL_ID)
        //        .SetSmallIcon(Resource.Drawable.ic_arrow_back_black_24)
        //        .SetContentTitle("Đang phát nhạc")
        //        .SetContentText("Ứng dụng đang phát nhạc trong nền.")
        //        .SetContentIntent(pendingIntent)
        //        .SetPriority(NotificationCompat.PriorityHigh)
        //        .SetOngoing(true) // Giữ thông báo luôn hiển thị
        //        .Build();
        //}

        private Notification BuildNotification()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            // Intent xử lý nút Play/Pause
            var playPauseIntent = new Intent(this, typeof(MusicReceiver));
            playPauseIntent.SetAction("ACTION_PLAY_PAUSE");

            var playPausePendingIntent = PendingIntent.GetBroadcast(this, 0, playPauseIntent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            // Chọn icon nút Play/Pause
            int icon = _isPlaying ? Resource.Drawable.avd_hide_password : Resource.Drawable.avd_show_password;
            string text = _isPlaying ? "Pause" : "Play";

            return new NotificationCompat.Builder(this, CHANNEL_ID)
                .SetSmallIcon(Resource.Drawable.ic_arrow_back_black_24)
                .SetContentTitle("Đang phát nhạc")
                .SetContentText("Ứng dụng đang phát nhạc trong nền.")
                .SetContentIntent(pendingIntent)
                .AddAction(icon, text, playPausePendingIntent) // Nút Play/Pause
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetOngoing(true)
                .Build();
        }
    }
}
