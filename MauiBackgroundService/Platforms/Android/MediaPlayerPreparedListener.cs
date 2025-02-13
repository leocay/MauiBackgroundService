using Android.Media;

namespace MauiBackgroundService.Platforms.Android;

public class MediaPlayerPreparedListener : Java.Lang.Object, MediaPlayer.IOnPreparedListener
{
    private readonly MauiBackgroundMusic _service;

    public MediaPlayerPreparedListener(MauiBackgroundMusic service)
    {
        _service = service;
    }

    public void OnPrepared(MediaPlayer mp)
    {
        Console.WriteLine("🎵 MediaPlayer đã sẵn sàng để phát!");
        mp.Start();  // Tự động phát nhạc khi đã sẵn sàng (nếu muốn)
        //_service.StartSeekBarUpdater(); // Bắt đầu cập nhật SeekBar
    }
}

