using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Android.Support.V4.Media;
using Android.Support.V4.Media.Session;
using AndroidX.Core.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.App.Notification;
using static Android.Text.Style.TtsSpan;

namespace MauiBackgroundService.Platforms.Android
{

    [Service(Exported = false, ForegroundServiceType = ForegroundService.TypeMediaPlayback)]
    public class MauiBackgroundMusic : Service
    {
        private MediaPlayer? _mediaPlayer;
        private bool _isPlaying = false;
        private MediaSessionCompat? _mediaSession; //Quản lý chung
        private PlaybackStateCompat.Builder? _stateBuilder; //Trạng thái phát
        private MediaMetadataCompat.Builder? _dataBuilders; //Thông tin bài hát
        int myId = (new object()).GetHashCode();
        private const string CHANNEL_ID = "backgroundServiceChannel";
        public override void OnCreate()
        {
            base.OnCreate();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
#pragma warning disable CA1416
                var serviceChannel =
                    new NotificationChannel(CHANNEL_ID,
                        "Background Service Channel",
                    NotificationImportance.High);

                if (GetSystemService(NotificationService)
                    is NotificationManager manager)
                {
                    manager.CreateNotificationChannel(serviceChannel);
                }
#pragma warning restore CA1416
            }

            _mediaSession = new MediaSessionCompat(this, "MediaSession");
            _mediaSession.Active = true;

            try
            {
                _mediaPlayer = MediaPlayer.Create(this, Resource.Raw.sample_music);
                if (_mediaPlayer == null) throw new Exception("Không thể tạo MediaPlayer");
                _mediaPlayer.Looping = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ MediaPlayer Error: {ex.Message}");
            }

            _dataBuilders = new MediaMetadataCompat.Builder();
            _dataBuilders.PutLong(MediaMetadataCompat.MetadataKeyDuration, _mediaPlayer!.Duration);

            _stateBuilder = new PlaybackStateCompat.Builder()
                .SetState(PlaybackStateCompat.StatePlaying, 0, 1.0f)!
                .SetActions(
                PlaybackStateCompat.ActionPlay |
                PlaybackStateCompat.ActionPause |
                PlaybackStateCompat.ActionStop |
                PlaybackStateCompat.ActionSkipToNext |
                PlaybackStateCompat.ActionSkipToPrevious |
                PlaybackStateCompat.ActionSeekTo
            );

            _mediaSession.SetMetadata(_dataBuilders.Build());
            _mediaSession.SetPlaybackState(_stateBuilder!.Build());
            _mediaSession.SetCallback(new MediaSessionCallback(this));

            Intent intentx = new Intent(this, typeof(MainActivity));
            intentx.SetFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);
            PendingIntent pendingIntent = PendingIntent.GetActivity(this, 0, intentx, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)!;

            var builder = new NotificationCompat.Builder(this, "backgroundServiceChannel")
                        .SetContentIntent(pendingIntent)
                        .SetContentTitle("hello")
                        .SetSmallIcon(Resource.Drawable.abc_ab_share_pack_mtrl_alpha)
                        .SetVisibility(NotificationCompat.VisibilityPublic)
                        .SetStyle(new AndroidX.Media.App.NotificationCompat.MediaStyle()?
                                .SetMediaSession(_mediaSession.SessionToken)?
                                .SetShowActionsInCompactView(1))
                        .Build();

            StartForeground(myId, builder);
        }
        public override void OnTaskRemoved(Intent? rootIntent)
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Stop();
                    _mediaPlayer.Release();
                    _mediaPlayer = null;
                }
                if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
                {
                    StopForeground(StopForegroundFlags.Remove);
                }
                else
                {
                    StopForeground(true);
                }

                if (_mediaSession != null)
                {
                    _mediaSession.Release();
                    _mediaSession = null;
                }

                StopSelf();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Error stopping service: {ex.Message}");
            }

            base.OnTaskRemoved(rootIntent);
        }
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


            //UpdatePlaybackState(); // 🔥 Đảm bảo cập nhật trạng thái trước khi tạo thông báo

            // 🔥 Thay vì tạo thông báo mới, chỉ cần cập nhật
            //StartForeground(1, BuildNotification());
        }

        public override IBinder? OnBind(Intent? intent) => null;

        public class MediaSessionCallback : MediaSessionCompat.Callback
        {
            private MauiBackgroundMusic? services;
            public MediaSessionCallback(MauiBackgroundMusic? mediaPlayer)
            {
                services = mediaPlayer;
            }
            public override void OnPlay()
            {

                services!._mediaPlayer!.Start();
                services!.UpdatePlaybackState(PlaybackStateCompat.StatePlaying,services._mediaPlayer.CurrentPosition);
            }

            public override void OnPause()
            {

                services!._mediaPlayer!.Pause();
                services!.UpdatePlaybackState(PlaybackStateCompat.StatePaused, services._mediaPlayer.CurrentPosition);
            }

            public override void OnSeekTo(long ps)
            {
                services!._mediaPlayer!.SeekTo((int)ps);
                services!.UpdatePlaybackState(PlaybackStateCompat.StatePlaying, ps);  
            }
        }

        public void UpdatePlaybackState(int state, long position)
        {
            _stateBuilder = new PlaybackStateCompat.Builder()?
                .SetState(state, position, 1.0f)?
                .SetActions(
                    PlaybackStateCompat.ActionPlay |
                    PlaybackStateCompat.ActionPause |
                    PlaybackStateCompat.ActionStop |
                    PlaybackStateCompat.ActionSkipToNext |
                    PlaybackStateCompat.ActionSkipToPrevious |
                    PlaybackStateCompat.ActionSeekTo
                );

            _mediaSession?.SetPlaybackState(_stateBuilder?.Build());
        }
     
    }
}
