public class PlayableTankWithTrackD : PlayableTank
{
    protected override void PlayTrack()
    {
        foreach (var t in tracks_)
        {
            t.enabled = true;
            t.Play("TrackD");
        }
    }
}