public class PlayableTankWithTrackC : PlayableTank
{
    protected override void PlayTrack()
    {
        foreach (var t in tracks_)
        {
            t.enabled = true;
            t.Play("TrackC");
        }
    }
}